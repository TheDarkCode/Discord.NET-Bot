using System.Threading.Tasks;
using Discord.Commands;

namespace ArcadesBot
{
    [Group("Blackjack"), Alias("b")]
    [Summary("Blackjack Commands")]
    public class BlackJackModule : Base
    {
        private BlackJackService _blackjackService { get; }
        public BlackJackModule(BlackJackService blackjackService)
        {
            _blackjackService = blackjackService;
        }
        [Command("start"), Summary("Start a BlackJack Match")]
        public async Task StartMatchAsync()
        {
            if (!_blackjackService.StartMatch(Context.User.Id))
            {
                await ReplyEmbedAsync("Player already in Match");
                return;
            }
            await ReplyEmbedAsync(_blackjackService.GetScoreFromMatch(Context.User.Id));
        }
        [Command("clear"), Summary("Clear all Matches")]
        public async Task ClearMatches()
        {
            _blackjackService.ClearMatches();

            await ReplyEmbedAsync("Matches Cleared");
        }
    }
}