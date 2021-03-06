﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Helpers;
using ArcadesBot.Models.Chess;
using ArcadesBot.Utility;
using ChessDotNet;
using Discord;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using File = ChessDotNet.File;
using Image = SixLabors.ImageSharp.Image;

namespace ArcadesBot.Services.Chess
{
    public class ChessService
    {
        public ChessService(AssetService assetService, ChessHelper chessHelper)
        {
            _chessHelper = chessHelper;
            _assetService = assetService;
        }

        private AssetService _assetService { get; }
        private ChessHelper _chessHelper { get; }

        #region Public Methods

        public ulong WhoseTurn(ChessMatchStatusModel match)
        {
            if (match.Game == null)
                match.Game = match.Match.HistoryList.Count == 0
                    ? new ChessGame()
                    : new ChessGame(match.Match.HistoryList.Select(x => x.Move), true);

            return match.Game.WhoseTurn != Player.White ? match.Match.ChallengeeId : match.Match.ChallengerId;
        }

        public async Task<ChessMatchStatusModel> WriteBoard(ulong guildId, ulong channelId, ulong playerId)
        {
            var match = _chessHelper.GetMatch(guildId, channelId, playerId);
            if (match == null)
                throw new ChessException("You are not in a game.");
            var moves = match.HistoryList.Select(x => x.Move);
            var enumerable = moves.ToList();
            var game = enumerable.Count != 0
                ? new ChessGame(enumerable, true)
                : new ChessGame();
            var otherPlayer = game.WhoseTurn == Player.White ? Player.Black : Player.White;
            var linkFromMatchAsync = await DrawBoard(match);
            return new ChessMatchStatusModel
            {
                Match = match,
                ImageLink = linkFromMatchAsync.ImageLink,
                NextPlayerId = linkFromMatchAsync.NextPlayer,
                IsCheck = game.IsInCheck(otherPlayer),
                IsCheckmated = game.IsCheckmated(otherPlayer)
            };
        }


        public ChessChallengeModel Challenge(ulong guildId, ulong channelId, IUser player1, IUser player2,
            Action<ChessChallengeModel> onTimeout = null)
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

        public ChessMatchModel AcceptChallenge(CustomCommandContext context, IUser player)
        {
            var challenge = _chessHelper.GetChallenge(context.Guild.Id, context.Channel.Id, player.Id, true);

            if (challenge == null)
                throw new ChessException("No challenge exists for you to accept.");

            if (_chessHelper.CheckPlayerInMatch(context.Guild.Id, challenge.ChallengeeId))
                throw new ChessException(string.Format("{0} is currently in a game.",
                    context.Guild.GetUser(challenge.ChallengeeId)));

            var challengee = context.Client.GetUser(challenge.ChallengeeId);
            var challenger = context.Client.GetUser(challenge.ChallengerId);

            var blackUrl = challengee.GetAvatarUrl() ?? challengee.GetDefaultAvatarUrl();
            var whiteUrl = challenger.GetAvatarUrl() ?? challenger.GetDefaultAvatarUrl();

            return _chessHelper.AcceptChallenge(challenge, blackUrl, whiteUrl);
        }

