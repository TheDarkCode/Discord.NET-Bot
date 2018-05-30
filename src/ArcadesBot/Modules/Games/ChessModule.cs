﻿//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using System.Linq;
//using System.IO;
//using System.Threading.Tasks;

//namespace ArcadesBot
//{
//    [Name("Chess")]
//    [Summary("All the commands related to chess")]
//    public class ChessModule : ModuleBase<CustomCommandContext>
//    {
//        private readonly ChessDatabase _chessDatabase;
//        private readonly IChessService _chessService;
//        private readonly ConfigDatabase _configDatabase;

//        public ChessModule(IChessService chessService, ChessDatabase chessDatabase, ConfigDatabase configDatabase)
//        {
//            _configDatabase = configDatabase;
//            _chessDatabase = chessDatabase;
//            _chessService = chessService;
//        }

//        [RequireContext(ContextType.Guild)]
//        [Command("show")]
//        [Summary("Shows the current board")]
//        public async Task ShowAsync(ulong? Id = null)
//        {
//            await Context.Channel.TriggerTypingAsync();
//            using (MemoryStream stream = new MemoryStream())
//            {
//                try
//                {
//                    ChessMatchStatus chessMatchStatus;
//                    if (!Id.HasValue)
//                        chessMatchStatus = await _chessService.WriteBoard(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id, stream);
//                    else
//                        chessMatchStatus = await _chessService.WriteBoard(Id, stream);
//                    string str;
//                    if (chessMatchStatus.IsOver)
//                    {
//                        str = "This match is over.";
//                        ulong? winnerId = chessMatchStatus.WinnerId;
//                        if (winnerId != null)
//                        {
//                            SocketGuildUser user = Context.Guild.GetUser((ulong)winnerId);
//                            str += $" {user.Mention} has won the match.";
//                        }
//                    }
//                    else
//                        str = Context.Guild.GetUser(chessMatchStatus.NextPlayerId).Mention + " is up next";
//                    var embedBuilder = new EmbedBuilder().WithImageUrl(chessMatchStatus.ImageId).WithColor(EmbedColors.GetSuccessColor()).WithDescription(str);
//                    await ReplyAsync("", false, embedBuilder.Build());
//                }
//                catch (ChessException ex)
//                {
//                    var embedBuilder = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
//                    await ReplyAsync("", false, embedBuilder.Build());
//                }
//            }
//        }

//        [RequireContext(ContextType.Guild)]
//        [Command("accept")]
//        [Summary("Accepts challenge if you have one")]
//        public async Task AcceptAsync()
//        {
//            await Context.Channel.TriggerTypingAsync();
//            var challengeAsync = await _chessDatabase.GetChallengeAsync(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id);
//            var embed = new EmbedBuilder();
//            try
//            {
//                if (challengeAsync != null)
//                {
//                    var chessMatch = await _chessService.AcceptChallenge(Context, Context.Message.Author);
//                    embed = new EmbedBuilder().WithDescription($"Match has started between {Context.Guild.GetUser(chessMatch.ChallengerId).Mention} and {Context.Guild.GetUser(chessMatch.ChallengeeId).Mention}.").WithColor(EmbedColors.GetSuccessColor());
//                    using (MemoryStream stream = new MemoryStream())
//                    {
//                        var chessMatchStatus = await _chessService.WriteBoard(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id, stream);
//                        embed.WithImageUrl(chessMatchStatus.ImageId).WithDescription("Your move " + Context.Guild.GetUser(chessMatchStatus.NextPlayerId).Mention);
//                        await ReplyAsync("", false, embed.Build());
//                    }
//                }
//            }
//            catch (ChessException ex)
//            {
//                embed.WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
//                await ReplyAsync("", false, embed.Build());
//            }

//        }
//        [RequireContext(ContextType.Guild)]
//        [Command("challenge")]
//        [Summary("Challenge someone to a chess match")]
//        public async Task ChallengeAsync(IUser challengee)
//        {
//            await Context.Channel.TriggerTypingAsync();
//            SocketGuildUser user = challengee as SocketGuildUser;
//            EmbedBuilder embed;
//            try
//            {
//                ChessChallenge chessChallenge = await _chessService.Challenge(Context.Guild.Id, Context.Channel.Id, Context.Message.Author, user, (async x =>
//                {
//                    SocketGuildUser Challengee = Context.Guild.GetUser(x.ChallengeeId);
//                    SocketGuildUser Challenger = Context.Guild.GetUser(x.ChallengerId);

