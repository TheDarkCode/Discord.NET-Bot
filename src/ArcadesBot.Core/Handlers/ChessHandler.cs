using Discord;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcadesBot
{
    public class ChessHandler
    {
        public ChessHandler(DatabaseHandler databaseHandler, ChessStatsHandler stats)
        {
            Database = databaseHandler;
            StatsHandler = stats;
        }
        private DatabaseHandler Database { get; }
        private ChessStatsHandler StatsHandler { get; }


        public List<ChessMatchModel> Matches 
            => Database.Query<ChessMatchModel>();
        public List<ChessChallengeModel> Challenges 
            => Database.Query<ChessChallengeModel>();
        public List<ChessMatchStatsModel> Stats 
            => Database.Query<ChessMatchStatsModel>();

        #region Public Methods
        public void CompleteMatch(ref ChessMatchModel chessMatch)
        {
            var statId = StatsHandler.AddStat(chessMatch);
            chessMatch.IdOfStat = statId;
            PrettyConsole.Log(LogSeverity.Info, "Complete ChessMatch", $"Completed Chess Match With Id: {chessMatch.Id}");
        }

        public void AddMatch(ulong guildId, ulong channelId, ulong challenger, ulong challengee, string whiteAvatarUrl, string blackAvatarUrl)
        {
            var id = Guid.NewGuid().ToString();
            Database.Create<ChessMatchModel>(ref id, new ChessMatchModel
            {
                ChallengerId = challenger,
                ChallengeeId = challengee,
                ChannelId = channelId,
                GuildId = guildId,
                WhiteAvatarUrl = whiteAvatarUrl,
                BlackAvatarUrl = blackAvatarUrl,
                HistoryList = new List<ChessMoveModel>()
            });
        }

        public void UpdateMatch(ChessMatchModel chessMatch)
        {
            if (chessMatch == null)
                return;
            if (chessMatch.Winner != 1)
            {
                CompleteMatch(ref chessMatch);
            }
            Database.Update<ChessMatchModel>($"{chessMatch.Id}", chessMatch);
        }

        public void AddChallenge(ChessChallengeModel challenge)
        {
            var id = Guid.NewGuid().ToString();
            Database.Create<ChessChallengeModel>(ref id, challenge);
        }

        public void UpdateChallenge(ChessChallengeModel chessChallenge)
        {
            if (chessChallenge == null)
                return;
            Database.Update<ChessChallengeModel>($"{chessChallenge.Id}", chessChallenge);
        }
        public void RemoveChallenge(ChessChallengeModel chessChallenge)
        {
            if (chessChallenge == null)
                return;
            Database.Delete<ChessChallengeModel>($"{chessChallenge.Id}");
        }
        #endregion
    }
}