using System;
using Discord;
using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArcadesBot
{
    public class GuildHelper
    {
        private GuildHandler GuildHandler { get; }
        private DiscordSocketClient Client { get; }
        public GuildHelper(GuildHandler guildHandler, DiscordSocketClient client)
        {
            Client = client;
            GuildHandler = guildHandler;
        }

        public IMessageChannel DefaultChannel(ulong guildId)
        {
            var guild = Client.GetGuild(guildId);
            return guild.TextChannels.FirstOrDefault(x => x.Name.Contains("general") || x.Name.Contains("lobby") || x.Id == guild.Id) ?? guild.DefaultChannel;
        }

        public UserProfile GetProfile(ulong guildId, ulong userId)
        {
            var guild = GuildHandler.GetGuild(Client.GetGuild(guildId).Id);
            if (!guild.Profiles.ContainsKey(userId))
            {
                guild.Profiles.Add(userId, new UserProfile());
                GuildHandler.Update(guild);
                return guild.Profiles[userId];
            }
            return guild.Profiles[userId];
        }
        /// <param name="guildId">The Guild's ID</param>
        /// <param name="channelId">The Channel's ID</param>
        /// <returns>
        /// true = Added
        /// false = Removed
        /// </returns>
        public bool ToggleBlackList(GuildModel guild, ulong channelId)
        {
            if (!guild.BlackListedChannels.Contains(channelId))
            {
                guild.BlackListedChannels.Add(channelId);
                GuildHandler.Update(guild);
                return true;
            }
            else
            {
                guild.BlackListedChannels.Remove(channelId);
                GuildHandler.Update(guild);
                return false;
            }
            
        }

        public void SaveProfile(ulong guildId, ulong userId, UserProfile profile)
        {
            var config = GuildHandler.GetGuild(guildId);
            config.Profiles[userId] = profile;
            GuildHandler.Update(config);
        }

        public (bool, ulong) GetChannelId(SocketGuild guild, string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                return (true, 0);
            UInt64.TryParse(channel.Replace('<', ' ').Replace('>', ' ').Replace('#', ' ').Replace(" ", ""), out var id);
            var getChannel = guild.GetTextChannel(id);
            if (getChannel != null)
                return (true, id);
            var findChannel = guild.TextChannels.FirstOrDefault(x => x.Name == channel.ToLower());
            if (findChannel != null)
                return (true, findChannel.Id);
            return (false, 0);
        }

        public (bool, ulong) GetRoleId(SocketGuild guild, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return (true, 0);
            UInt64.TryParse(role.Replace('<', ' ').Replace('>', ' ').Replace('@', ' ').Replace('&', ' ').Replace(" ", ""), out var id);
            var getRole = guild.GetRole(id);
            if (getRole != null)
                return (true, id);
            var findRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());
            if (findRole != null)
                return (true, findRole.Id);
            return (false, 0);
        }

        public (bool, string) ListCheck<T>(List<T> collection, object value, string objectName, string collectionName)
        {
            var check = collection.Contains((T)value);

            if (collection.Contains((T)value))
                return (false, $"`{objectName}` already exists in {collectionName}.");
            if (collection.Count == collection.Capacity)
                return (false, $"Reached max number of entries");

            return (true, $"`{objectName}` has been added to {collectionName}");
        }

        public bool HierarchyCheck(IGuild guild, IGuildUser user)
        {
            var guild = guild as SocketGuild;
            var highestRole = guild.CurrentUser.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Position;
            return (user as SocketGuildUser).Roles.Any(x => x.Position > highestRole) ? true : false;
        }
    }
}