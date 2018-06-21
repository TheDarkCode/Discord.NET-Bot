using ChessDotNet;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class ChessHelper
    {
        private ChessStatsHandler ChessStatsHandler { get; }
        private ChessHandler ChessHandler { get; }
        private DiscordSocketClient Client { get; }
        public ChessHelper(ChessHandler chessHandler, DiscordSocketClient client, ChessStatsHandler chessStatsHandler)
        {
            Client = client;
            ChessHandler = chessHandler;
            ChessStatsHandler = chessStatsHandler;
        }

        public IEnumerable<ChessMatchStatsModel> GetStatsFromUser(ulong userId, ulong guildId)
            => ChessHandler.GetStats(userId, guildId);

        public IEnumerable<ChessMatchStatsModel> GetStatsByGuild(ulong guildId)
            => ChessHandler.GetStats(guildId: guildId);

        public IEnumerable<ChessMatchStatsModel> GetGlobalStats()
            => ChessHandler.GetStats();

        public ChessChallengeModel GetChallenge(ulong guildId, ulong channelId, ulong invokerId)
            =>  ChessHandler.GetChallenges().Where(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId)).OrderByDescending(x => x.TimeoutDate).FirstOrDefault();

        public bool CheckPlayerInMatch(ulong guildId, ulong invokerId)
           =>  ChessHandler.GetMatches().Any(x => x.GuildId == guildId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == 1);

        public ChessMatchModel GetMatch(ulong guildId, ulong channelId, ulong invokerId)
            =>  ChessHandler.GetMatches().FirstOrDefault(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == 1);

        public ChessMatchModel GetMatchByStatId(string id)
            =>  ChessHandler.GetMatches().FirstOrDefault(x => x.IdOfStat == id);



        public ChessChallengeModel GetChallenge(string id)
            => ChessHandler.GetChallenges().FirstOrDefault(x => x.Id == id);

        public ChessChallengeModel CreateChallenge(ulong guildId, ulong channelId, IUser challenger, IUser challengee)
        {
            ChessHandler.AddChallenge(guildId, channelId, challenger.Id, challengee.Id, challenger.GetAvatarUrl(), challengee.GetAvatarUrl());
            return GetChallenge(guildId, channelId, challenger.Id);
        }

        public void UpdateChessGame(ChessMatchStatusModel matchStatus)
        {
            var match = matchStatus.Match;

            switch (matchStatus.Status)
            {
                case Cause.Checkmate:
                    // ReSharper disable once PossibleInvalidOperationException
                    match.Winner = (ulong)matchStatus.WinnerId;
                    match.EndBy = Cause.Checkmate;
                    break;
                case Cause.Stalemate:
                    match.Winner = 0;
                    match.EndBy = Cause.Stalemate;
                    break;
            }

            ChessHandler.UpdateMatch(match);
        }

        public ChessMatchModel AcceptChallenge(ChessChallengeModel challenge, string blackUrl, string whiteUrl)
        {
            challenge.Accepted = true;
            ChessHandler.UpdateChallenge(challenge);
            ChessHandler.AddMatch(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId, challenge.ChallengeeId, whiteUrl, blackUrl);
            return GetMatch(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId);
        }

        public ulong Resign(ulong guildId, ulong channelId, ulong invokerId)
        {

            var chessMatch = GetMatch(guildId, channelId, invokerId);
            chessMatch.Winner = chessMatch.ChallengeeId == invokerId 
                ? chessMatch.ChallengerId 
                : chessMatch.ChallengeeId;
            chessMatch.EndBy = Cause.Resign;

            ChessHandler.UpdateMatch(chessMatch);
            return chessMatch.Winner;
        }
    }
}
