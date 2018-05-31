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
        private GuildHandler _guildHandler { get; }
        private DiscordSocketClient _client { get; }
        public GuildHelper(GuildHandler guildHandler, DiscordSocketClient client)
        {
            _client = client;
            _guildHandler = guildHandler;
        }

        public IMessageChannel DefaultChannel(ulong GuildId)
        {
            var Guild = _client.GetGuild(GuildId);
            return Guild.TextChannels.FirstOrDefault(x => x.Name.Contains("general") || x.Name.Contains("lobby") || x.Id == Guild.Id) ?? Guild.DefaultChannel;
        }

        public UserProfile GetProfile(ulong guildId, ulong userId)
        {
            var Guild = _guildHandler.GetGuild(_client.GetGuild(guildId).Id);
            if (!Guild.Profiles.ContainsKey(userId))
            {
                Guild.Profiles.Add(userId, new UserProfile());
                _guildHandler.Update(Guild);
                return Guild.Profiles[userId];
            }
            return Guild.Profiles[userId];
        }
        /// <param name="guildId">The Guild's ID</param>
        /// <param name="channelId">The Channel's ID</param>
        /// <returns>
        /// true = Added
        /// false = Removed
        /// </returns>
        public bool ToggleBlackList(GuildModel Guild, ulong channelId)
        {
            if (!Guild.BlackListedChannels.Contains(channelId))
            {
                Guild.BlackListedChannels.Add(channelId);
                _guildHandler.Update(Guild);
                return true;
            }
            else
            {
                Guild.BlackListedChannels.Remove(channelId);
                _guildHandler.Update(Guild);
                return false;
            }
            
        }

        public void SaveProfile(ulong GuildId, ulong UserId, UserProfile Profile)
        {
            var Config = _guildHandler.GetGuild(GuildId);
            Config.Profiles[UserId] = Profile;
            _guildHandler.Update(Config);
        }

        public (bool, ulong) GetChannelId(SocketGuild Guild, string Channel)
        {
            if (string.IsNullOrWhiteSpace(Channel))
                return (true, 0);
            UInt64.TryParse(Channel.Replace('<', ' ').Replace('>', ' ').Replace('#', ' ').Replace(" ", ""), out ulong Id);
            var GetChannel = Guild.GetTextChannel(Id);
            if (GetChannel != null)
                return (true, Id);
            var FindChannel = Guild.TextChannels.FirstOrDefault(x => x.Name == Channel.ToLower());
            if (FindChannel != null)
                return (true, FindChannel.Id);
            return (false, 0);
        }

        public (bool, ulong) GetRoleId(SocketGuild Guild, string Role)
        {
            if (string.IsNullOrWhiteSpace(Role))
                return (true, 0);
            UInt64.TryParse(Role.Replace('<', ' ').Replace('>', ' ').Replace('@', ' ').Replace('&', ' ').Replace(" ", ""), out ulong Id);
            var GetRole = Guild.GetRole(Id);
            if (GetRole != null)
                return (true, Id);
            var FindRole = Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == Role.ToLower());
            if (FindRole != null)
                return (true, FindRole.Id);
            return (false, 0);
        }

        public (bool, string) ListCheck<T>(List<T> Collection, object Value, string ObjectName, string CollectionName)
        {
            var check = Collection.Contains((T)Value);
            if (Collection.Contains((T)Value)) return (false, $"`{ObjectName}` already exists in {CollectionName}.");
            if (Collection.Count == Collection.Capacity) return (false, $"Reached max number of entries");
            return (true, $"`{ObjectName}` has been added to {CollectionName}");
        }

        public bool HierarchyCheck(IGuild IGuild, IGuildUser User)
        {
            var Guild = IGuild as SocketGuild;
            var HighestRole = Guild.CurrentUser.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Position;
            return (User as SocketGuildUser).Roles.Any(x => x.Position > HighestRole) ? true : false;
        }
    }
}