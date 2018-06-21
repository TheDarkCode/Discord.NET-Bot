using Discord;
using Raven.Client.Documents;
using System;

namespace ArcadesBot
{
    public class ConfigHandler
    {
        private IDocumentStore Store { get; }
        public ConfigHandler(IDocumentStore store) 
            => Store = store;
        public ConfigModel Config
        {
            get
            {
                using (var session = Store.OpenSession())
                    return session.Load<ConfigModel>("Config");
            }
        }

        public ConfigModel ConfigCheck()
        {
            using (var session = Store.OpenSession())
            {
                if (session.Advanced.Exists("Config"))
                    return Config;
                PrettyConsole.Log(LogSeverity.Info, "Arcade's Bot", "Enter Bot's Token:");
                var token = Console.ReadLine();
                PrettyConsole.Log(LogSeverity.Info, "Arcade's Bot", "Enter Bot's Prefix:");
                var prefix = Console.ReadLine();
                session.Store(new ConfigModel
                {
                    Id = "Config",
                    Token = token,
                    Prefix = prefix
                });
                session.SaveChanges();
            }
            return Config;
        }

        public void Save(ConfigModel getConfig = null)
        {
            getConfig = getConfig ?? Config;
            if (getConfig == null)
                return;
            using (var session = Store.OpenSession())
            {
                session.Store(getConfig, "Config");
                session.SaveChanges();
            }
        }
    }
}