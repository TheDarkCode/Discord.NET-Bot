using Discord;
using Raven.Client.Documents;

namespace ArcadesBot
{
    public class GuildHandler
    {
        DatabaseHandler _database { get; }
        public GuildHandler(DatabaseHandler database) 
            => _database = database;

        public GuildModel GetGuild(ulong id) 
            => _database.Select<GuildModel>(id: $"{id}");

        public void RemoveGuild(ulong id) 
            => _database.Delete<GuildModel>(id: id);

        public void AddGuild(ulong id)
        {
            string refId = $"{id}";
            _database.Create<GuildModel>(ref refId, new GuildModel { Id = $"{id}", Prefix = "%" });
        }
            
    }
}