//                    embed = new EmbedBuilder().WithDescription($"Challenge timed out for {Challenger.Mention} challenging {Challengee.Mention}").WithColor(EmbedColors.GetSuccessColor());
//                    await ReplyAsync("", false, embed.Build());
//                }));
//                embed = new EmbedBuilder().WithDescription(Context.Message.Author.Mention + $" is challenging {user.Mention}.").WithColor(EmbedColors.GetSuccessColor());
//                await ReplyAsync("", false, embed.Build());
//            }
//            catch (ChessException ex)
//            {
//                embed = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
//                await ReplyAsync("", false, embed.Build());
//            }
//        }

//        [RequireContext(ContextType.Guild)]
//        [Command("resign")]
//        [Summary("Resign from your current chess game")]
//        public async Task ResignAsync()
//        {
//            await Context.Channel.TriggerTypingAsync();
//            try
//            {
//                ulong winner = await _chessService.Resign(Context.Guild.Id, Context.Channel.Id, Context.Message.Author);
//                SocketGuildUser user = Context.Guild.GetUser(winner);
//                EmbedBuilder embedBuilder = new EmbedBuilder().WithDescription($"{Context.Message.Author.Mention} has resigned the match. {user.Mention} has won the game.").WithColor(EmbedColors.GetSuccessColor());
//                await ReplyAsync("", false, embedBuilder.Build());
//            }
//            catch (ChessException ex)
//            {
//                var embedBuilder = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetSuccessColor());
//                await ReplyAsync("", false, embedBuilder.Build());
//            }
//        }

//        [RequireContext(ContextType.Guild)]
//        [Command("stats")]
//        [Summary("Currently a filler command")]
//        public async Task StatsAsync()
//        {
//        }

//        [IsAdmin]
//        [RequireContext(ContextType.Guild)]
//        [Command("changetimeout")]
//        [Summary("Change the timeout on this guild in seconds")]
//        public async Task ChangeTimoutAsync(ulong seconds = 30)
//        {
//            await _configDatabase.ChangeTimeout(seconds, Context.Guild.Id);
//            var embedBuilder = new EmbedBuilder().WithDescription($"The timeout of this guild has been changed to `{seconds}` seconds").WithColor(EmbedColors.GetSuccessColor());
//            await ReplyAsync("", false, embedBuilder.Build());
//        }

//        [RequireContext(ContextType.Guild)]
//        [Command("move")]
//        [Summary("Moves a piece, if your pawn reaches the other side of the board it will be promoted to queen by default.\nYou can promote your pawn to other pieces if you like, r = Rook, b = Bishop, q = Queen, n = Knight. \nAn example move promoting a white pawn to a Knight would be **!a7a8n**")]
//        public async Task MoveAsync(string move)
//        {
//            await Context.Channel.TriggerTypingAsync();
//            try
//            {
//                using (MemoryStream stream = new MemoryStream())
//                {
//                    var result = await _chessService.Move(stream, Context.Guild.Id, Context.Channel.Id, Context.Message.Author, move);
//                    if (result.IsOver)
//                    {
//                        string str = "The match is over.";
//                        if (result.WinnerId.HasValue)
//                        {
//                            SocketGuildUser user = Context.Guild.GetUser(result.WinnerId.Value);
//                            str += $" {user.Mention} has won the match";
//                        }
//                        if (result.IsCheckmated)
//                            str += " by checkmating";
//                        var embedBuilder = new EmbedBuilder().WithImageUrl(result.ImageId).WithColor(EmbedColors.GetSuccessColor()).WithDescription(str);
//                        await ReplyAsync("", false, embedBuilder.Build(), (RequestOptions)null);
//                    }
//                    else
//                    {
//                        ulong num2 = _chessService.WhoseTurn(result.Match);
//                        string str = $"Your move {Context.Guild.GetUser(num2).Mention}.";
//                        if (result.IsCheck)
//                            str += " Check!";
//                        EmbedBuilder embedBuilder = new EmbedBuilder().WithImageUrl(result.ImageId).WithColor(EmbedColors.GetSuccessColor()).WithDescription(str);
//                        await ReplyAsync("", false, embedBuilder.Build());
//                    }
//                }
//            }
//            catch (ChessException ex)
//            {
//                EmbedBuilder embedBuilder = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
//                await ReplyAsync("", false, embedBuilder.Build());
//            }
//        }
//    }
//}