using ChessDotNet;
using System;
using System.Collections.Generic;

namespace ArcadesBot
{
    public class ChessMatchModel
    {

        public string Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ChallengerId { get; set; }
        public ulong ChallengeeId { get; set; }
        public List<ChessMoveModel> HistoryList { get; set; }
        public string WhiteAvatarUrl { get; set; }
        public string BlackAvatarUrl { get; set; }
        public ulong Winner { get; set; } = 1;
        public Cause EndBy { get; set; } = Cause.OnGoing;
        public string IdOfStat { get; set; }
    }
}