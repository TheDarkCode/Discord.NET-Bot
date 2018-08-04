using Discord;
using Discord.Commands;
using System.IO;
using System.Threading.Tasks;

namespace ArcadesBot
{
    [Name("Chess")]
    [Summary("All the commands related to chess")]
    public class ChessModule : Base
    {
        private ChessHelper _chessHelper { get; }
        private ChessService _chessService { get; }

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
            try
            {
                var chessMatchStatus = await _chessService.WriteBoard(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id);
                string str;
                if (chessMatchStatus.Status != Cause.OnGoing)
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
                var embedBuilder = new EmbedBuilder()
                    .WithSuccessColor()
                    .WithImageUrl(chessMatchStatus.ImageLink)
                    .WithDescription(str);
                await SendFileAsync($"Chessboards/board{chessMatchStatus.Match.Id}-{chessMatchStatus.Match.HistoryList.Count}.png", embed: embedBuilder);
            }
            catch (ChessException ex)
            {
                var embedBuilder = new EmbedBuilder()
                    .WithErrorColor()
                    .WithDescription(ex.Message);
                await ReplyAsync("", embedBuilder);
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("accept")]
        [Summary("Accepts challenge if you have one")]
        public async Task AcceptAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            var challengeAsync = _chessHelper.GetChallenge(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id, true);
            var embed = new EmbedBuilder();
            try
            {
                if (challengeAsync != null)
                {
                    var chessMatch = _chessService.AcceptChallenge(Context, Context.Message.Author);
                    embed = new EmbedBuilder()
                        .WithSuccessColor()
                        .WithDescription($"Match has started between {Context.Guild.GetUser(chessMatch.ChallengerId).Mention} and {Context.Guild.GetUser(chessMatch.ChallengeeId).Mention}.");

                    var chessMatchStatus = await _chessService.WriteBoard(Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id);
                    embed.WithImageUrl(chessMatchStatus.ImageLink).WithDescription("Your move " + Context.Guild.GetUser(chessMatchStatus.NextPlayerId).Mention);
                    await SendFileAsync($"Chessboards/board{chessMatchStatus.Match.Id}-{chessMatchStatus.Match.HistoryList.Count}.png", embed: embed);
                }
            }
            catch (ChessException ex)
            {
                embed.WithDescription(ex.Message);
                await ReplyEmbedAsync(embed: embed);
            }

        }
        [RequireContext(ContextType.Guild)]
        [Command("challenge")]
        [Summary("Challenge someone to a chess match")]
        public async Task ChallengeAsync([Summary("The person you want to challenge to a match of chess")]IUser user)
        {
            await Context.Channel.TriggerTypingAsync();
            EmbedBuilder embed;
            try
            {
                _chessService.Challenge(Context.Guild.Id, Context.Channel.Id, Context.Message.Author, user, async x =>
                {
                    var challengee = Context.Guild.GetUser(x.ChallengeeId);
                    var challenger = Context.Guild.GetUser(x.ChallengerId);

                    embed = new EmbedBuilder()
                    .WithSuccessColor()
                    .WithDescription($"Challenge timed out for {challenger.Mention} challenging {challengee.Mention}");
                    await ReplyEmbedAsync("", embed);
                });
                embed = new EmbedBuilder()
                    .WithSuccessColor()
                    .WithDescription(Context.Message.Author.Mention + $" is challenging {user.Mention}.");
                await ReplyEmbedAsync("", embed);
            }
            catch (ChessException ex)
            {
                embed = new EmbedBuilder().WithErrorColor().WithDescription(ex.Message);
                await ReplyEmbedAsync("", embed);
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
                var embedBuilder = new EmbedBuilder().WithSuccessColor().WithDescription($"{Context.Message.Author.Mention} has resigned the match. {user.Mention} has won the game.");
                await ReplyEmbedAsync("", embedBuilder);
            }
            catch (ChessException ex)
            {
                var embedBuilder = new EmbedBuilder().WithErrorColor().WithDescription(ex.Message);
                await ReplyEmbedAsync("", embedBuilder);
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("stats")]
        [Summary("Currently a filler command")]
        public async Task StatsAsync()
        {

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
                    if (result.Status != Cause.OnGoing)
                    {
                        var str = "The match is over.";
                        if (result.WinnerId.HasValue)
                        {
                            var user = Context.Guild.GetUser(result.WinnerId.Value);
                            str += $" {user.Mention} has won the match";
                        }
                        if (result.IsCheckmated)
                            str += " by checkmating";
                        var embedBuilder = new EmbedBuilder()
                            .WithSuccessColor()
                            .WithImageUrl(result.ImageLink)
                            .WithDescription(str);
                        await SendFileAsync($"Chessboards/board{result.Match.Id}-{result.Match.HistoryList.Count}.png", embed: embedBuilder);
                    }
                    else
                    {
                        var userId = _chessService.WhoseTurn(result);
                        var str = $"Your move {Context.Guild.GetUser(userId).Mention}.";
                        if (result.IsCheck)
                            str += " Check!";
                        var embedBuilder = new EmbedBuilder()
                            .WithSuccessColor()
                            .WithImageUrl(result.ImageLink)
                            .WithDescription(str);
                        await SendFileAsync($"Chessboards/board{result.Match.Id}-{result.Match.HistoryList.Count}.png", embed: embedBuilder);
                    }
                }
            }
            catch (ChessException ex)
            {
                var embedBuilder = new EmbedBuilder()
                    .WithErrorColor()
                    .WithDescription(ex.Message);
                await ReplyEmbedAsync("", embedBuilder);
            }
        }
    }
}