        public async Task<ChessMatchStatusModel> Move(ulong guildId, ulong channelId, IUser player, string rawMove)
        {
            var rankToRowMap = new Dictionary<int, int>
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
            var moveInput = rawMove.Replace(" ", "").ToUpper();
            if (!Regex.IsMatch(moveInput, "^[A-H][1-8][A-H][1-8][Q|N|B|R]?$"))
                throw new ChessException("Error parsing move. Example move: a2a4");
            var match = _chessHelper.GetMatch(guildId, channelId, player.Id);
            if (match == null)
                throw new ChessException("You are not currently in a game");

            var moves = match.HistoryList.Select(x => x.Move).ToList();
            var game = moves.Count != 0
                ? new ChessGame(moves, true)
                : new ChessGame();
            var whoseTurn = game.WhoseTurn;
            var otherPlayer = whoseTurn == Player.White ? Player.Black : Player.White;
            if (whoseTurn == Player.White && player.Id != match.ChallengerId ||
                whoseTurn == Player.Black && player.Id != match.ChallengeeId)
                throw new ChessException("It's not your turn.");

            var sourceX = moveInput[0].ToString();
            var sourceY = moveInput[1].ToString();
            var destX = moveInput[2].ToString();
            var destY = moveInput[3].ToString();

            var positionEnumValues = (IEnumerable<File>) Enum.GetValues(typeof(File));

            var enumValues = positionEnumValues.ToList();
            var sourcePositionX = enumValues.Single(x => x.ToString("g") == sourceX);
            var destPositionX = enumValues.Single(x => x.ToString("g") == destX);

            var originalPosition = new Position(sourcePositionX, int.Parse(sourceY));
            var newPosition = new Position(destPositionX, int.Parse(destY));

            var board = game.GetBoard();
            var file = int.Parse(sourcePositionX.ToString("d"));
            var collumn = rankToRowMap[int.Parse(sourceY)];
            if (board[collumn][file] == null)
                throw new ChessException("Invalid move.");
            var pieceChar = board[collumn][file].GetFenCharacter();
            var isPawn = pieceChar.ToString().ToLower() == "p";
            char? promotion;
            if (destY != "1" && destY != "8" || !isPawn)
                promotion = null;
            else
                promotion = moveInput[4].ToString().ToLower()[0];
            var move = new Move(originalPosition, newPosition, whoseTurn, promotion);

            if (!game.IsValidMove(move))
                throw new ChessException("Invalid move.");
            var chessMove = new ChessMoveModel
            {
                Move = move,
                MovedfenChar = pieceChar,
                MoveDate = DateTime.Now,
                Player = whoseTurn
            };
            game.ApplyMove(move, true);
            match.HistoryList.Add(chessMove);

            var endCause = Cause.OnGoing;
            if (game.IsStalemated(otherPlayer))
                endCause = Cause.Stalemate;
            else if (game.IsCheckmated(otherPlayer))
                endCause = Cause.Checkmate;

            var imageLinkValues = await DrawBoard(match);
            var status = new ChessMatchStatusModel
            {
                Game = game,
                ImageLink = imageLinkValues.ImageLink,
                Status = endCause,
                WinnerId = endCause == Cause.Checkmate ? (ulong?) player.Id : null,
                IsCheck = game.IsInCheck(otherPlayer),
                IsCheckmated = game.IsCheckmated(otherPlayer),
                Match = match
            };
            _chessHelper.UpdateChessGame(status);
            return await Task.FromResult(status);
        }

        #endregion

        #region Private Methods

        private void DrawImage(IImageProcessingContext<Rgba32> processor, string name, int x, int y)
        {
            var image = Image.Load(_assetService.GetImagePath("Chess", $"{name}.png"));
            processor.DrawImage(image, new Size(50, 50), new Point(x * 50 + 117, y * 50 + 19), new GraphicsOptions());
        }

