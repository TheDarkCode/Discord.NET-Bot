using Discord;
using System;
using System.Collections.Generic;

namespace ArcadesBot
{
    public class ChessHandler
    {
        public ChessHandler(DatabaseHandler databaseHandler, ChessStatsHandler stats)
        {
            _database = databaseHandler;
            _statsHandler = stats;
        }

        private DatabaseHandler _database { get; }
        private ChessStatsHandler _statsHandler { get; }


        public List<ChessMatchModel> Matches
            => _database.Query<ChessMatchModel>();

        public List<ChessChallengeModel> Challenges
            => _database.Query<ChessChallengeModel>();

        public List<ChessMatchStatsModel> Stats
            => _database.Query<ChessMatchStatsModel>();

        #region Public Methods

        public void CompleteMatch(ref ChessMatchModel chessMatch)
        {
            var statId = _statsHandler.AddStat(chessMatch);
            chessMatch.IdOfStat = statId;
            PrettyConsole.Log(LogSeverity.Info, "Complete ChessMatch",
                $"Completed Chess Match With Id: {chessMatch.Id}");
        }

        public void AddMatch(ulong guildId, ulong channelId, ulong challenger, ulong challengee, string whiteAvatarUrl,
            string blackAvatarUrl)
        {
            var id = Guid.NewGuid().ToString();
            _database.Create<ChessMatchModel>(ref id, new ChessMatchModel
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
            if (chessMatch.Winner != 1) CompleteMatch(ref chessMatch);
            _database.Update<ChessMatchModel>($"{chessMatch.Id}", chessMatch);
        }

        public void AddChallenge(ChessChallengeModel challenge)
        {
            var id = Guid.NewGuid().ToString();
            _database.Create<ChessChallengeModel>(ref id, challenge);
        }

        public void UpdateChallenge(ChessChallengeModel chessChallenge)
        {
            if (chessChallenge == null)
                return;
            _database.Update<ChessChallengeModel>($"{chessChallenge.Id}", chessChallenge);
        }

        public void RemoveChallenge(ChessChallengeModel chessChallenge)
        {
            if (chessChallenge == null)
                return;
            _database.Delete<ChessChallengeModel>($"{chessChallenge.Id}");
        }

        #endregion
    }
}