using System.Drawing;
using Raven.Client.Documents;
using Discord;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ArcadesBot
{
    public class ChessHandler
    {
        private IDocumentStore Store { get; }
        public ChessHandler(IDocumentStore store)
            => Store = store;

        public List<ChessMatchModel> GetMatches()
        {
            using (var Session = Store.OpenSession())
                return Session.Query<ChessMatchModel>().ToList();
        }
        public List<ChessChallengeModel> GetChallenges()
        {
            using (var Session = Store.OpenSession())
                return Session.Query<ChessChallengeModel>().ToList();
        }
        public void CompleteMatch(ChessMatchModel chessMatch)
        {
            using (var Session = Store.OpenSession())
                Session.Delete($"{chessMatch.Id}");
            PrettyConsole.Log(LogSeverity.Info, "Remove ChessMatch",  $"Removed Chess Match With Id: {chessMatch.Id}");
        }

        public void AddMatch(ulong guildId, ulong channelId, ulong challenger, ulong challengee, string whiteAvatarURL, string blackAvatarURL)
        {
            var id = Guid.NewGuid();
            using (var Session = Store.OpenSession())
            {
                while (Session.Advanced.Exists($"{id}"))
                    id = Guid.NewGuid();
                Session.Store(new ChessMatchModel
                {
                    Id = $"{id}",
                    ChallengerId = challenger,
                    ChallengeeId = challengee,
                    ChannelId = channelId,
                    GuildId = guildId,
                    WhiteAvatarURL = whiteAvatarURL,
                    BlackAvatarURL = blackAvatarURL,
                    HistoryList = new List<ChessMoveModel>()
                });
                Session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "Add ChessMatch",  $"Added Chess Match With Id: {id}");
        }

        public void UpdateMatch(ChessMatchModel chessMatch)
        {
            if (chessMatch == null)
                return;
            using (var Session = Store.OpenSession())
            {
                Session.Store(chessMatch, $"{chessMatch.Id}");
                Session.SaveChanges();
            }
        }

        public void AddChallenge(ulong guildId, ulong channelId, ulong challenger, ulong challengee, string whiteAvatarURL, string blackAvatarURL)
        {
            var id = Guid.NewGuid();
            using (var Session = Store.OpenSession())
            {
                while (Session.Advanced.Exists($"{id}"))
                    id = Guid.NewGuid();
                Session.Store(new ChessChallengeModel
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
                Session.SaveChanges();
            }
            PrettyConsole.Log(LogSeverity.Info, "Add ChessChallenge", $"Added Chess Challenge With Id: {id}");
        }

        public void UpdateChallenge(ChessChallengeModel chessChallenge)
        {
            if (chessChallenge == null)
                return;
            using (var Session = Store.OpenSession())
            {
                Session.Store(chessChallenge, $"{chessChallenge.Id}");
                Session.SaveChanges();
            }
        }
    }
}