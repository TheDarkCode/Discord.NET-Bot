using System.Threading.Tasks;
using ArcadesBot.CommandExtensions.Attribute;
using ArcadesBot.Interactive;
using ArcadesBot.Services.BlackJack;
using Discord.Commands;

namespace ArcadesBot.Modules.Games
{
    [Group("Blackjack"), Alias("b")]
    [Summary("Blackjack Commands")]
    public class BlackJackModule : InteractiveBase
    {
        private BlackJackService _blackjackService { get; }
        public BlackJackModule(BlackJackService blackjackService) 
            => _blackjackService = blackjackService;

        [Command("start"), Summary("Start a BlackJack Match"), Usage("blackjack start")]
        public async Task StartMatchAsync()
        {
            if (!_blackjackService.StartMatch(Context.User.Id))
            {
                await ReplyEmbedAsync("Player already in Match");
                return;
            }

            await StartBlackJackAsync();
            _blackjackService.ClearMatches();

        }
        [Command("clear"), Summary("Clear all Matches"), Usage("blackjack clear")]
        public async Task ClearMatches()
        {
            _blackjackService.ClearMatches();

            await ReplyEmbedAsync("Matches Cleared");
        }
    }
}