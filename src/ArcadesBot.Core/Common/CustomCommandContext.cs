using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace ArcadesBot
{
    public class CustomCommandContext : ICommandContext
    {
        public CustomCommandContext(DiscordSocketClient client, SocketUserMessage msg, IServiceProvider provider)
        {
            Client = client;
            Message = msg;
            User = msg.Author;
            Channel = msg.Channel;
            Guild = (msg.Channel as SocketTextChannel).Guild;
            Random = provider.GetRequiredService<Random>();
            HttpClient = provider.GetRequiredService<HttpClient>();
            GuildHelper = provider.GetRequiredService<GuildHelper>();
            WebhookService = provider.GetRequiredService<WebhookService>();
            Database = provider.GetRequiredService<DatabaseHandler>();
            GuildHandler = provider.GetRequiredService<GuildHandler>();
            Config = provider.GetRequiredService<DatabaseHandler>().Config;
            Server = provider.GetRequiredService<GuildHandler>().GetGuild(Guild.Id);
        }

        public IUser User { get; }
        public SocketGuild Guild { get; }
        public Random Random { get; }
        public HttpClient HttpClient { get; }
        public GuildModel Server { get; }
        public ConfigModel Config { get; }
        public DatabaseHandler Database { get; }
        public GuildHandler GuildHandler { get; }
        public DiscordSocketClient Client { get; }
        public IUserMessage Message { get; }
        public GuildHelper GuildHelper { get; }
        public IMessageChannel Channel { get; }
        public WebhookService WebhookService { get; }

        //ICommandContext
        IDiscordClient ICommandContext.Client => Client;
        IGuild ICommandContext.Guild => Guild;
        IMessageChannel ICommandContext.Channel => Channel;
        IUser ICommandContext.User => User;
        IUserMessage ICommandContext.Message => Message;
    }
}