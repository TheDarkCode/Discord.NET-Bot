﻿using System;
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
        IDocumentStore Store { get; }
        ConfigHandler Config { get; }
        public DatabaseHandler(IDocumentStore store, ConfigHandler config)
        {
            Store = store;
            Config = config;
        }

        public static DatabaseModel DbConfig
        {
            get
            {
                var dbConfigPath = $"{Directory.GetCurrentDirectory()}/DBConfig.json";
                if (File.Exists(dbConfigPath))
                    return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(dbConfigPath));

                File.WriteAllText(dbConfigPath, JsonConvert.SerializeObject(new DatabaseModel(), Formatting.Indented));
                return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(dbConfigPath));
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
            Config.ConfigCheck();
        }

        private async Task DatabaseSetupAsync()
        {
            if (Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).Any(x => x == DbConfig.DatabaseName))
                return;

            PrettyConsole.Log(LogSeverity.Warning, "Database", $"Database {DbConfig.DatabaseName} doesn't exist!");
            _ = await Store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(DbConfig.DatabaseName)));
            PrettyConsole.Log(LogSeverity.Info, "Database", $"Created Database{DbConfig.DatabaseName}.");
            PrettyConsole.Log(LogSeverity.Info, "Database", "Setting up backup operation...");

            _ = await Store.Maintenance.SendAsync(new UpdatePeriodicBackupOperation(new PeriodicBackupConfiguration
            {
                Name = "Backup",
                BackupType = BackupType.Backup,
                FullBackupFrequency = "*/10 * * * *",
                IncrementalBackupFrequency = "0 2 * * *",
                LocalSettings = new LocalSettings { FolderPath = DbConfig.BackupLocation }
            })).ConfigureAwait(false);

            PrettyConsole.Log(LogSeverity.Info, "Database", "Finished backup operation!");
        }
    }
}