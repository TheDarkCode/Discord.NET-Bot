using Discord;
using Raven.Client.Documents;

namespace ArcadesBot
{
    public class GuildHandler
    {
        DatabaseHandler Store { get; }
        public GuildHandler(DatabaseHandler store) 
            => Store = store;

        public GuildModel GetGuild(ulong id) 
            => Store.Select<GuildModel>(id: $"{id}");

        public void RemoveGuild(ulong id) 
            => Store.Delete<GuildModel>(id: id);

        public void AddGuild(ulong id)
        {
            string refId = $"{id}";
            Store.Create<GuildModel>(ref refId, new GuildModel { Id = $"{id}", Prefix = "%" });
        }
            
    }
}