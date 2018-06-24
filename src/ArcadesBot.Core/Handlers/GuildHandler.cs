using Discord;
using Raven.Client.Documents;

namespace ArcadesBot
{
    public class GuildHandler
    {
        IDocumentStore Store { get; }
        public GuildHandler(IDocumentStore store) 
            => Store = store;

        public GuildModel GetGuild(ulong id)
        {
            using (var session = Store.OpenSession())
                return session.Load<GuildModel>($"{id}");
        }

        public void RemoveGuild(ulong id, string name = null)
        {
            using (var session = Store.OpenSession())
                session.Delete($"{id}");
            PrettyConsole.Log(LogSeverity.Info, "RemoveGuild", string.IsNullOrWhiteSpace(name) ? $"Removed Server With Id: {id}" : $"Removed Config For {name}");
        }

        public void AddGuild(ulong id, string name = null)
        {
            using (var session = Store.OpenSession())
            {
                if (session.Advanced.Exists($"{id}")) return;
                session.Store(new GuildModel
                {
                    Id = $"{id}",
                    Prefix = "%"
                });
                session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "AddGuild", string.IsNullOrWhiteSpace(name) ? $"Added Server With Id: {id}" : $"Created Config For {name}");
        }

        public void Update(GuildModel server)
        {
            if (server == null)
                return;
            using (var session = Store.OpenSession())
            {
                session.Store(server, server.Id);
                session.SaveChanges();
            }
        }
    }
}