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
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly GuildHelper _guildhelper;
        private readonly GuildHandler _guildhandler;

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
            //_chessService = _provider.GetService<IChessService>();
            //_manager = _provider.GetService<ConfigDatabase>();
        }

        public async Task StartAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            
            _discord.MessageReceived += CommandHandlerAsync;
            _discord.LeftGuild += LeftGuild;
            _discord.GuildAvailable += GuildAvailable;

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
                || Msg.Author.IsBot) return;
            if (!(Msg.HasStringPrefix(Context.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Server.Prefix, ref argPos) ||
                Msg.HasMentionPrefix(Context.Client.CurrentUser, ref argPos)) || Msg.Source != MessageSource.User) return;
            var result = await _commands.ExecuteAsync(Context, argPos, _provider, MultiMatchHandling.Best);
            var search = _commands.Search(Context, argPos);
            var command = search.IsSuccess ? search.Commands.FirstOrDefault().Command : null;
            switch (result.Error)
            {
                case CommandError.Exception: PrettyConsole.Log(LogSeverity.Error, "", result.ErrorReason);
                    break;
                case CommandError.UnmetPrecondition:
                    if (!result.ErrorReason.Contains("SendMessages")) await Context.Channel.SendMessageAsync(result?.ErrorReason);
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
    }
}
