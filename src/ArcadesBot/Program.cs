using System.Threading.Tasks;
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

            Configuration.EnsureExists();

            var services = await _startup.ConfigureServices();

            var manager = services.GetService<CommandManager>();
            await manager.StartAsync();
            await services.GetRequiredService<DatabaseHandler>().DatabaseCheck();
            await Task.Delay(-1);
        }
    }
}