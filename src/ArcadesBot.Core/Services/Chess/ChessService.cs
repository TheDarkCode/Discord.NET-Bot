// Decompiled with JetBrains decompiler
// Type: ArcadesBot.ChessService
// Assembly: ArcadesBot.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 391C6D21-3115-439D-A2A7-9E633410D780
// Assembly location: C:\Users\Arcade\Desktop\ArcadesBot.Core.dll

using ChessDotNet;
using Discord;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class ChessService : IChessService
    {
        private readonly IAssetService _assetService;
        private readonly ChessDatabase _chessDatabase;
        private readonly ConfigDatabase _configDatabase;

        public ChessService(IAssetService assetService, ChessDatabase chessDatabase, ConfigDatabase configDatabase)
        {
            _configDatabase = configDatabase;
            _chessDatabase = chessDatabase;
            _assetService = assetService;
        }

        public ulong WhoseTurn(ChessMatch match)
        {
            if (match.ChessGame.WhoseTurn != Player.White)
                return match.ChallengeeId;
            return match.ChallengerId;
        }

        public async Task<ChessMatch> GetMatch(ulong guildId, ulong channelId, IUser player)
        {
            return await _chessDatabase.GetMatchAsync(guildId, channelId, player.Id);
        }

        private void DrawImage(IImageProcessingContext<Rgba32> processor, string name, int x, int y)
        {
            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath($"{name}.png"));
            processor.DrawImage(image, new Size(50, 50), new Point(x * 50 + 117, y * 50 + 19), new GraphicsOptions());
        }

        public async Task<ChessMatchStatus> WriteBoard(ulong? Id, Stream stream)
        {
            ChessMatch match = null;
            match = await _chessDatabase.GetMatchAsync(Id);
            if (match.ChessGame == null && match.MoveList == null)
                throw new ChessException("This is not a number that belongs to a match");
            ChessMatchStatus chessMatchStatus;
            if (match.MoveList == null)
            {
                Player otherPlayer = match.ChessGame.WhoseTurn == Player.White ? Player.Black : Player.White;
                bool checkMated = match.ChessGame.IsCheckmated(Player.White) || match.ChessGame.IsCheckmated(Player.Black);
                bool isOver = checkMated || match.ChessGame.IsStalemated(otherPlayer);
                var linkFromMatchAsync = await GetImageLinkFromMatchAsync(match, stream);
                chessMatchStatus = new ChessMatchStatus()
                {
                    ImageId = linkFromMatchAsync.ImageId,
                    NextPlayerId = linkFromMatchAsync.NextPlayerId,
                    IsOver = isOver,
                    IsCheck = match.ChessGame.IsInCheck(otherPlayer),
                    IsCheckmated = checkMated
                };
            }
            else
                chessMatchStatus = new ChessMatchStatus()
                {
                    ImageId = string.Format("http://128.199.35.40/board{0}-{1}.png", match.Id, match.MoveList.Count),
                    IsOver = true,
                    WinnerId = match.Winner
                };
            return chessMatchStatus;
        }

        public async Task<ChessMatchStatus> WriteBoard(ulong guildId, ulong channelId, ulong playerId, Stream stream)
        {
            ChessService chessService = this;
            ChessMatch match = null;
            match = await chessService._chessDatabase.GetMatchAsync(guildId, channelId, playerId);
            if (match == null)
                throw new ChessException("You are not in a game.");
            Player otherPlayer = match.ChessGame.WhoseTurn == Player.White ? Player.Black : Player.White;
            bool isOver = match.ChessGame.IsCheckmated(otherPlayer) || match.ChessGame.IsStalemated(otherPlayer);
            ChessMatchStatus linkFromMatchAsync = await chessService.GetImageLinkFromMatchAsync(match, stream);
            return new ChessMatchStatus()
            {
                ImageId = linkFromMatchAsync.ImageId,
                NextPlayerId = linkFromMatchAsync.NextPlayerId,
                IsOver = isOver,
                IsCheck = match.ChessGame.IsInCheck(otherPlayer),
                IsCheckmated = match.ChessGame.IsCheckmated(otherPlayer)
            };
        }


        private async Task<ChessMatchStatus> GetImageLinkFromMatchAsync(ChessMatch match, Stream stream)
        {
            ChessMove lastMove;
            await Task.Run(async () =>
            {
                var board = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("board.png"));

                HttpClient httpClient = new HttpClient();
                var turnIndicator = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("turn_indicator.png"));
                var whiteAvatarData = SixLabors.ImageSharp.Image.Load(await httpClient.GetByteArrayAsync(match.WhiteAvatarURL));
                var blackAvatarData = SixLabors.ImageSharp.Image.Load(await httpClient.GetByteArrayAsync(match.BlackAvatarURL));
                httpClient.Dispose();

                Piece[][] boardPieces = match.ChessGame.GetBoard();

                lastMove = match.HistoryList.OrderByDescending((x => x.MoveDate)).FirstOrDefault();
                var RankToRowMap = new Dictionary<int, int>()
                {
                    {1, 7},
                    {2, 6},
                    {3, 5},
                    {4, 4},
                    {5, 3},
                    {6, 2},
                    {7, 1},
                    {8, 0}
                };
                var turnIndicatorPoint = match.ChessGame.WhoseTurn != Player.Black ? new Point(538, 367) : new Point(40, 15);
                board.Mutate(processor =>
                {
                    #region Mutating the board
                    for (int x = 0; x < boardPieces.Length; ++x)
                    {
                        for (int y = 0; y < boardPieces[x].Length; ++y)
                        {
                            if (lastMove != null && (lastMove.Move.OriginalPosition.File == (ChessDotNet.File)x && RankToRowMap[lastMove.Move.OriginalPosition.Rank] == y
                            || lastMove.Move.NewPosition.File == (ChessDotNet.File)x && RankToRowMap[lastMove.Move.NewPosition.Rank] == y))
                            {
                                DrawImage(processor, "yellow_square", x, y);
                            }
                            Piece piece = boardPieces[y][x];
                            if (piece != (Piece)null)
                            {
                                if (piece.GetFenCharacter().ToString().ToUpper() == "K" && match.ChessGame.IsInCheck(piece.Owner))
                                {
                                    DrawImage(processor, "red_square", x, y);
                                }
                                string str = "white";
                                if (new char[6] { 'r', 'n', 'b', 'q', 'k', 'p' }.Contains(piece.GetFenCharacter()))
                                    str = "black";
                                DrawImage(processor, string.Format("{0}_{1}", str, piece.GetFenCharacter()), x, y);
                            }
                        }
                    }
                    var BlackPawnCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'p' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhitePawnCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'P' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackRookCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'r' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteRookCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'R' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackKnightCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'n' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteKnightCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'N' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackBishopCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'b' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteBishopCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'B' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackQueenCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'q' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteQueenCount = match.ChessGame.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'Q' && x.Owner == Player.White && !x.IsPromotionResult);
                    int Row = 1;
                    Image<Rgba32> BlackPawn = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("black_p.png"));
                    
                    for (int index = 8; index > BlackPawnCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage<Rgba32>(BlackPawn, new Size(30, 30), new Point(533, 16 + Row * 30), new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(BlackPawn, new Size(30, 30), new Point(566, 16 + Row * 30), new GraphicsOptions());
                            ++Row;
                        }
                    }
                    Row = 8;

                    Image<Rgba32> image2 = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("white_P.png"));
                    for (int index = 8; index > WhitePawnCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(image2, new Size(30, 30), new Point(20, 125 + Row * 30), new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(image2, new Size(30, 30), new Point(53, 125 + Row * 30), new GraphicsOptions());
                            --Row;
                        }
                    }
                    Row = 5;

                    Image<Rgba32> image3 = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("black_r.png"));
                    for (int index = 2; index > Row; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(image3, new Size(30, 30), new Point(533, 16 + Row * 30), new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(image3, new Size(30, 30), new Point(566, 16 + Row * 30), new GraphicsOptions());
                            ++Row;
                        }
                    }
                    Row = 4;

                    Image<Rgba32> WhiteRook = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("white_R.png"));
                    for (int index = 2; index > WhiteRookCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(WhiteRook, new Size(30, 30), new Point(20, 125 + Row * 30), new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(WhiteRook, new Size(30, 30), new Point(53, 125 + Row * 30), new GraphicsOptions());
                            --Row;
                        }
                    }
                    Row = 6;

                    Image<Rgba32> BlackKnight = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("black_n.png"));
                    for (int index = 2; index > BlackKnightCount; --index)
                    {
                        if (index % 2 == 0)
                            processor.DrawImage(BlackKnight, new Size(30, 30), new Point(533, 16 + Row * 30), new GraphicsOptions());
                        else
                        {
                            processor.DrawImage(BlackKnight, new Size(30, 30), new Point(566, 16 + Row * 30), new GraphicsOptions());
                            ++Row;
                        }
                    }
                    Row = 3;

                    Image<Rgba32> WhiteKnight = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("white_N.png"));
                    for (int i = 2; i > WhiteKnightCount; --i)
                    {
                        if (i % 2 == 0)
                            processor.DrawImage(WhiteKnight, new Size(30, 30), new Point(20, 125 + Row * 30), new GraphicsOptions());
                        else
                        {
                            processor.DrawImage(WhiteKnight, new Size(30, 30), new Point(53, 125 + Row * 30), new GraphicsOptions());
                            --Row;
                        }
                    }
                    Row = 7;

                    var BlackBishop = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("black_b.png"));
                    for (int index = 2; index > Row; --index)
                    {
                        if (index % 2 == 0)
                            processor.DrawImage(BlackBishop, new Size(30, 30), new Point(533, 16 + Row * 30), new GraphicsOptions());
                        else
                        {
                            processor.DrawImage(BlackBishop, new Size(30, 30), new Point(566, 16 + Row * 30), new GraphicsOptions());
                            ++Row;
                        }
                    }
                    Row = 2;

                    Image<Rgba32> WhiteBishop = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("white_B.png"));
                    for (int index = 2; index > WhiteBishopCount; --index)
                    {
                        if (index % 2 == 0)
                            processor.DrawImage(WhiteBishop, new Size(30, 30), new Point(20, 125 + Row * 30), new GraphicsOptions());
                        else
                        {
                            processor.DrawImage(WhiteBishop, new Size(30, 30), new Point(53, 125 + Row * 30), new GraphicsOptions());
                            --Row;
                        }
                    }
                    Row = 8;

                    var BlackQueen = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("black_q.png"));
                    if (BlackQueenCount == 0)
                        processor.DrawImage(BlackQueen, new Size(30, 30), new Point(533, 16 + Row * 30), new GraphicsOptions());
                    Row = 1;

                    var WhiteQueen = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("white_Q.png"));
                    if (WhiteQueenCount == 0)
                        processor.DrawImage(WhiteQueen, new Size(30, 30), new Point(20, 125 + Row * 30), new GraphicsOptions());

                    processor.DrawImage(turnIndicator, new Size(56, 56), turnIndicatorPoint, new GraphicsOptions());
                    processor.DrawImage(whiteAvatarData, new Size(50, 50), new Point(541, 370), new GraphicsOptions());
                    processor.DrawImage(blackAvatarData, new Size(50, 50), new Point(43, 18), new GraphicsOptions());
                    #endregion
                });
                board.Save($"/var/www/html/board{match.Id}-{match.HistoryList.Count}.png");
            });
            return new ChessMatchStatus()
            {
                ImageId = $"http://128.199.35.40/board{match.Id}-{match.HistoryList.Count}.png",
                NextPlayerId = WhoseTurn(match)
            };
        }

        public async Task<ChessChallenge> Challenge(ulong guildId, ulong channelId, IUser player1, IUser player2, Action<ChessChallenge> onTimeout = null)
        {
            if (player1.Equals(player2))
                throw new ChessException("You can't challenge yourself.");
            if (await _chessDatabase.CheckPlayerInMatchAsync(guildId, player1.Id))
                throw new ChessException($"{player1.Mention} is currently in a game.");
            if (await _chessDatabase.CheckPlayerInMatchAsync(guildId, player2.Id))
                throw new ChessException($"{player2.Mention} is currently in a game.");

            var config = await _configDatabase.GetConfigAsync(guildId);

            var challenge = await _chessDatabase.CreateChallengeAsync(guildId, channelId, player1, player2, config.TimeoutInSeconds);
            RemoveChallenge(challenge, onTimeout);
            return challenge;
        }

        public async Task<ulong> Resign(ulong guildId, ulong channelId, IUser player)
        {
            return await _chessDatabase.ResignAsync(guildId, channelId, player.Id);
        }

        public async Task<ChessMatch> AcceptChallenge(CustomCommandContext Context, IUser player)
        {
            var challenge = await _chessDatabase.GetChallengeAsync(Context.Guild.Id, Context.Channel.Id, player.Id);

            if (challenge == null)
                throw new ChessException("No challenge exists for you to accept.");

            if (await _chessDatabase.CheckPlayerInMatchAsync(Context.Guild.Id, challenge.ChallengeeId))
                throw new ChessException(string.Format("{0} is currently in a game.", Context.Guild.GetUser(challenge.ChallengeeId)));

            SocketUser Challengee = Context.Client.GetUser(challenge.ChallengeeId);
            SocketUser Challenger = Context.Client.GetUser(challenge.ChallengerId);

            string blackURL = Challengee.GetAvatarUrl() ?? DefaulDiscordAvatar.GetURL(Challengee.DiscriminatorValue);
            string whiteURL = Challenger.GetAvatarUrl() ?? DefaulDiscordAvatar.GetURL(Challenger.DiscriminatorValue);

            return await _chessDatabase.AcceptChallengeAsync(challenge, new ChessGame(), blackURL, whiteURL);
        }

        public async Task<ChessMatchStatus> Move(Stream stream, ulong guildId, ulong channelId, IUser player, string rawMove)
        {
            var RankToRowMap = new Dictionary<int, int>()
                {
                    {1, 7},
                    {2, 6},
                    {3, 5},
                    {4, 4},
                    {5, 3},
                    {6, 2},
                    {7, 1},
                    {8, 0}
                };
            string moveInput = rawMove.Replace(" ", "").ToUpper();
            if (!Regex.IsMatch(moveInput, "^[A-H][1-8][A-H][1-8][Q|N|B|R]?$"))
                throw new ChessException("Error parsing move. Example move: a2a4");
            var match = await _chessDatabase.GetMatchAsync(guildId, channelId, player.Id);
            if (match == null)
                throw new ChessException("You are not currently in a game");
            Player whoseTurn = match.ChessGame.WhoseTurn;
            Player otherPlayer = whoseTurn == Player.White ? Player.Black : Player.White;
            if (whoseTurn == Player.White && player.Id != match.ChallengerId || whoseTurn == Player.Black && player.Id != match.ChallengeeId)
                throw new ChessException("It's not your turn.");

            string sourceX = moveInput[0].ToString();
            string sourceY = moveInput[1].ToString();
            string destX = moveInput[2].ToString();
            string destY = moveInput[3].ToString();

            var positionEnumValues = (IEnumerable<ChessDotNet.File>)Enum.GetValues(typeof(ChessDotNet.File));
            var sourcePositionX = positionEnumValues.Single(x => x.ToString("g") == sourceX);
            var destPositionX = positionEnumValues.Single(x => x.ToString("g") == destX);

            var originalPosition = new Position(sourcePositionX, int.Parse(sourceY));
            var newPosition = new Position(destPositionX, int.Parse(destY));

            Piece[][] board = match.ChessGame.GetBoard();
            var file = int.Parse(sourcePositionX.ToString("d"));
            var collumn = RankToRowMap[int.Parse(sourceY)];
            var pieceChar = board[file][collumn].GetFenCharacter();
            var isPawn = pieceChar.ToString().ToLower() == "p";
            char? promotion;
            if (destY != "1" && destY != "8" || !isPawn)
                promotion = null;
            else
                promotion = moveInput[4].ToString().ToLower()[0];
            Move move = new Move(originalPosition, newPosition, whoseTurn, promotion);
            if (!match.ChessGame.IsValidMove(move))
                throw new ChessException("Invalid move.");
            ChessMove chessMove = new ChessMove()
            {
                Move = move,
                MoveDate = DateTime.Now
            };
            int num3 = (int)match.ChessGame.ApplyMove(move, true);
            match.HistoryList.Add(chessMove);
            bool checkMated = match.ChessGame.IsCheckmated(otherPlayer);
            bool isOver = checkMated || match.ChessGame.IsStalemated(otherPlayer);
            var ImageLinkValues = await GetImageLinkFromMatchAsync(match, stream);
            ChessMatchStatus status = new ChessMatchStatus()
            {
                ImageId = ImageLinkValues.ImageId,
                IsOver = isOver,
                WinnerId = isOver & checkMated ? (ulong?)player.Id : null,
                IsCheck = match.ChessGame.IsInCheck(otherPlayer),
                IsCheckmated = match.ChessGame.IsCheckmated(otherPlayer),
                Match = match
            };
            await _chessDatabase.UpdateChessGameAsync(match, status);
            return await Task.FromResult(status);
        }

        private async void RemoveChallenge(ChessChallenge challenge, Action<ChessChallenge> onTimeout)
        {
            while (challenge.TimeoutDate > DateTime.Now)
                await Task.Delay(1000);

            if (challenge.Accepted)
                return;
            onTimeout?.Invoke(challenge);
        }
    }
}
