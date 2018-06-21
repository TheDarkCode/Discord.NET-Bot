using Discord;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcadesBot
{
    public class ChessHandler
    {
        private IDocumentStore Store { get; }
        public ChessStatsHandler Stats { get; set; }

        public ChessHandler(IDocumentStore store, ChessStatsHandler stats)
        {
            Store = store;
            Stats = stats;
        }

        public List<ChessMatchModel> GetMatches()
        {
            using (var session = Store.OpenSession())
                return session.Query<ChessMatchModel>().ToList();
        }
        public List<ChessChallengeModel> GetChallenges()
        {
            using (var session = Store.OpenSession())
                return session.Query<ChessChallengeModel>().ToList();
        }
        public List<ChessMatchStatsModel> GetStats(ulong? user = null, ulong? guildId = null)
        {
            using (var session = Store.OpenSession())
            {
                if (user != null)
                    return session.Query<ChessMatchStatsModel>().Where(x => x.Participants.Contains((ulong) user) && x.GuildId == guildId).ToList();

                if (guildId != null)
                    return session.Query<ChessMatchStatsModel>().Where(x => x.GuildId == guildId).ToList();

                return session.Query<ChessMatchStatsModel>().ToList();
            }
                
        }
        public void CompleteMatch(ref ChessMatchModel chessMatch)
        {
            var statId = Stats.AddStat(chessMatch);
            chessMatch.IdOfStat = statId;
            PrettyConsole.Log(LogSeverity.Info, "Complete ChessMatch", $"Completed Chess Match With Id: {chessMatch.Id}");
        }

        public void AddMatch(ulong guildId, ulong channelId, ulong challenger, ulong challengee, string whiteAvatarUrl, string blackAvatarUrl)
        {
            var id = Guid.NewGuid();
            using (var session = Store.OpenSession())
            {
                while (session.Advanced.Exists($"{id}"))
                    id = Guid.NewGuid();
                session.Store(new ChessMatchModel
                {
                    Id = $"{id}",
                    ChallengerId = challenger,
                    ChallengeeId = challengee,
                    ChannelId = channelId,
                    GuildId = guildId,
                    WhiteAvatarUrl = whiteAvatarUrl,
                    BlackAvatarUrl = blackAvatarUrl,
                    HistoryList = new List<ChessMoveModel>()
                });
                session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "Add ChessMatch",  $"Added Chess Match With Id: {id}");
        }

        public void UpdateMatch(ChessMatchModel chessMatch)
        {
            if (chessMatch == null)
                return;
            if (chessMatch.Winner != 1)
            {
                CompleteMatch(ref chessMatch);
            }
            using (var session = Store.OpenSession())
            {
                session.Store(chessMatch, $"{chessMatch.Id}");
                session.SaveChanges();
            }
        }

        public void AddChallenge(ulong guildId, ulong channelId, ulong challenger, ulong challengee, string whiteAvatarUrl, string blackAvatarUrl)
        {
            var id = Guid.NewGuid();
            using (var session = Store.OpenSession())
            {
                while (session.Advanced.Exists($"{id}"))
                    id = Guid.NewGuid();
                session.Store(new ChessChallengeModel
                {
                    Id = $"{id}",
                    ChallengerId = challenger,
                    ChallengeeId = challengee,
                    ChannelId = channelId,
                    GuildId = guildId,
                    Accepted = false,
                    DateCreated = DateTime.Now,
                    TimeoutDate = DateTime.Now.AddMinutes(1)
                });
                session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "Add ChessChallenge", $"Added Chess Challenge With Id: {id}");
        }

        public void UpdateChallenge(ChessChallengeModel chessChallenge)
        {
            if (chessChallenge == null)
                return;
            using (var session = Store.OpenSession())
            {
                session.Store(chessChallenge, $"{chessChallenge.Id}");
                session.SaveChanges();
            }
        }
    }
}