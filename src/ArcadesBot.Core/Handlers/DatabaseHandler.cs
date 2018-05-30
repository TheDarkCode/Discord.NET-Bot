using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Discord;
using Raven.Client.ServerWide.Operations;
using Raven.Client.Documents.Operations.Backups;

namespace ArcadesBot
{
    public class DatabaseHandler
    {
        IDocumentStore _store { get; }
        ConfigHandler _config { get; }
        public DatabaseHandler(IDocumentStore store, ConfigHandler config)
        {
            _store = store;
            _config = config;
        }

        public static DatabaseModel DBConfig
        {
            get
            {
                var DBConfigPath = $"{Directory.GetCurrentDirectory()}/DBConfig.json";
                if (File.Exists(DBConfigPath))
                    return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(DBConfigPath));

                File.WriteAllText(DBConfigPath, JsonConvert.SerializeObject(new DatabaseModel(), Formatting.Indented));
                return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(DBConfigPath));
            }
        }

        public async Task DatabaseCheck()
        {
            if (Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "Raven.Server") == null)
            {
                PrettyConsole.Log(LogSeverity.Critical, "Database", "Raven Server isn't running. Please make sure RavenDB is running.\nExiting ...");
                await Task.Delay(5000);
                Environment.Exit(Environment.ExitCode);
            }

            await DatabaseSetupAsync().ConfigureAwait(false);
            _config.ConfigCheck();
        }

        private async Task DatabaseSetupAsync()
        {
            if (_store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).Any(x => x == DBConfig.DatabaseName))
                return;

            PrettyConsole.Log(LogSeverity.Warning, "Database", $"Database {DBConfig.DatabaseName} doesn't exist!");
            _ = await _store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(DBConfig.DatabaseName)));
            PrettyConsole.Log(LogSeverity.Info, "Database", $"Created Database{DBConfig.DatabaseName}.");
            PrettyConsole.Log(LogSeverity.Info, "Database", "Setting up backup operation...");

            _ = await _store.Maintenance.SendAsync(new UpdatePeriodicBackupOperation(new PeriodicBackupConfiguration
            {
                Name = "Backup",
                BackupType = BackupType.Backup,
                FullBackupFrequency = "*/10 * * * *",
                IncrementalBackupFrequency = "0 2 * * *",
                LocalSettings = new LocalSettings { FolderPath = DBConfig.BackupLocation }
            })).ConfigureAwait(false);

            PrettyConsole.Log(LogSeverity.Info, "Database", "Finished backup operation!");
        }
    }
}