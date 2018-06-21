using System;

namespace ArcadesBot
{
    public class ChessMatchStatsModel
    {
        public string Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong[] Participants { get; set; }
        public ulong Winner { get; set; }
        public Cause EndBy { get; set; }
        public DateTime EndDate { get; set; }
        public ulong MoveCount { get; set; }
        public ulong CreatedBy { get; set; }
    }
    public enum Cause 
    {
        OnGoing,
        Resign,
        Stalemate,
        Checkmate
    }
}