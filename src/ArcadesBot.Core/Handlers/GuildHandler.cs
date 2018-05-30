using System.Drawing;
using Raven.Client.Documents;
using Discord;

namespace ArcadesBot
{
    public class GuildHandler
    {
        IDocumentStore Store { get; }
        public GuildHandler(IDocumentStore store) 
            => Store = store;

        public GuildModel GetGuild(ulong Id)
        {
            using (var Session = Store.OpenSession())
                return Session.Load<GuildModel>($"{Id}");
        }

        public void RemoveGuild(ulong Id, string Name = null)
        {
            using (var Session = Store.OpenSession())
                Session.Delete($"{Id}");
            PrettyConsole.Log(LogSeverity.Info, "RemoveGuild", string.IsNullOrWhiteSpace(Name) ? $"Removed Server With Id: {Id}" : $"Removed Config For {Name}");
        }

        public void AddGuild(ulong Id, string Name = null)
        {
            using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists($"{Id}")) return;
                Session.Store(new GuildModel
                {
                    Id = $"{Id}",
                    Prefix = "%"
                });
                Session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "AddGuild", string.IsNullOrWhiteSpace(Name) ? $"Added Server With Id: {Id}" : $"Created Config For {Name}");
        }

        public void Save(GuildModel Server)
        {
            if (Server == null) return;
            using (var Session = Store.OpenSession())
            {
                Session.Store(Server, Server.Id);
                Session.SaveChanges();
            }
        }
    }
}