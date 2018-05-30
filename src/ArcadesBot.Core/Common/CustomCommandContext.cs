using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
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
            Guild = (msg.Channel as IGuildChannel).Guild;
            Random = provider.GetRequiredService<Random>();
            HttpClient = provider.GetRequiredService<HttpClient>();
            GuildHelper = provider.GetRequiredService<GuildHelper>();
            GuildHandler = provider.GetRequiredService<GuildHandler>();
            Config = provider.GetRequiredService<ConfigHandler>().Config;
            Session = provider.GetRequiredService<IDocumentStore>().OpenSession();
            Server = provider.GetRequiredService<GuildHandler>().GetGuild(Guild.Id);
        }

        public IUser User { get; }
        public IGuild Guild { get; }
        public Random Random { get; }
        public HttpClient HttpClient { get; }
        public GuildModel Server { get; }
        public ConfigModel Config { get; }
        public DiscordSocketClient Client { get; }
        public IUserMessage Message { get; }
        public GuildHelper GuildHelper { get; }
        public IMessageChannel Channel { get; }
        public IDocumentSession Session { get; }
        public GuildHandler GuildHandler { get; }
        public IServiceProvider Provider { get; }

        public bool IsPrivate => Channel is IPrivateChannel;

        //ICommandContext
        IDiscordClient ICommandContext.Client => Client;
        IGuild ICommandContext.Guild => Guild;
        IMessageChannel ICommandContext.Channel => Channel;
        IUser ICommandContext.User => User;
        IUserMessage ICommandContext.Message => Message;
    }
}
