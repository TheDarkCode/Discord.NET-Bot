using Discord;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcadesBot
{
    public class ChessStatsHandler
    {
        private DatabaseHandler Database { get; }
        public ChessStatsHandler(DatabaseHandler database)
            => Database = database;

        public string AddStat(ChessMatchModel chessMatch)
        {
            var id = $"{Guid.NewGuid()}";
            return Database.Create<ChessMatchStatsModel>(ref id, new ChessMatchStatsModel
            {
                Id = id,
                GuildId = chessMatch.GuildId,
                Participants = new[] { chessMatch.ChallengerId, chessMatch.ChallengeeId },
                CreatedBy = chessMatch.ChallengerId,
                Winner = chessMatch.Winner,
                EndBy = chessMatch.EndBy,
                EndDate = DateTime.Now,
                MoveCount = (ulong)chessMatch.HistoryList.Count
            }).Id;
        }
    }
}