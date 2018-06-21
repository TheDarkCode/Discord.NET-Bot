using Discord;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcadesBot
{
    public class ChessStatsHandler
    {
        private IDocumentStore Store { get; }
        public ChessStatsHandler(IDocumentStore store)
            => Store = store;

        public string AddStat(ChessMatchModel chessMatch)
        {
            var id = Guid.NewGuid();
            using (var session = Store.OpenSession())
            {
                while (session.Advanced.Exists($"{id}"))
                    id = Guid.NewGuid();
                session.Store(new ChessMatchStatsModel
                {
                    Id = $"{id}",
                    GuildId = chessMatch.GuildId,
                    Participants =  new [] { chessMatch.ChallengerId, chessMatch.ChallengeeId },
                    CreatedBy = chessMatch.ChallengerId,
                    Winner = chessMatch.Winner,
                    EndBy = chessMatch.EndBy,
                    EndDate = DateTime.Now,
                    MoveCount = (ulong)chessMatch.HistoryList.Count
                });
                session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "Add ChessMatch Stat", $"Added Chess Match With Id: {id}");
            return $"{id}";
        }
    }
}