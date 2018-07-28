using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Discord;
using Raven.Client.ServerWide.Operations;
using System.Reflection;

namespace ArcadesBot
{
    public class DatabaseHandler
    {
        private IDocumentStore _store { get; set; }
        public DatabaseHandler() 
            => Initialize();

        public ConfigModel Config 
            => Select<ConfigModel>("Config");

        public static DatabaseModel DbConfig
        {
            get
            {
                var dbConfigPath = $"{Directory.GetCurrentDirectory()}/config/DBConfig.json";
                if (File.Exists(dbConfigPath))
                    return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(dbConfigPath));

                File.WriteAllText(dbConfigPath, JsonConvert.SerializeObject(new DatabaseModel(), Formatting.Indented));
                return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(dbConfigPath));
            }
        }
        public void Initialize()
        {
            var DBName = DbConfig.DatabaseName; 
            if (Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "Raven.Server") == null)
                PrettyConsole.Log(LogSeverity.Error, "Database", "Please make sure RavenDB is running.");

            _store = new Lazy<IDocumentStore>(
                    () => new DocumentStore { Database = DbConfig.DatabaseName, Urls = new[] { DbConfig.DatabaseUrl } }.Initialize(),
                    true).Value;
            if (_store == null)
                PrettyConsole.Log(LogSeverity.Error, "Database", "Failed to build document store.");



            if (_store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).All(x => x != DBName))
                _store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(DBName)));

            _store.AggressivelyCacheFor(TimeSpan.FromMinutes(30));



            using (var session = _store.OpenSession())
            {
                if (session.Advanced.Exists("Config"))
                    return;
                PrettyConsole.Log(LogSeverity.Info, "Arcade's Bot", "Enter Bot's Token:");
                var token = Console.ReadLine();
                PrettyConsole.Log(LogSeverity.Info, "Arcade's Bot", "Enter Bot's Prefix:");
                var prefix = Console.ReadLine();
                var model = new ConfigModel
                {
                    Prefix = prefix,
                    Blacklist = new List<ulong>(),
                    Namespaces = new List<string>(),
                    ApiKeys = new Dictionary<string, string>{ { "Giphy", "dc6zaTOxFJmzC" }, { "Google", "" }, { "Discord", token }, { "Imgur", "" }, { "Cleverbot", "" } }
                };
                var id = "Config";
                Create<ConfigModel>(ref id, model);
            }
        }

        public T Create<T>(ref string id, object data)
        {
            var returnValue = (T)data;
            using (var session = _store.OpenSession(_store.Database))
            {
                if (session.Advanced.Exists($"{id}") && ulong.TryParse(id, out _))
                {
                    return returnValue;
                }
                while (session.Advanced.Exists($"{id}") )
                    id = Guid.NewGuid().ToString();
                session.Store((T)data, $"{id}");
                PrettyConsole.Log(LogSeverity.Info, "Database", $"Added {typeof(T).Name} with {id} id.");

                session.SaveChanges();
                session.Dispose();
            }

            return returnValue;
        }

        public T Select<T>(object id = null)
        {
            T returnValue;
            using (var session = _store.OpenSession(_store.Database))
            {
                returnValue = session.Load<T>($"{id}");

                session.SaveChanges();
                session.Dispose();
            }

            return returnValue;
        }
        public List<T> Query<T>()
        {
            List<T> returnValue;
            using (var session = _store.OpenSession(_store.Database))
            {
                returnValue = session.Query<T>().ToList();
            }

            return returnValue;
        }
        public void Delete<T>(object id)
        {
            using (var session = _store.OpenSession(_store.Database))
            {
                PrettyConsole.Log(LogSeverity.Info, "Database", $"Removed {typeof(T).Name} with {id} id.");
                session.Delete(session.Load<T>($"{id}"));

                session.SaveChanges();
                session.Dispose();
            }
        }
        public void Update<T>(object id, object data)
        {
            using (var session = _store.OpenSession())
            {
                session.Store((T)data, $"{id}");
                session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "Database", $"Updated {nameof(T)} with {id} id.");
        }

    }
}