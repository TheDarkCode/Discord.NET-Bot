using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ArcadesBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private readonly Startup _startup = new Startup();

        public async Task StartAsync()
        {
            PrettyConsole.NewLine($"ArcadesBot v{AppHelper.Version}");
            PrettyConsole.NewLine();

            var services = await _startup.ConfigureServices();
            services.GetRequiredService<ConfigHandler>().ConfigCheck();
            var manager = services.GetService<CommandManager>();
            await manager.StartAsync();

            var discord = services.GetService<DiscordSocketClient>();
            var config = services.GetService<ConfigHandler>();
            await discord.LoginAsync(TokenType.Bot, config.Config.Token);
            await discord.StartAsync();

            await services.GetRequiredService<DatabaseHandler>().DatabaseCheck();
            await Task.Delay(-1);
        }
    }
}