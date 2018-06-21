using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class CommandManager
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly GuildHelper _guildhelper;
        private readonly GuildHandler _guildhandler;
        private readonly WebhookService _webhookservice;
        private readonly ConfigHandler _confighandler;
        private CancellationTokenSource _cancellationToken;
        private Random _random;

        //private readonly IChessService _chessService;
        //private readonly ConfigDatabase _manager;
        private readonly IServiceProvider _provider;

        public CommandManager(IServiceProvider provider)
        {
            _provider = provider;
            _discord = _provider.GetService<DiscordSocketClient>();
            _commands = _provider.GetService<CommandService>();
            _guildhelper = _provider.GetService<GuildHelper>();
            _guildhandler = _provider.GetService<GuildHandler>();
            _webhookservice = _provider.GetService<WebhookService>();
            _confighandler = _provider.GetService<ConfigHandler>();
            _cancellationToken = new CancellationTokenSource();
            _random = _provider.GetService<Random>();
            //_chessService = _provider.GetService<IChessService>();
            //_manager = _provider.GetService<ConfigDatabase>();
        }

        public async Task StartAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            
            _discord.MessageReceived += CommandHandlerAsync;
            _discord.LeftGuild += LeftGuild;
            _discord.GuildAvailable += GuildAvailable;
            _discord.Disconnected += Disconnected;
            _discord.MessageDeleted += MessageDeletedAsync;
            _discord.JoinedGuild += JoinedGuildAsync;
            _discord.UserJoined += UserJoinedAsync;
            _discord.UserLeft += UserLeftAsync;

            PrettyConsole.Log(LogSeverity.Info, "Commands", $"Loaded {_commands.Modules.Count()} modules with {_commands.Commands.Count()} commands");
        } 

        public async Task ExecuteAsync(CustomCommandContext context, IServiceProvider provider, int argPos)
        {
            var result = await _commands.ExecuteAsync(context, argPos, provider);
            await ResultAsync(context, result);
        }

        public async Task ExecuteAsync(CustomCommandContext context, IServiceProvider provider, string input)
        {
            var result = await _commands.ExecuteAsync(context, input, provider);
            await ResultAsync(context, result);
        }

        private async Task ResultAsync(CustomCommandContext context, IResult result)
        {
            if (result.IsSuccess)
                return;

            if (result is ExecuteResult r)
            {
                PrettyConsole.Log(LogSeverity.Error, "Commands", r.Exception?.ToString());
                return;
            }

            await context.Channel.SendMessageAsync(result.ErrorReason);
        }
        internal async Task CommandHandlerAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage msg))
                return;
            var argPos = 0;
            
            var context = new CustomCommandContext(_discord, msg, _provider);
            if (context.Config.Blacklist.Contains(msg.Author.Id) || _guildhelper.GetProfile(context.Guild.Id, context.User.Id).IsBlacklisted
                || msg.Author.IsBot || context.Server.BlackListedChannels.Contains(context.Channel.Id))
                return;

            if (!(msg.HasStringPrefix(context.Config.Prefix, ref argPos) || msg.HasStringPrefix(context.Server.Prefix, ref argPos) ||
                msg.HasMentionPrefix(context.Client.CurrentUser, ref argPos)) || msg.Source != MessageSource.User)
                return;
            var result = await _commands.ExecuteAsync(context, argPos, _provider, MultiMatchHandling.Best);
            var search = _commands.Search(context, argPos);
            var command = search.IsSuccess ? search.Commands.FirstOrDefault().Command : null;
            switch (result.Error)
            {
                case CommandError.Exception:
                    PrettyConsole.Log(LogSeverity.Error, "Exception", result.ErrorReason);
                    break;
                case CommandError.UnmetPrecondition:
                    if (!result.ErrorReason.Contains("SendMessages"))
                        await context.Channel.SendMessageAsync(result?.ErrorReason);
                    break;
                case CommandError.BadArgCount:
                    var name = command.Module != null && command.Name.Contains("Async")
                        ? command.Module.Group : $"{command.Module.Group ?? null} {command.Name}";
                    await context.Channel.SendMessageAsync($"**Usage:** {context.Config.Prefix}{name} {StringHelper.ParametersInfo(command.Parameters)}");
                    break;
            }
            _ = Task.Run(() => RecordCommand(command, context));
        }
        internal void RecordCommand(CommandInfo command, CustomCommandContext context)
        {
            if (command == null)
                return;
            var profile = _guildhelper.GetProfile(context.Guild.Id, context.User.Id);
            if (!profile.Commands.ContainsKey(command.Name))
                profile.Commands.Add(command.Name, 0);
            profile.Commands[command.Name]++;
            _guildhelper.SaveProfile(context.Guild.Id, context.User.Id, profile);
        }
        internal Task LeftGuild(SocketGuild guild) 
            => Task.Run(() 
                => _guildhandler.RemoveGuild(guild.Id, guild.Name));

        internal Task GuildAvailable(SocketGuild guild) 
            => Task.Run(() 
                => _guildhandler.AddGuild(guild.Id, guild.Name));
        internal Task Disconnected(Exception error)
        {
            _ = Task.Delay(TimeSpan.FromSeconds(5), _cancellationToken.Token).ContinueWith(async _ =>
            {
                PrettyConsole.Log(LogSeverity.Info, "Connection Manager", $"Checking connection state...");
                await CheckStateAsync();
            });
            return Task.CompletedTask;
        }
        internal async Task CheckStateAsync()
        {
            if (_discord.ConnectionState == ConnectionState.Connected)
                return;

            var timeout = Task.Delay(TimeSpan.FromSeconds(30));
            var connect = _discord.StartAsync();
            var localTask = await Task.WhenAny(timeout, connect);

            if (localTask == timeout || connect.IsFaulted)
                Environment.Exit(1);
            else if (connect.IsCompletedSuccessfully)
            {
                PrettyConsole.Log(LogSeverity.Info, "Connection Manager", "Client Reset Completed.");
                return;
            }
            else
                Environment.Exit(1);
        }
        internal Task Connected()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();
            PrettyConsole.Log(LogSeverity.Info, "Connected", "Connected to Discord.");
            return Task.CompletedTask;
        }
        internal async Task MessageDeletedAsync(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            var config = _guildhandler.GetGuild((channel as SocketGuildChannel).Guild.Id);
            var message = await cache.GetOrDownloadAsync();
            if (message == null || config == null || !config.Mod.LogDeletedMessages || message.Author.IsBot) return;
            config.DeletedMessages.Add(new MessageWrapper
            {
                ChannelId = channel.Id,
                MessageId = message.Id,
                AuthorId = message.Author.Id,
                DateTime = message.Timestamp.DateTime,
                Content = message.Content ?? message.Attachments.FirstOrDefault()?.Url
            });
            _guildhandler.Update(config);
        }
        internal async Task JoinedGuildAsync(SocketGuild guild)
        {
            _guildhandler.AddGuild(guild.Id, guild.Name);
            await guild.DefaultChannel.SendMessageAsync(_confighandler.Config.JoinMessage ?? "Thank you for inviting me to your server!");
        }

        internal async Task UserLeftAsync(SocketGuildUser user)
        {
            var config = _guildhandler.GetGuild(user.Guild.Id);
            await _webhookservice.SendMessageAsync(new WebhookOptions
            {
                Name = _discord.CurrentUser.Username,
                Webhook = config.LeaveWebhook,
                Message = !config.LeaveMessages.Any() ? $"**{user.Username}** abandoned us!"
                : StringHelper.Replace(config.LeaveMessages[_random.Next(0, config.LeaveMessages.Count)], user.Guild.Name, user.Username)
            });
        }

        internal async Task UserJoinedAsync(SocketGuildUser user)
        {
            var config = _guildhandler.GetGuild(user.Guild.Id);
            await _webhookservice.SendMessageAsync(new WebhookOptions
            {
                Name = _discord.CurrentUser.Username,
                Webhook = config.JoinWebhook,
                Message = !config.JoinMessages.Any() ? $"**{user.Username}** is here to rock our world! Yeah, baby!"
                : StringHelper.Replace(config.JoinMessages[_random.Next(0, config.JoinMessages.Count)], user.Guild.Name, user.Mention)
            });
            var role = user.Guild.GetRole(config.Mod.JoinRole);
            if (role != null)
                await user.AddRoleAsync(role).ConfigureAwait(false);
        }
    }
}
