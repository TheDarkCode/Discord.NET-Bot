using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ArcadesBot
{
    public class Program
    {
        public static void Main()
            => new Program().StartAsync().GetAwaiter().GetResult();

        private Startup _startup { get; } = new Startup();

        public async Task StartAsync()
        {
            PrettyConsole.NewLine($"ArcadesBot v{AppHelper.Version}");
            PrettyConsole.NewLine();

            var services = await _startup.ConfigureServices();

            await services.GetService<SchedulerService>().Initialize();

            var manager = services.GetService<CommandManager>();
            await manager.StartAsync();
            var databaseHandler = services.GetRequiredService<DatabaseHandler>();
            var discord = services.GetService<DiscordSocketClient>();
            await discord.LoginAsync(TokenType.Bot, databaseHandler.Config.ApiKeys["Discord"]);
            await discord.StartAsync();

            
            await Task.Delay(-1);
        }
    }
}