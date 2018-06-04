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
                using (var Session = Store.OpenSession())
                    return Session.Load<ConfigModel>("Config");
            }
        }

        public ConfigModel ConfigCheck()
        {
            using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists("Config"))
                    return Config;
                PrettyConsole.Log(LogSeverity.Info, "Arcade's Bot", "Enter Bot's Token:");
                string Token = Console.ReadLine();
                PrettyConsole.Log(LogSeverity.Info, "Arcade's Bot", "Enter Bot's Prefix:");
                string Prefix = Console.ReadLine();
                Session.Store(new ConfigModel
                {
                    Id = "Config",
                    Token = Token,
                    Prefix = Prefix
                });
                Session.SaveChanges();
            }
            return Config;
        }

        public void Save(ConfigModel GetConfig = null)
        {
            GetConfig = GetConfig ?? Config;
            if (GetConfig == null)
                return;
            using (var Session = Store.OpenSession())
            {
                Session.Store(GetConfig, "Config");
                Session.SaveChanges();
            }
        }
    }
}