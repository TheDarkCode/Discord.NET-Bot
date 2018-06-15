using ChessDotNet;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
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
                .AddSingleton(new DocumentStore
                {
                    Certificate = DatabaseHandler.DbConfig.Certificate,
                    Database = DatabaseHandler.DbConfig.DatabaseName,
                    Urls = new[] { DatabaseHandler.DbConfig.DatabaseUrl }
                }.Initialize())
                .AddSingleton<ChessHandler>()
                .AddSingleton<GuildHandler>()
                .AddSingleton<ChessStatsHandler>()
                .AddSingleton<ConfigHandler>()
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
                MessageCacheSize = 1000
            });

            var commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                ThrowOnError = false
            });

            discord.Log += OnLogAsync;
            commands.Log += OnLogAsync;

            services.AddSingleton(discord);
            services.AddSingleton(commands);
            return Task.CompletedTask;
        }
        
        private Task LoadGoogleAsync(IServiceCollection services)
        {
            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);

            var config = provider.GetService<ConfigHandler>();
            ConfigModel model;
            if (config.Config == null)
                model = config.ConfigCheck();
            else
                model = config.Config;

            var youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = config.Config.ApiKeys["Google"],
                MaxUrlLength = 256
            });
            services.AddSingleton(youtube);
            return Task.CompletedTask;
        }

        private Task OnLogAsync(LogMessage msg)
            => PrettyConsole.LogAsync(msg.Severity, msg.Source, msg.Exception?.ToString() ?? msg.Message);
    }
}
