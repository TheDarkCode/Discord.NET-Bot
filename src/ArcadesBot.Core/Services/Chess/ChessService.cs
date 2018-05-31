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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class ChessService
    {
        private readonly AssetService _assetService;
        private readonly ChessHelper _chessHelper;

        public ChessService(AssetService assetService, ChessHelper chessHelper)
        {
            _chessHelper = chessHelper;
            _assetService = assetService;
        }

        public ulong WhoseTurn(ChessMatchStatusModel match)
        {
            if (match.Match.ChessGame == null)
            {
                if (match.Game.WhoseTurn != Player.White)
                    return match.Match.ChallengeeId;
                return match.Match.ChallengerId;
            }
            else
            {
                if (match.Match.ChessGame.WhoseTurn != Player.White)
                    return match.Match.ChallengeeId;
                return match.Match.ChallengerId;
            }
        }

        private void DrawImage(IImageProcessingContext<Rgba32> processor, string name, int x, int y)
        {
            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath($"{name}.png"));
            processor.DrawImage(image, new Size(50, 50), new Point(x * 50 + 117, y * 50 + 19), new GraphicsOptions());
        }

        public async Task<ChessMatchStatusModel> WriteBoard(Guid? Id, Stream stream)
        {
            ChessMatchModel match = null;
            match =  _chessHelper.GetMatch(Id);
            if (match.ChessGame == null && match.MoveList == null)
                throw new ChessException("This is not a Id that belongs to a match");
            ChessMatchStatusModel chessMatchStatus;
            if (match.MoveList == null)
            {
                Player otherPlayer = match.ChessGame.WhoseTurn == Player.White ? Player.Black : Player.White;
                bool checkMated = match.ChessGame.IsCheckmated(Player.White) || match.ChessGame.IsCheckmated(Player.Black);
                bool isOver = checkMated || match.ChessGame.IsStalemated(otherPlayer);
                var linkFromMatchAsync = await GetImageLinkFromMatchAsync(match, stream);
                chessMatchStatus = new ChessMatchStatusModel()
                {
                    ImageId = linkFromMatchAsync.ImageId,
                    NextPlayerId = linkFromMatchAsync.NextPlayerId,
                    IsOver = isOver,
                    IsCheck = match.ChessGame.IsInCheck(otherPlayer),
                    IsCheckmated = checkMated
                };
            }
            else
                chessMatchStatus = new ChessMatchStatusModel()
                {
                    ImageId = $"attachment://Chessboards/board{match.Id}-{match.HistoryList.Count}.png",
                    IsOver = true,
                    WinnerId = match.Winner
                };
            return chessMatchStatus;
        }

        public async Task<ChessMatchStatusModel> WriteBoard(ulong guildId, ulong channelId, ulong playerId, Stream stream)
        {
            ChessMatchModel match = null;
            match = _chessHelper.GetMatch(guildId, channelId, playerId);
            if (match == null)
                throw new ChessException("You are not in a game.");
            var moves = match.MoveList;
            ChessGame game;
            if (moves.Count != 0)
                game = new ChessGame(moves, true);
            else
                game = new ChessGame();
            Player otherPlayer = game.WhoseTurn == Player.White ? Player.Black : Player.White;
            bool isOver = game.IsCheckmated(otherPlayer) || game.IsStalemated(otherPlayer);
            ChessMatchStatusModel linkFromMatchAsync = await GetImageLinkFromMatchAsync(match, stream);
            return new ChessMatchStatusModel()
            {
                Match = match,
                ImageId = linkFromMatchAsync.ImageId,
                NextPlayerId = linkFromMatchAsync.NextPlayerId,
                IsOver = isOver,
                IsCheck = game.IsInCheck(otherPlayer),
                IsCheckmated = game.IsCheckmated(otherPlayer)
            };
        }


        private async Task<ChessMatchStatusModel> GetImageLinkFromMatchAsync(ChessMatchModel match, Stream stream)
        {
            ChessMoveModel lastMove;
            ChessGame game = new ChessGame();
            await Task.Run(async () =>
            {
                var board = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("board.png"));

                HttpClient httpClient = new HttpClient();
                var turnIndicator = SixLabors.ImageSharp.Image.Load(_assetService.GetImagePath("turn_indicator.png"));
                var whiteAvatarData = SixLabors.ImageSharp.Image.Load(await httpClient.GetByteArrayAsync(match.WhiteAvatarURL));
                var blackAvatarData = SixLabors.ImageSharp.Image.Load(await httpClient.GetByteArrayAsync(match.BlackAvatarURL));
                httpClient.Dispose();

                var moves = match.MoveList;
                
                if (moves.Count != 0)
                    game = new ChessGame(moves, true);
                else
                    game = new ChessGame();
                
                Piece[][] boardPieces = game.GetBoard();

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
                var turnIndicatorPoint = game.WhoseTurn != Player.Black ? new Point(538, 367) : new Point(40, 15);
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
                                if (piece.GetFenCharacter().ToString().ToUpper() == "K" && game.IsInCheck(piece.Owner))
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
                    var BlackPawnCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'p' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhitePawnCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'P' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackRookCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'r' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteRookCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'R' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackKnightCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'n' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteKnightCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'N' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackBishopCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'b' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteBishopCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'B' && x.Owner == Player.White && !x.IsPromotionResult);
                    var BlackQueenCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'q' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var WhiteQueenCount = game.PiecesOnBoard.Count(x => x.GetFenCharacter() == 'Q' && x.Owner == Player.White && !x.IsPromotionResult);
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
                board.Save($"{Directory.GetCurrentDirectory()}\\Chessboards\\board{match.Id}-{match.HistoryList.Count}.png");
            });
            return new ChessMatchStatusModel()
            {
                ImageId = $"attachment://board{match.Id}-{match.HistoryList.Count}.png",
                NextPlayerId = WhoseTurn(new ChessMatchStatusModel
                {
                   Match = new ChessMatchModel{
                        ChallengeeId = match.ChallengeeId,
                        ChallengerId = match.ChallengerId,
                        ChessGame = game
                   }
                })
            };
        }

        public ChessChallengeModel Challenge(ulong guildId, ulong channelId, IUser player1, IUser player2, Action<ChessChallengeModel> onTimeout = null)
        {
            if (player1.Equals(player2))
                throw new ChessException("You can't challenge yourself.");
            if (_chessHelper.CheckPlayerInMatch(guildId, player1.Id))
                throw new ChessException($"{player1.Mention} is currently in a game.");
            if (_chessHelper.CheckPlayerInMatch(guildId, player2.Id))
                throw new ChessException($"{player2.Mention} is currently in a game.");

            var challenge = _chessHelper.CreateChallenge(guildId, channelId, player1, player2);
            RemoveChallenge(challenge, onTimeout);
            return challenge;
        }

        public ulong Resign(ulong guildId, ulong channelId, IUser player)
            => _chessHelper.Resign(guildId, channelId, player.Id);

        public ChessMatchModel AcceptChallenge(CustomCommandContext Context, IUser player)
        {
            var challenge = _chessHelper.GetChallenge(Context.Guild.Id, Context.Channel.Id, player.Id);

            if (challenge == null)
                throw new ChessException("No challenge exists for you to accept.");

            if (_chessHelper.CheckPlayerInMatch(Context.Guild.Id, challenge.ChallengeeId))
                throw new ChessException(string.Format("{0} is currently in a game.", Context.Guild.GetUser(challenge.ChallengeeId)));

            SocketUser Challengee = Context.Client.GetUser(challenge.ChallengeeId);
            SocketUser Challenger = Context.Client.GetUser(challenge.ChallengerId);

            string blackURL = Challengee.GetAvatarUrl() ?? Challengee.GetDefaultAvatarUrl();
            string whiteURL = Challenger.GetAvatarUrl() ?? Challenger.GetDefaultAvatarUrl();

            return _chessHelper.AcceptChallenge(challenge, blackURL, whiteURL);
        }

        public async Task<ChessMatchStatusModel> Move(Stream stream, ulong guildId, ulong channelId, IUser player, string rawMove)
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
            var match = _chessHelper.GetMatch(guildId, channelId, player.Id);
            if (match == null)
                throw new ChessException("You are not currently in a game");

            var moves = match.MoveList;
            ChessGame game;
            if (moves.Count != 0)
                game = new ChessGame(moves, true);
            else
                game = new ChessGame();
            Player whoseTurn = game.WhoseTurn;
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

            Piece[][] board = game.GetBoard();
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
            if (!game.IsValidMove(move))
                throw new ChessException("Invalid move.");
            var chessMove = new ChessMoveModel()
            {
                Move = move,
                MoveDate = DateTime.Now
            };
            game.ApplyMove(move, true);
            match.MoveList.Add(move);
            match.HistoryList.Add(chessMove);
            bool checkMated = game.IsCheckmated(otherPlayer);
            bool isOver = checkMated || game.IsStalemated(otherPlayer);
            var ImageLinkValues = await GetImageLinkFromMatchAsync(match, stream);
            var status = new ChessMatchStatusModel()
            {
                Game = game,
                ImageId = ImageLinkValues.ImageId,
                IsOver = isOver,
                WinnerId = isOver & checkMated ? (ulong?)player.Id : null,
                IsCheck = game.IsInCheck(otherPlayer),
                IsCheckmated = game.IsCheckmated(otherPlayer),
                Match = match
            };
            _chessHelper.UpdateChessGame(status);
            return await Task.FromResult(status);
        }

        private async void RemoveChallenge(ChessChallengeModel challenge, Action<ChessChallengeModel> onTimeout)
        {
            while (challenge.TimeoutDate > DateTime.Now)
                await Task.Delay(1000);
            challenge = _chessHelper.GetChallenge((Guid?)new Guid(challenge.Id));
            if (challenge.Accepted)
                return;
            onTimeout?.Invoke(challenge);
        }
    }
}
