using System;

namespace ArcadesBot
{
    public class GameModel
    {
        public string Id = $"{Guid.NewGuid()}";
        public ulong PlayerId { get; set; }
        public ulong GuildId { get; set; }

        public DeckModel PlayingDeck { get; set; }
        public PlayerModel Player = new PlayerModel();
        public PlayerModel Dealer = new PlayerModel();
    }
}