using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class CommandManager
    {
        private DiscordSocketClient _discord { get; }
        private CommandService _commands { get; }
        private GuildHelper _guildhelper { get; }
        private GuildHandler _guildhandler { get; }
        private WebhookService _webhookservice { get; }
        private Random _random { get; }
        private IServiceProvider _provider { get; }

        public CommandManager(IServiceProvider provider)
        {
            _provider = provider;
            _discord = _provider.GetService<DiscordSocketClient>();
            _commands = _provider.GetService<CommandService>();
            _guildhelper = _provider.GetService<GuildHelper>();
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

            PrettyConsole.Log(LogSeverity.Info, "Commands", $"Loaded {_commands.Modules.Count()} modules with {_commands.Commands.Count()} commands");
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
                default:
                    throw new ArgumentOutOfRangeException();
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
