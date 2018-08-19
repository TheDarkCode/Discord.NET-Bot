using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace ArcadesBot
{
    [Group("Blackjack"), Alias("b")]
    [Summary("Blackjack Commands")]
    public class BlackJackModule : InteractiveBase
    {
        private BlackJackService _blackjackService { get; }
        public BlackJackModule(BlackJackService blackjackService) 
            => _blackjackService = blackjackService;

        [Command("start"), Summary("Start a BlackJack Match")]
        public async Task StartMatchAsync()
        {
            if (!_blackjackService.StartMatch(Context.User.Id))
            {
                await ReplyEmbedAsync("Player already in Match");
                return;
            }

            var guid = _blackjackService.CreateImage(Context.User.Id);
            var embedBuilder = new EmbedBuilder()
                .WithSuccessColor()
                .WithImageUrl($"attachment://board{guid}.png")
                .WithDescription(_blackjackService.GetScoreFromMatch(Context.User.Id))
                .WithFooter("Available options: stand and hit (timeout = 30 seconds)");
            var messageToUpdate = await SendFileAsync($"BlackJack/board{guid}.png", embed: embedBuilder);
            var message = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));

        }
        [Command("clear"), Summary("Clear all Matches")]
        public async Task ClearMatches()
        {
            _blackjackService.ClearMatches();

            await ReplyEmbedAsync("Matches Cleared");
        }
    }
}