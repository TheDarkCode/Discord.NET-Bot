using System.Diagnostics.CodeAnalysis;
using ArcadesBot.Models.BlackJack;

namespace ArcadesBot.Handlers
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class BlackJackHandler
    {
        public BlackJackHandler(DatabaseHandler database) 
            => _database = database;

        private DatabaseHandler _database { get; }

        public GameModel GetGame(ulong id)
            => _database.Select<GameModel>($"{id}");

        public void DeleteGame(ulong id)
            => _database.Delete<GameModel>(id);

        public void AddGame(ulong id)
        {
            var refId = $"{id}";
            _database.Create<GameModel>(ref refId, new GameModel());
        }
    }
}