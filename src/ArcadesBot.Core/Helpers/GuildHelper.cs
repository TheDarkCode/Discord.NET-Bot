using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace ArcadesBot
{
    public class GuildHelper
    {
        public GuildHelper(GuildHandler guildHandler, DiscordSocketClient client, DatabaseHandler database)
        {
            _client = client;
            _guildHandler = guildHandler;
            _database = database;
        }

        private GuildHandler _guildHandler { get; }
        private DiscordSocketClient _client { get; }
        private DatabaseHandler _database { get; }

        public IMessageChannel DefaultChannel(ulong guildId)
        {
            var guild = _client.GetGuild(guildId);
            return guild.TextChannels.FirstOrDefault(x =>
                       x.Name.Contains("general") || x.Name.Contains("lobby") || x.Id == guild.Id) ??
                   guild.DefaultChannel;
        }

        public UserProfile GetProfile(ulong guildId, ulong userId)
        {
            var guild = _guildHandler.GetGuild(guildId);
            if (guild.Profiles.ContainsKey(userId))
                return guild.Profiles[userId];
            guild.Profiles.Add(userId, new UserProfile());
            _database.Update<GuildModel>(guildId, guild);
            return guild.Profiles[userId];
        }

        public bool ToggleBlackList(GuildModel guild, ulong channelId)
        {
            if (!guild.BlackListedChannels.Contains(channelId))
            {
                guild.BlackListedChannels.Add(channelId);
                _database.Update<GuildModel>(guild.Id, guild);
                return true;
            }

            guild.BlackListedChannels.Remove(channelId);
            _database.Update<GuildModel>(guild.Id, guild);
            return false;
        }

        public void SaveProfile(ulong guildId, ulong userId, UserProfile profile)
        {
            var data = _guildHandler.GetGuild(guildId);
            data.Profiles[userId] = profile;
            _database.Update<GuildModel>(data.Id, data);
        }

        public (bool, ulong) GetChannelId(SocketGuild guild, string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                return (true, 0);
            ulong.TryParse(channel.Replace('<', ' ').Replace('>', ' ').Replace('#', ' ').Replace(" ", ""), out var id);
            var getChannel = guild.GetTextChannel(id);
            if (getChannel != null)
                return (true, id);
            var findChannel = guild.TextChannels.FirstOrDefault(x => x.Name == channel.ToLower());
            return findChannel != null ? (true, findChannel.Id) : ((bool, ulong)) (false, 0);
        }

        public (bool, ulong) GetRoleId(SocketGuild guild, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return (true, 0);
            ulong.TryParse(
                role.Replace('<', ' ').Replace('>', ' ').Replace('@', ' ').Replace('&', ' ').Replace(" ", ""),
                out var id);
            var getRole = guild.GetRole(id);
            if (getRole != null)
                return (true, id);
            var findRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());
            if (findRole != null)
                return (true, findRole.Id);
            return (false, 0);
        }

        public (bool Added, string Message) ListCheck<T>(List<T> collection, object value, string objectName,
            string collectionName)
        {
            if (collection.Contains((T) value))
                return (false, $"{objectName} already exists in {collectionName}.");
            if (collection.Count == collection.Capacity)
                return (false, "Reached max number of entries");

            return (true, $"`{objectName}` has been added to {collectionName}");
        }

        public bool HierarchyCheck(IGuild guild, IGuildUser user)
        {
            var guildSocket = guild as SocketGuild;
            var highestRole = guildSocket.CurrentUser.Roles.OrderByDescending(x => x.Position).FirstOrDefault()
                .Position;
            return (user as SocketGuildUser).Roles.Any(x => x.Position > highestRole);
        }
    }
}