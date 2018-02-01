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
        private readonly IChessService _chessService;
        private readonly ConfigDatabase _manager;
        private readonly IServiceProvider _provider;

        public CommandManager(IServiceProvider provider)
        {
            _provider = provider;
            _discord = _provider.GetService<DiscordSocketClient>();
            _commands = _provider.GetService<CommandService>();
            _chessService = _provider.GetService<IChessService>();
            _manager = _provider.GetService<ConfigDatabase>();
        }

        public async Task StartAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            
            _discord.MessageReceived += OnMessageReceivedAsync;
            PrettyConsole.Log(LogSeverity.Info, "Commands", $"Loaded {_commands.Modules.Count()} modules with {_commands.Commands.Count()} commands");
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg))
                return;

            var context = new CustomCommandContext(_discord, msg);
            if (context.User.IsBot)
                return;
            var prefix = await _manager.GetPrefixAsync(context.Guild.Id);
            int argPos = 0;
            bool hasStringPrefix = prefix != null && msg.HasStringPrefix(prefix, ref argPos);

            if ((hasStringPrefix || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)))
                await ExecuteAsync(context, _provider, argPos);
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
    }
}
