using ChessDotNet;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class Startup
    {
        public async Task<IServiceProvider> ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton<DatabaseHandler>()
                .AddSingleton<ChessHandler>()
                .AddSingleton<GuildHandler>()
                .AddSingleton<ChessStatsHandler>()
                .AddSingleton<SchedulerService>()
                .AddSingleton<CommandManager>()
                .AddSingleton<RoslynManager>()
                .AddSingleton<Random>()
                .AddSingleton<ChessHelper>()
                .AddSingleton<HttpClient>()
                .AddSingleton<AssetService>()
                .AddSingleton<ChessService>()
                .AddSingleton<WebhookService>()
                .AddSingleton<GuildHelper>()
                .AddSingleton<WebhookService>()
                .AddSingleton<DatabaseHandler>()
                .AddSingleton<ChessGame>();

            // Discord
            await LoadDiscordAsync(services);

            // Google
            await LoadGoogleAsync(services);
            
            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            return provider;
        }

        private Task LoadDiscordAsync(IServiceCollection services)
        {
            var discord = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000,
            });

            var commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                ThrowOnError = false
            });
            
            discord.Log += OnLogAsync;
            commands.Log += OnLogAsync;


            commands.AddTypeReader<TagModel>(new TagModelTypeReader());

            services.AddSingleton(discord);
            services.AddSingleton(commands);
            return Task.CompletedTask;
        }
        
        private Task LoadGoogleAsync(IServiceCollection services)
        {
            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);

            var databaseHandler = provider.GetService<DatabaseHandler>();
            if (databaseHandler.Config == null)
                databaseHandler.Initialize();

            var youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = databaseHandler.Config.ApiKeys["Google"],
                MaxUrlLength = 256
            });
            services.AddSingleton(youtube);
            return Task.CompletedTask;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            PrettyConsole.Log(msg.Severity, msg.Source, msg.Exception?.ToString() ?? msg.Message);
            return Task.CompletedTask;
        }
            
    }
}
