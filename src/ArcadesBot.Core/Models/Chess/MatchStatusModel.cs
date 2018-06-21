using ChessDotNet;
using Discord;

namespace ArcadesBot
{
    public class ChessMatchStatusModel
    {
        public ChessMatchModel Match { get; set; }
        public ChessGame Game { get; set; }
        public string ImageLink { get; set; }
        public bool IsCheck { get; set; }
        public ulong? WinnerId { get; set; }
        public ulong NextPlayerId { get; set; }
        public Cause Status { get; set; }
        public bool IsCheckmated { get; set; }
    }
}
