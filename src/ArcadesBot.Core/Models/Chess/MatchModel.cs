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
        public ChessGame ChessGame { get; set; }
        public List<Move> MoveList { get; set; }
        public List<ChessMoveModel> HistoryList { get; set; }
        public string WhiteAvatarURL { get; set; }
        public string BlackAvatarURL { get; set; }
        public ulong Winner { get; set; } = 1;
        public bool Stalemate { get; set; }
    }
}