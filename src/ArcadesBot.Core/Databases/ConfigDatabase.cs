using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class ConfigDatabase : DbContext
    {
        public DbSet<GuildConfig> GuildConfigs { get; set; }

        public ConfigDatabase()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "data");
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            string datadir = Path.Combine(baseDir, "config.sqlite.db");
            optionsBuilder.UseSqlite($"Filename={datadir}");
        }

        public async Task<GuildConfig> GetConfigAsync(ulong guildId)
        {
            var config = await GuildConfigs.FirstOrDefaultAsync(x => x.GuildId == guildId);

            if (config != null)
                return config;

            config = new GuildConfig(guildId);
            await GuildConfigs.AddAsync(config);
            await SaveChangesAsync();
            return config;
        }

        public async Task<string> GetPrefixAsync(ulong guildId)
        {
            var config = await GetConfigAsync(guildId);
            return config.Prefix;
        }

        public async Task SetPrefixAsync(GuildConfig config, string prefix)
        {
            config.Prefix = string.IsNullOrWhiteSpace(prefix) ? null : prefix;

            GuildConfigs.Update(config);
            await SaveChangesAsync();
        }

        public async Task BlockChannel(GuildConfig config, ulong channelToBlock)
        {
            config.NotAllowedChannelList.Add(channelToBlock);
            await SaveChangesAsync();
        }
        public async Task AllowChannel(GuildConfig config, ulong channelToAllow)
        {
            config.NotAllowedChannelList.Remove(channelToAllow);
            await SaveChangesAsync();
        }
        public async Task AddToModList(GuildConfig config, ulong roleId)
        {
            config.ModRolesList.Add(roleId);
            await SaveChangesAsync();
        }
        public async Task RemoveFromModList(GuildConfig config, ulong roleId)
        {
            config.ModRolesList.Remove(roleId);
            await SaveChangesAsync();
        }
        public async Task AddToAdminList(GuildConfig config, ulong roleId)
        {
            config.AdminRoleList.Add(roleId);
            await SaveChangesAsync();
        }
        public async Task RemoveFromAdminList(GuildConfig config, ulong roleId)
        {
            config.AdminRoleList.Remove(roleId);
            await SaveChangesAsync();
        }
        public async Task ChangeTimeout(ulong timeInSeconds, ulong guildId)
        {
            var config = await GetConfigAsync(guildId);
            config.TimeoutInSeconds = timeInSeconds;
            GuildConfigs.Update(config);
            await SaveChangesAsync();
        }
    }
}
