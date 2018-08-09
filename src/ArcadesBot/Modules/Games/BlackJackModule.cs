using System.Threading.Tasks;
using Discord.Commands;

namespace ArcadesBot
{
    [Group("blackjack"), Alias("b")]
    public class BlackJackModule : Base
    {
        [Command("start")]
        public async Task StartMatchAsync()
        {
            


            await ReplyEmbedAsync("");
        }
    }
}