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
        internal async Task CommandHandlerAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg))
                return;
            int argPos = 0;
            
            var Context = new CustomCommandContext(_discord, Msg, _provider);
            if (Context.Config.Blacklist.Contains(Msg.Author.Id) || _guildhelper.GetProfile(Context.Guild.Id, Context.User.Id).IsBlacklisted
                || Msg.Author.IsBot || Context.Server.BlackListedChannels.Contains(Context.Channel.Id))
                return;
            if (!(Msg.HasStringPrefix(Context.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Server.Prefix, ref argPos) ||
                Msg.HasMentionPrefix(Context.Client.CurrentUser, ref argPos)) || Msg.Source != MessageSource.User)
                return;
            var result = await _commands.ExecuteAsync(Context, argPos, _provider, MultiMatchHandling.Best);
            var search = _commands.Search(Context, argPos);
            var command = search.IsSuccess ? search.Commands.FirstOrDefault().Command : null;
            switch (result.Error)
            {
                case CommandError.Exception:
                    PrettyConsole.Log(LogSeverity.Error, "Exception", result.ErrorReason);
                    break;
                case CommandError.UnmetPrecondition:
                    if (!result.ErrorReason.Contains("SendMessages"))
                        await Context.Channel.SendMessageAsync(result?.ErrorReason);
                    break;
                case CommandError.BadArgCount:
                    string Name = command.Module != null && command.Name.Contains("Async")
                        ? command.Module.Group : $"{command.Module.Group ?? null} {command.Name}";
                    await Context.Channel.SendMessageAsync($"**Usage:** {Context.Config.Prefix}{Name} {StringHelper.ParametersInfo(command.Parameters)}");
                    break;
            }
            _ = Task.Run(() => RecordCommand(command, Context));
        }
        internal void RecordCommand(CommandInfo Command, CustomCommandContext Context)
        {
            if (Command == null)
                return;
            var Profile = _guildhelper.GetProfile(Context.Guild.Id, Context.User.Id);
            if (!Profile.Commands.ContainsKey(Command.Name))
                Profile.Commands.Add(Command.Name, 0);
            Profile.Commands[Command.Name]++;
            _guildhelper.SaveProfile(Context.Guild.Id, Context.User.Id, Profile);
        }
        internal Task LeftGuild(SocketGuild Guild) 
            => Task.Run(() 
                => _guildhandler.RemoveGuild(Guild.Id, Guild.Name));

        internal Task GuildAvailable(SocketGuild Guild) 
            => Task.Run(() 
                => _guildhandler.AddGuild(Guild.Id, Guild.Name));
        internal Task Disconnected(Exception Error)
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

            var Timeout = Task.Delay(TimeSpan.FromSeconds(30));
            var Connect = _discord.StartAsync();
            var LocalTask = await Task.WhenAny(Timeout, Connect);

            if (LocalTask == Timeout || Connect.IsFaulted)
                Environment.Exit(1);
            else if (Connect.IsCompletedSuccessfully)
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
        internal async Task MessageDeletedAsync(Cacheable<IMessage, ulong> Cache, ISocketMessageChannel Channel)
        {
            var Config = _guildhandler.GetGuild((Channel as SocketGuildChannel).Guild.Id);
            var Message = await Cache.GetOrDownloadAsync();
            if (Message == null || Config == null || !Config.Mod.LogDeletedMessages || Message.Author.IsBot) return;
            Config.DeletedMessages.Add(new MessageWrapper
            {
                ChannelId = Channel.Id,
                MessageId = Message.Id,
                AuthorId = Message.Author.Id,
                DateTime = Message.Timestamp.DateTime,
                Content = Message.Content ?? Message.Attachments.FirstOrDefault()?.Url
            });
            _guildhandler.Update(Config);
        }
        internal async Task JoinedGuildAsync(SocketGuild Guild)
        {
            _guildhandler.AddGuild(Guild.Id, Guild.Name);
            await Guild.DefaultChannel.SendMessageAsync(_confighandler.Config.JoinMessage ?? "Thank you for inviting me to your server!");
        }

        internal async Task UserLeftAsync(SocketGuildUser User)
        {
            var Config = _guildhandler.GetGuild(User.Guild.Id);
            await _webhookservice.SendMessageAsync(new WebhookOptions
            {
                Name = _discord.CurrentUser.Username,
                Webhook = Config.LeaveWebhook,
                Message = !Config.LeaveMessages.Any() ? $"**{User.Username}** abandoned us!"
                : StringHelper.Replace(Config.LeaveMessages[_random.Next(0, Config.LeaveMessages.Count)], User.Guild.Name, User.Username)
            });
        }

        internal async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = _guildhandler.GetGuild(User.Guild.Id);
            await _webhookservice.SendMessageAsync(new WebhookOptions
            {
                Name = _discord.CurrentUser.Username,
                Webhook = Config.JoinWebhook,
                Message = !Config.JoinMessages.Any() ? $"**{User.Username}** is here to rock our world! Yeah, baby!"
                : StringHelper.Replace(Config.JoinMessages[_random.Next(0, Config.JoinMessages.Count)], User.Guild.Name, User.Mention)
            });
            var Role = User.Guild.GetRole(Config.Mod.JoinRole);
            if (Role != null)
                await User.AddRoleAsync(Role).ConfigureAwait(false);
        }
    }
}