        private async Task<ImageLinkModel> DrawBoard(ChessMatchModel match)
        {
            await Task.Run(async () =>
            {
                var board = Image.Load(_assetService.GetImagePath("Chess", "board.png"));

                var httpClient = new HttpClient();
                var turnIndicator = Image.Load(_assetService.GetImagePath("Chess", "turn_indicator.png"));
                var whiteAvatarData = Image.Load(await httpClient.GetByteArrayAsync(match.WhiteAvatarUrl));
                var blackAvatarData = Image.Load(await httpClient.GetByteArrayAsync(match.BlackAvatarUrl));
                httpClient.Dispose();

                var moves = match.HistoryList.Select(x => x.Move).ToList();

                var game = moves.Count != 0 ? new ChessGame(moves, true) : new ChessGame();

                var boardPieces = game.GetBoard();

                var lastMove = match.HistoryList.OrderByDescending(x => x.MoveDate).FirstOrDefault();
                var rankToRowMap = new Dictionary<int, int>
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

                    for (var x = 0; x < boardPieces.Length; ++x)
                    for (var y = 0; y < boardPieces[x].Length; ++y)
                    {
                        if (lastMove != null && (lastMove.Move.OriginalPosition.File == (File) x &&
                                                 rankToRowMap[lastMove.Move.OriginalPosition.Rank] == y
                                                 || lastMove.Move.NewPosition.File == (File) x &&
                                                 rankToRowMap[lastMove.Move.NewPosition.Rank] == y))
                            DrawImage(processor, "yellow_square", x, y);
                        var piece = boardPieces[y][x];
                        if (piece == null)
                                continue;
                        if (piece.GetFenCharacter().ToString().ToUpper() == "K" && game.IsInCheck(piece.Owner))
                            DrawImage(processor, "red_square", x, y);
                        var str = "white";
                        if (new[] {'r', 'n', 'b', 'q', 'k', 'p'}.Contains(piece.GetFenCharacter()))
                            str = "black";
                        DrawImage(processor, string.Format("{0}_{1}", str, piece.GetFenCharacter()), x, y);
                    }

                    var blackPawnCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'p' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var whitePawnCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'P' && x.Owner == Player.White && !x.IsPromotionResult);
                    var blackRookCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'r' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var whiteRookCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'R' && x.Owner == Player.White && !x.IsPromotionResult);
                    var blackKnightCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'n' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var whiteKnightCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'N' && x.Owner == Player.White && !x.IsPromotionResult);
                    var blackBishopCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'b' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var whiteBishopCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'B' && x.Owner == Player.White && !x.IsPromotionResult);
                    var blackQueenCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'q' && x.Owner == Player.Black && !x.IsPromotionResult);
                    var whiteQueenCount = game.PiecesOnBoard.Count(x =>
                        x.GetFenCharacter() == 'Q' && x.Owner == Player.White && !x.IsPromotionResult);
                    var row = 1;

                    var blackPawn = Image.Load(_assetService.GetImagePath("Chess", "black_p.png"));

                    for (var index = 8; index > blackPawnCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(blackPawn, new Size(30, 30), new Point(533, 16 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(blackPawn, new Size(30, 30), new Point(566, 16 + row * 30),
                                new GraphicsOptions());
                            ++row;
                        }
                    }

                    row = 8;

                    var image2 = Image.Load(_assetService.GetImagePath("Chess", "white_P.png"));
                    for (var index = 8; index > whitePawnCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(image2, new Size(30, 30), new Point(20, 125 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(image2, new Size(30, 30), new Point(53, 125 + row * 30),
                                new GraphicsOptions());
                            --row;
                        }
                    }

                    row = 5;

                    var image3 = Image.Load(_assetService.GetImagePath("Chess", "black_r.png"));
                    for (var index = 2; index > blackRookCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(image3, new Size(30, 30), new Point(533, 16 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(image3, new Size(30, 30), new Point(566, 16 + row * 30),
                                new GraphicsOptions());
                            ++row;
                        }
                    }

                    row = 4;

                    var whiteRook = Image.Load(_assetService.GetImagePath("Chess", "white_R.png"));
                    for (var index = 2; index > whiteRookCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(whiteRook, new Size(30, 30), new Point(20, 125 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(whiteRook, new Size(30, 30), new Point(53, 125 + row * 30),
                                new GraphicsOptions());
                            --row;
                        }
                    }

                    row = 6;

                    var blackKnight = Image.Load(_assetService.GetImagePath("Chess", "black_n.png"));
                    for (var index = 2; index > blackKnightCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(blackKnight, new Size(30, 30), new Point(533, 16 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(blackKnight, new Size(30, 30), new Point(566, 16 + row * 30),
                                new GraphicsOptions());
                            ++row;
                        }
                    }

                    row = 3;

                    var whiteKnight = Image.Load(_assetService.GetImagePath("Chess", "white_N.png"));
                    for (var i = 2; i > whiteKnightCount; --i)
                    {
                        if (i % 2 == 0)
                        {
                            processor.DrawImage(whiteKnight, new Size(30, 30), new Point(20, 125 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(whiteKnight, new Size(30, 30), new Point(53, 125 + row * 30),
                                new GraphicsOptions());
                            --row;
                        }
                    }

                    row = 7;

                    var blackBishop = Image.Load(_assetService.GetImagePath("Chess", "black_b.png"));
                    for (var index = 2; index > blackBishopCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(blackBishop, new Size(30, 30), new Point(533, 16 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(blackBishop, new Size(30, 30), new Point(566, 16 + row * 30),
                                new GraphicsOptions());
                            ++row;
                        }
                    }

                    row = 2;

                    var whiteBishop = Image.Load(_assetService.GetImagePath("Chess", "white_B.png"));
                    for (var index = 2; index > whiteBishopCount; --index)
                    {
                        if (index % 2 == 0)
                        {
                            processor.DrawImage(whiteBishop, new Size(30, 30), new Point(20, 125 + row * 30),
                                new GraphicsOptions());
                        }
                        else
                        {
                            processor.DrawImage(whiteBishop, new Size(30, 30), new Point(53, 125 + row * 30),
                                new GraphicsOptions());
                            --row;
                        }
                    }

                    row = 8;

                    var blackQueen = Image.Load(_assetService.GetImagePath("Chess", "black_q.png"));
                    if (blackQueenCount == 0)
                        processor.DrawImage(blackQueen, new Size(30, 30), new Point(533, 16 + row * 30),
                            new GraphicsOptions());
                    row = 1;

                    var whiteQueen = Image.Load(_assetService.GetImagePath("Chess", "white_Q.png"));
                    if (whiteQueenCount == 0)
                        processor.DrawImage(whiteQueen, new Size(30, 30), new Point(20, 125 + row * 30),
                            new GraphicsOptions());

                    processor.DrawImage(turnIndicator, new Size(56, 56), turnIndicatorPoint, new GraphicsOptions());
                    processor.DrawImage(whiteAvatarData, new Size(50, 50), new Point(541, 370), new GraphicsOptions());
                    processor.DrawImage(blackAvatarData, new Size(50, 50), new Point(43, 18), new GraphicsOptions());

                    var table = Image.Load(_assetService.GetImagePath("Chess", "table.png"));
                    processor.DrawImage(table, new Size(108, 148), new Point(596, 18), new GraphicsOptions());

                    if (!SystemFonts.TryFind("Arial", out var font))
                    {
                        PrettyConsole.Log(LogSeverity.Critical, "Image drawing", "Cannot find font 'Arial' install Arial as a font and try again");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }

                    processor.DrawText("Last 5 moves", font.CreateFont(12, FontStyle.Bold), Rgba32.Black, new PointF(616, 19));
                    processor.DrawText("White", font.CreateFont(12, FontStyle.Bold), Rgba32.Black, new PointF(606, 42));
                    processor.DrawText("Black", font.CreateFont(12, FontStyle.Bold), Rgba32.Black, new PointF(660, 42));
                    var blackMoves = match.HistoryList.OrderByDescending(x => x.MoveDate).Where(x => x.Player == Player.Black).ToList();
                    var whiteMoves = match.HistoryList.OrderByDescending(x => x.MoveDate).Where(x => x.Player == Player.White).ToList();

                    for (var i = 0; i < blackMoves.Count && i < 5; i++)
                    {
                        var safeguard = i;
                        var player = blackMoves[i].Player.ToString().ToLower();
                        var file = blackMoves[i].Move.NewPosition.File;
                        var rank = blackMoves[i].Move.NewPosition.Rank;
                        var fenChar = blackMoves[i].MovedfenChar;
                        var image = Image.Load(_assetService.GetImagePath("Chess", $"{player}_{fenChar}.png"));
                        if (blackMoves.Count != whiteMoves.Count)
                            safeguard++;
                        if (whiteMoves.Count > 5 && safeguard == 5)
                            continue;
                        processor.DrawImage(image, new Size(12, 12), new Point(660, 65 + safeguard * 21), new GraphicsOptions());
                        
                        processor.DrawText($"{file.ToString() + rank}", font.CreateFont(12, FontStyle.Bold), Rgba32.Black, new PointF(672, 63 + safeguard * 21));
                    }
                    for (var i = 0; i < whiteMoves.Count && i < 5; i++)
                    {
                        var player = whiteMoves[i].Player.ToString().ToLower();
                        var file = whiteMoves[i].Move.NewPosition.File;
                        var rank = whiteMoves[i].Move.NewPosition.Rank;
                        var fenChar = whiteMoves[i].MovedfenChar;
                        var image = Image.Load(_assetService.GetImagePath("Chess", $"{player}_{fenChar}.png"));
                        processor.DrawImage(image, new Size(12, 12), new Point(606, 65 + i * 21), new GraphicsOptions());

                        processor.DrawText($"{file.ToString() + rank}", font.CreateFont(12, FontStyle.Bold), Rgba32.Black, new PointF(618, 63 + i * 21));

                    }

                    #endregion
                });
                board.Save($"{Directory.GetCurrentDirectory()}\\Chessboards\\board{match.Id}-{match.HistoryList.Count}.png");
            });

            var nextPlayer = WhoseTurn(new ChessMatchStatusModel
            {
                Match = new ChessMatchModel
                {
                    HistoryList = match.HistoryList,
                    ChallengeeId = match.ChallengeeId,
                    ChallengerId = match.ChallengerId
                }
            });
            return new ImageLinkModel
            {
                ImageLink = $"attachment://board{match.Id}-{match.HistoryList.Count}.png",
                NextPlayer = nextPlayer
            };
        }

        private async void RemoveChallenge(ChessChallengeModel challenge, Action<ChessChallengeModel> onTimeout)
        {
            while (challenge.TimeoutDate > DateTime.Now)
                await Task.Delay(250);
            challenge = _chessHelper.GetChallenge(challenge.Id);
            if (challenge.Accepted)
                return;

            onTimeout?.Invoke(challenge);
        }
        #endregion
    }
}