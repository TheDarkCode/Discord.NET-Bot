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
        private ChessHandler _chessHandler { get; }
        private DiscordSocketClient _client { get; }
        public ChessHelper(ChessHandler chessHandler, DiscordSocketClient client)
        {
            _client = client;
            _chessHandler = chessHandler;
        }

        public ChessChallengeModel GetChallenge(ulong guildId, ulong channelId, ulong invokerId)
            =>  _chessHandler.GetChallenges().Where(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId)).OrderByDescending(x => x.TimeoutDate).FirstOrDefault();

        public bool CheckPlayerInMatch(ulong guildId, ulong invokerId)
           =>  _chessHandler.GetMatches().Any(x => x.GuildId == guildId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == 1);

        public ChessMatchModel GetMatch(ulong guildId, ulong channelId, ulong invokerId)
            =>  _chessHandler.GetMatches().FirstOrDefault(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == 1);

        public ChessMatchModel GetMatch(Guid? Id)
            =>  _chessHandler.GetMatches().FirstOrDefault(x => x.Id == $"{Id}");

        public ChessChallengeModel GetChallenge(Guid? Id)
            => _chessHandler.GetChallenges().FirstOrDefault(x => x.Id == $"{Id}");

        public ChessChallengeModel CreateChallenge(ulong guildId, ulong channelId, IUser challenger, IUser challengee)
        {
            _chessHandler.AddChallenge(guildId, channelId, challenger.Id, challengee.Id, challenger.GetAvatarUrl(), challengee.GetAvatarUrl());
            return GetChallenge(guildId, channelId, challenger.Id);
        }

        public void UpdateChessGame(ChessMatchStatusModel matchStatus)
        {
            Player player = matchStatus.Game.WhoseTurn == Player.White ? Player.Black : Player.White;

            if (matchStatus.WinnerId != null)
                matchStatus.Match.Winner = (ulong)matchStatus.WinnerId;

            if (matchStatus.Game.IsStalemated(player))
                matchStatus.Match.Stalemate = true;

            _chessHandler.UpdateMatch(matchStatus.Match);
        }

        public ChessMatchModel AcceptChallenge(ChessChallengeModel challenge, string blackURL, string whiteURL)
        {
            challenge.Accepted = true;
            _chessHandler.UpdateChallenge(challenge);
            _chessHandler.AddMatch(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId, challenge.ChallengeeId, whiteURL, blackURL);
            return GetMatch(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId);
        }

        public ulong Resign(ulong guildId, ulong channelId, ulong invokerId)
        {

            ChessMatchModel chessMatch = GetMatch(guildId, channelId, invokerId);
            chessMatch.Winner = chessMatch.ChallengeeId == invokerId ? chessMatch.ChallengerId : chessMatch.ChallengeeId;

            _chessHandler.UpdateMatch(chessMatch);
            return chessMatch.Winner;
        }
    }
}
