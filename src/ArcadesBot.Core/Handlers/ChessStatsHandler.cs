using System;
using ArcadesBot.Models.Chess;

namespace ArcadesBot.Handlers
{
    public class ChessStatsHandler
    {
        public ChessStatsHandler(DatabaseHandler database)
        {
            _database = database;
        }

        private DatabaseHandler _database { get; }

        public string AddStat(ChessMatchModel chessMatch)
        {
            var id = $"{Guid.NewGuid()}";
            return _database.Create<ChessMatchStatsModel>(ref id, new ChessMatchStatsModel
            {
                Id = id,
                GuildId = chessMatch.GuildId,
                Participants = new[] {chessMatch.ChallengerId, chessMatch.ChallengeeId},
                CreatedBy = chessMatch.ChallengerId,
                Winner = chessMatch.Winner,
                EndBy = chessMatch.EndBy,
                EndDate = DateTime.Now,
                MoveCount = (ulong) chessMatch.HistoryList.Count
            }).Id;
        }
    }
}