namespace ArcadesBot
{
    public class GuildHandler
    {
        public GuildHandler(DatabaseHandler database) => _database = database;

        private DatabaseHandler _database { get; }

        public GuildModel GetGuild(ulong id) 
            => _database.Select<GuildModel>($"{id}");

        public void RemoveGuild(ulong id) 
            => _database.Delete<GuildModel>(id);

        public void AddGuild(ulong id)
        {
            var refId = $"{id}";
            _database.Create<GuildModel>(ref refId, new GuildModel {Id = $"{id}", Prefix = "%"});
        }
    }
}