using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Handlers;
using ArcadesBot.Helpers;
using ArcadesBot.Models;
using ArcadesBot.Utility;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ArcadesBot.Services
{
    public class CommandManager
    {
        private DiscordSocketClient _discord { get; }
        private CommandService _commands { get; }
        private GuildHelper _guildhelper { get; }
        private GuildHandler _guildhandler { get; }
        private WebhookService _webhookservice { get; }
        private DatabaseHandler _databaseHandler { get; }
        private Random _random { get; }
        private IServiceProvider _provider { get; }

        public CommandManager(IServiceProvider provider)
        {
            _provider = provider;
            _discord = _provider.GetService<DiscordSocketClient>();
            _commands = _provider.GetService<CommandService>();
            _guildhelper = _provider.GetService<GuildHelper>();
            _databaseHandler = _provider.GetService<DatabaseHandler>();
            _guildhandler = _provider.GetService<GuildHandler>();
            _webhookservice = _provider.GetService<WebhookService>();
            _random = _provider.GetService<Random>();
        }

        public async Task StartAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            
            _discord.MessageReceived += CommandHandlerAsync;
            _discord.LeftGuild += LeftGuild;
            _discord.GuildAvailable += GuildAvailable;
            _discord.UserJoined += UserJoinedAsync;
            _discord.UserLeft += UserLeftAsync;
            _discord.MessageDeleted += DiscordOnMessageDeleted;

            PrettyConsole.Log(LogSeverity.Info, "Commands", $"Loaded {_commands.Modules.Count()} modules with {_commands.Commands.Count()} commands");
        }

        internal async Task DiscordOnMessageDeleted(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            
            var guildChannel = channel is SocketGuildChannel ? (SocketGuildChannel) channel : null;
            if (guildChannel == null)
                return;
            var server = _guildhandler.GetGuild(guildChannel.Guild.Id);
            if (!server.Mod.LogDeletedMessages)
                return;
            var message = cache.HasValue ? cache.Value : await cache.GetOrDownloadAsync();
            if (string.IsNullOrWhiteSpace(message.Content) || message.Author.IsBot)
                return;

            var logChannel = _guildhelper.GetGuildChannel(server.Id, server.Mod.TextChannel) as IMessageChannel;
            var embed = new EmbedBuilder().WithErrorColor().WithTitle($"Deleted message in #{guildChannel.Name}")
                .AddField("Content", message.Content ?? message.Attachments.FirstOrDefault().Url)
                .AddField("Author", $"{message.Author.Mention} ({message.Author.Username}#{message.Author.DiscriminatorValue})");
            await logChannel.SendMessageAsync(embed: embed.Build());



            //logChannel.s
            //{
            //    MessageId = message.Id,
            //    ChannelId = channel.Id,
            //    DateTimeOffset = MiscExt.Central,
            //    AuthorId = message.Author.Id,
            //    Content = message.Content ?? message.Attachments.FirstOrDefault().Url
            //});
            //Db.Save<ServerObject>(server, (channel as SocketGuildChannel).Guild.Id);
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

                
            if (!(msg.HasStringPrefix(context.Server.Prefix, ref argPos) || msg.HasMentionPrefix(context.Client.CurrentUser, ref argPos)) 
                || msg.Source != MessageSource.User)
                return;
            if (msg.Content == context.Server.Prefix)
            {
                await _commands.ExecuteAsync(context, "?", _provider);
                return;
            }
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
                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder().WithErrorColor()
                            .WithDescription(result.ErrorReason).Build());
                    break;
                case CommandError.UnknownCommand:
                    break;
                case CommandError.ParseFailed:
                    break;
                case CommandError.BadArgCount:
                    break;
                case CommandError.ObjectNotFound:
                    await context.Channel.SendMessageAsync(embed: new EmbedBuilder().WithErrorColor()
                        .WithDescription(result.ErrorReason).Build());
                    break;
                case CommandError.MultipleMatches:
                    break;
                case CommandError.Unsuccessful:
                    break;
            }
             await Task.Run(() => RecordCommand(command, context));
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
                => _guildhandler.RemoveGuild(guild.Id));

        internal Task GuildAvailable(SocketGuild guild) 
            => Task.Run(() 
                => _guildhandler.AddGuild(guild.Id));

        internal async Task UserLeftAsync(SocketGuildUser user)
        {
            var config = _guildhandler.GetGuild(user.Guild.Id);
            await _webhookservice.SendMessageAsync(new WebhookOptions
            {
                Name = _discord.CurrentUser.Username,
                Webhook = config.LeaveWebhook,
                Message = !config.LeaveMessages.Any() ? $"**{user.Username}** abandoned us!"
                : config.JoinMessages[_random.Next(0, config.JoinMessages.Count)].Replace(user.Guild.Name, user.Username)
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
                : config.JoinMessages[_random.Next(0, config.JoinMessages.Count)].Replace(user.Guild.Name, user.Mention)
            });
            var role = user.Guild.GetRole(config.Mod.JoinRole);
            if (role != null)
                await user.AddRoleAsync(role).ConfigureAwait(false);
        }
    }
}
