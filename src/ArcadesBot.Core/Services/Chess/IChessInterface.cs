//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using Discord;

//namespace ArcadesBot
//{
//    public interface IChessService
//    {
//        ulong WhoseTurn(ChessMatch match);
//        Task<ChessMatch> GetMatch(ulong guildId, ulong channel, IUser player);
//        Task<ChessMatchStatus> WriteBoard(ulong? Id, Stream stream);
//        Task<ChessMatchStatus> WriteBoard(ulong guildId, ulong channel, ulong playerId, Stream stream);
//        Task<ChessMatchStatus> Move(Stream stream, ulong guildId, ulong channelId, IUser player, string rawMove);
//        Task<ChessChallenge> Challenge(ulong guildId, ulong channelId, IUser player1, IUser player2, Action<ChessChallenge> onTimeout = null);
//        Task<ChessMatch> AcceptChallenge(CustomCommandContext Context, IUser player);
//        Task<ulong> Resign(ulong guildId, ulong channelId, IUser player);
//    }
//}
