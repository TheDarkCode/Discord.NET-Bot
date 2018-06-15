using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ArcadesBot
{
    [Name("Chess")]
    [Summary("All the commands related to chess")]
    public class ChessModule : Base
    {
        private readonly ChessHelper _chessHelper;
        private readonly ChessService _chessService;

        public ChessModule(ChessService chessService, ChessHelper chessHelper)
        {
            _chessHelper = chessHelper;
            _chessService = chessService;
        }

        [RequireContext(ContextType.Guild)]
        [Command("show")]
        [Summary("Shows the current board")]
        public async Task ShowAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            using (var stream = new MemoryStream())
            {
                try
                {
                    var chessMatchStatus = await _chessService.WriteBoard(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id, stream);
                    string str;
                    if (chessMatchStatus.IsOver)
                    {
                        str = "This match is over.";
                        var winnerId = chessMatchStatus.WinnerId;
                        if (winnerId != null)
                        {
                            var user = Context.Guild.GetUser((ulong)winnerId);
                            str += $" {user.Mention} has won the match.";
                        }
                    }
                    else
                        str = Context.Guild.GetUser(chessMatchStatus.NextPlayerId).Mention + " is up next";
                    var embedBuilder = new EmbedBuilder().WithImageUrl(chessMatchStatus.ImageId).WithColor(EmbedColors.GetSuccessColor()).WithDescription(str);
                    await Context.Channel.SendFileAsync($"Chessboards/board{chessMatchStatus.Match.Id}-{chessMatchStatus.Match.HistoryList.Count}.png", embed: embedBuilder.Build());
                }
                catch (ChessException ex)
                {
                    var embedBuilder = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
                    await ReplyAsync("", false, embedBuilder.Build());
                }
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("accept")]
        [Summary("Accepts challenge if you have one")]
        public async Task AcceptAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            var challengeAsync = _chessHelper.GetChallenge(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id);
            var embed = new EmbedBuilder();
            try
            {
                if (challengeAsync != null)
                {
                    var chessMatch = _chessService.AcceptChallenge(Context, Context.Message.Author);
                    embed = new EmbedBuilder().WithDescription($"Match has started between {Context.Guild.GetUser(chessMatch.ChallengerId).Mention} and {Context.Guild.GetUser(chessMatch.ChallengeeId).Mention}.").WithColor(EmbedColors.GetSuccessColor());
                    using (var stream = new MemoryStream())
                    {
                        var chessMatchStatus = await _chessService.WriteBoard(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id, stream);
                        embed.WithImageUrl(chessMatchStatus.ImageId).WithDescription("Your move " + Context.Guild.GetUser(chessMatchStatus.NextPlayerId).Mention);
                        await Context.Channel.SendFileAsync($"Chessboards/board{chessMatchStatus.Match.Id}-{chessMatchStatus.Match.HistoryList.Count}.png", embed: embed.Build());
                    }
                }
            }
            catch (ChessException ex)
            {
                embed.WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
                await ReplyAsync("", false, embed.Build());
            }

        }
        [RequireContext(ContextType.Guild)]
        [Command("challenge")]
        [Summary("Challenge someone to a chess match")]
        public async Task ChallengeAsync([Summary("The person you want to challenge to a match of chess")]IUser challengee)
        {
            await Context.Channel.TriggerTypingAsync();
            var user = challengee as SocketGuildUser;
            EmbedBuilder embed;
            try
            {
                var _ = _chessService.Challenge(Context.Guild.Id, Context.Channel.Id, Context.Message.Author, user, async x =>
                {
                    var challengeeUser = Context.Guild.GetUser(x.ChallengeeId);
                    var challengerUser = Context.Guild.GetUser(x.ChallengerId);

                    embed = new EmbedBuilder().WithDescription($"Challenge timed out for {challengerUser.Mention} challenging {challengeeUser.Mention}").WithColor(EmbedColors.GetSuccessColor());
                    await ReplyAsync("", false, embed.Build());
                });
                embed = new EmbedBuilder().WithDescription(Context.Message.Author.Mention + $" is challenging {user.Mention}.").WithColor(EmbedColors.GetSuccessColor());
                await ReplyAsync("", false, embed.Build());
            }
            catch (ChessException ex)
            {
                embed = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
                await ReplyAsync("", false, embed.Build());
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("resign")]
        [Summary("Resign from your current chess game")]
        public async Task ResignAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            try
            {
                var winner = _chessService.Resign(Context.Guild.Id, Context.Channel.Id, Context.Message.Author);
                var user = Context.Guild.GetUser(winner);
                var embedBuilder = new EmbedBuilder().WithDescription($"{Context.Message.Author.Mention} has resigned the match. {user.Mention} has won the game.").WithColor(EmbedColors.GetSuccessColor());
                await ReplyAsync("", false, embedBuilder.Build());
            }
            catch (ChessException ex)
            {
                var embedBuilder = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetSuccessColor());
                await ReplyAsync("", false, embedBuilder.Build());
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("stats")]
        [Summary("Currently a filler command")]
        public async Task StatsAsync([Summary("Availible Scopes:\n - User (Default)\n - Guild\n - Global")]Scope scope = Scope.User)
        {
            IEnumerable<ChessMatchStatsModel> stats;
            ChessMatchStatsModel lastGame = null;
            var embed = new EmbedBuilder();
            switch (scope)
            {
                case Scope.User:
                    embed.WithAuthor(Context.User).WithDescription($"Stats for {Context.User}");
                    stats =  _chessHelper.GetStatsFromUser(Context.User.Id, Context.Guild.Id);
                    lastGame = stats.OrderByDescending(x => x.EndDate).FirstOrDefault();
                    if (lastGame == null)
                        embed.AddField("No games found on this server", "");
                    else
                    {
                        embed.AddField(
                            "Games completed",
                            stats.Count(),
                            true
                        ).AddField(
                            "Amount of games won",
                            stats.Count(x => x.Winner == Context.User.Id),
                            true
                        ).AddField(
                            "Last game",
                            lastGame.Winner == Context.User.Id
                                ? $"Won by {lastGame.EndBy}"
                                : $"Lost by {lastGame.EndBy}",
                            true
                        ).WithUrl($"attachment://board{lastGame.Id}-{lastGame.MoveCount}.png");
                    }

                    break;
                case Scope.Guild:
                    embed.WithAuthor(Context.Guild.IconUrl).WithDescription($"Stats for {Context.Guild}");
                    stats = _chessHelper.GetStatsByGuild(Context.Guild.Id);
                    lastGame = stats.OrderByDescending(x => x.EndDate).FirstOrDefault();
                    if (lastGame == null)
                        embed.AddField("No games found on this server", "");
                    else
                    {
                        embed.AddField(
                            "Games completed",
                            stats.Count(),
                            true
                        ).AddField(
                            "Amount of games won",
                            stats.Count(x => x.Winner == Context.User.Id),
                            true
                        ).AddField(
                            "Last game",
                            lastGame.Winner == Context.User.Id
                                ? $"Won by {lastGame.EndBy}"
                                : $"Lost by {lastGame.EndBy}",
                            true
                        ).WithImageUrl($"attachment://board{lastGame.Id}-{lastGame.MoveCount}.png");
                    }

                    break;
                case Scope.Global:
                    embed.WithAuthor(Context.User).WithDescription($"Stats for {Context.Guild}");
                    stats = _chessHelper.GetGlobalStats();
                    lastGame = stats.OrderByDescending(x => x.EndDate).FirstOrDefault();
                    if (lastGame == null)
                    {
                        embed.AddField("No games found globally", "");
                        break;
                    }
                    else
                    {
                        double gamesCount = stats.Count();
                        double challengerWinnerCount = stats.Count(x => x.Winner == x.CreatedBy);
                        var division = challengerWinnerCount / gamesCount;
                        embed.AddField(
                            "Games completed",
                            stats.Count(),
                            true
                        ).AddField(
                            "Won by challenger",
                            Math.Round(division * 100, 2) + "%",
                            true
                        ).AddField(
                            "Last game",
                            lastGame.Winner == Context.User.Id
                                ? $"Won by {lastGame.EndBy}"
                                : $"Lost by {lastGame.EndBy}",
                            true
                        ).WithImageUrl($"attachment://board{lastGame.Id}-{lastGame.MoveCount}.png");
                        break;
                    }
            }

            if (lastGame != null)
            {
                var match = _chessHelper.GetMatchByStatId(lastGame.Id);
                Stream stream = File.OpenRead($"Chessboards/board{match.Id}-{lastGame.MoveCount}.png");
                await SendFileAsync(stream, $"board{lastGame.Id}-{lastGame.MoveCount}.png",
                    embed: embed.Build());
            }
            else
                await ReplyAsync("", embed: embed.Build());
        }


        [RequireContext(ContextType.Guild)]
        [Command("move")]
        [Summary("Move a piece on the board")]
        public async Task MoveAsync([Summary("Moves a piece, if your pawn reaches the other side of the board it will be promoted to queen by default.\nYou can promote your pawn to other pieces if you like, r = Rook, b = Bishop, q = Queen, n = Knight. \nAn example move promoting a white pawn to a Knight would be **!a7a8n**")]string move)
        {
            await Context.Channel.TriggerTypingAsync();
            try
            {
                using (var stream = new MemoryStream())
                {
                    var result = await _chessService.Move(stream, Context.Guild.Id, Context.Channel.Id, Context.Message.Author, move);
                    if (result.IsOver)
                    {
                        var str = "The match is over.";
                        if (result.WinnerId.HasValue)
                        {
                            var user = Context.Guild.GetUser(result.WinnerId.Value);
                            str += $" {user.Mention} has won the match";
                        }
                        if (result.IsCheckmated)
                            str += " by checkmating";
                        var embedBuilder = new EmbedBuilder().WithImageUrl(result.ImageId).WithColor(EmbedColors.GetSuccessColor()).WithDescription(str);
                        await Context.Channel.SendFileAsync($"Chessboards/board{result.Match.Id}-{result.Match.HistoryList.Count}.png", embed: embedBuilder.Build());
                    }
                    else
                    {
                        var userId = _chessService.WhoseTurn(result);
                        var str = $"Your move {Context.Guild.GetUser(userId).Mention}.";
                        if (result.IsCheck)
                            str += " Check!";
                        var embedBuilder = new EmbedBuilder().WithImageUrl(result.ImageId).WithColor(EmbedColors.GetSuccessColor()).WithDescription(str);
                        await SendFileAsync($"Chessboards/board{result.Match.Id}-{result.Match.HistoryList.Count}.png", embed: embedBuilder.Build());
                    }
                }
            }
            catch (ChessException ex)
            {
                var embedBuilder = new EmbedBuilder().WithDescription(ex.Message).WithColor(EmbedColors.GetErrorColor());
                await ReplyAsync("", false, embedBuilder.Build());
            }
        }
    }
}