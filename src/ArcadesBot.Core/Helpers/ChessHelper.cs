using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace ArcadesBot
{
    public class ChessHelper
    {
        public ChessHelper(ChessHandler chessHandler) 
            => _chessHandler = chessHandler;

        private ChessHandler _chessHandler { get; }

        public List<ChessMatchStatsModel> Stats
            => _chessHandler.Stats;

        public List<ChessMatchStatsModel> GetStatsFromUser(ulong userId, ulong guildId) 
            => Stats.Where(x => x.Participants.Contains(userId) && x.GuildId == guildId).ToList();

        public ChessChallengeModel GetChallenge(ulong guildId, ulong channelId, ulong invokerId, bool overRide = false)
        {
            if (overRide)
                return _chessHandler.Challenges
                    .Where(x => x.GuildId == guildId && x.ChannelId == channelId && x.ChallengeeId == invokerId)
                    .OrderByDescending(x => x.TimeoutDate).FirstOrDefault();
            return _chessHandler.Challenges
                .Where(x => x.GuildId == guildId && x.ChannelId == channelId &&
                            (x.ChallengeeId == invokerId || x.ChallengerId == invokerId))
                .OrderByDescending(x => x.TimeoutDate).FirstOrDefault();
        }


        public bool CheckPlayerInMatch(ulong guildId, ulong invokerId) 
            => _chessHandler.Matches.Any(x => x.GuildId == guildId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == 1);

        public ChessMatchModel GetMatch(ulong guildId, ulong channelId, ulong invokerId) 
            => _chessHandler.Matches.FirstOrDefault(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == 1);

        public ChessMatchModel GetMatchByStatId(string id) 
            => _chessHandler.Matches.FirstOrDefault(x => x.IdOfStat == id);


        public ChessChallengeModel GetChallenge(string id) 
            => _chessHandler.Challenges.FirstOrDefault(x => x.Id == id);

        public ChessChallengeModel CreateChallenge(ulong guildId, ulong channelId, IUser challenger, IUser challengee)
        {
            var challenge = new ChessChallengeModel
            {
                ChallengerId = challenger.Id,
                ChallengeeId = challengee.Id,
                ChannelId = channelId,
                GuildId = guildId,
                Accepted = false,
                DateCreated = DateTime.Now,
                TimeoutDate = DateTime.Now.AddMinutes(1)
            };
            _chessHandler.AddChallenge(challenge);
            return GetChallenge(guildId, channelId, challenger.Id);
        }

        public void UpdateChessGame(ChessMatchStatusModel matchStatus)
        {
            var match = matchStatus.Match;
            switch (matchStatus.Status)
            {
                case Cause.Checkmate:
                    match.Winner = (ulong) matchStatus.WinnerId;
                    match.EndBy = Cause.Checkmate;
                    break;
                case Cause.Stalemate:
                    match.Winner = 0;
                    match.EndBy = Cause.Stalemate;
                    break;
                case Cause.OnGoing:
                    break;
                case Cause.Resign:
                    break;
            }

            _chessHandler.UpdateMatch(match);
        }

        public ChessMatchModel AcceptChallenge(ChessChallengeModel challenge, string blackUrl, string whiteUrl)
        {
            challenge.Accepted = true;
            _chessHandler.UpdateChallenge(challenge);
            _chessHandler.AddMatch(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId,
                challenge.ChallengeeId, whiteUrl, blackUrl);
            return GetMatch(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId);
        }

        public ulong Resign(ulong guildId, ulong channelId, ulong invokerId)
        {
            var chessMatch = GetMatch(guildId, channelId, invokerId);
            chessMatch.Winner = chessMatch.ChallengeeId == invokerId
                ? chessMatch.ChallengerId
                : chessMatch.ChallengeeId;
            chessMatch.EndBy = Cause.Resign;

            _chessHandler.UpdateMatch(chessMatch);
            return chessMatch.Winner;
        }
    }
}