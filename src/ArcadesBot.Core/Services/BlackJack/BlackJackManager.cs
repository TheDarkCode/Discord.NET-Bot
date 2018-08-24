using Discord.Commands;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArcadesBot
{
    public class BlackJackManager
    {
        private BlackJackHandler _blackJackHelper { get; }
        private AssetService _assetService { get; }

        public BlackJackManager(AssetService assetService, BlackJackHandler blackJackHandler)
        {
            _blackJackHelper = blackJackHandler;
            _assetService = assetService;
        }

        private GameModel _game { get; set; }

        public void StartGame(ICommandContext context)
        {
            _game.PlayingDeck = new DeckModel(_assetService);
            _game.GuildId = context.Guild.Id;
            _game.PlayerId = context.Message.Author.Id;
            DealFirstTwoCards();
        }
        
        public void DealFirstTwoCards()
        {
            _game.PlayingDeck.Shuffle();

            _game.Dealer.HitMe(_game.PlayingDeck.DealCard());
            _game.Dealer.HitMe(_game.PlayingDeck.DealCard());

            _game.Player.HitMe(_game.PlayingDeck.DealCard());
            _game.Player.HitMe(_game.PlayingDeck.DealCard());
        }

        public CardModel DealCard()
            => _game.PlayingDeck.DealCard();

        public string DisplayScores(bool fullDealerCards = false)
        {
            var dealerScore = GetDealerScore();
            if (!fullDealerCards)
                dealerScore = (int)_game.Dealer.ShowHand()[0].Value;
            var sb = new StringBuilder();
            sb.AppendLine(fullDealerCards ? $"Dealer has : {dealerScore}" : $"Dealer has : {dealerScore} but 1 card is hidden");
            sb.AppendLine($"Player has : {_game.Player.GetScore()}");
            return sb.ToString();
        }

        public string CreateImageAsync(bool showFullDealerCards = false)
        {
            var playerCardsPaths = new List<string>();
            var dealerCardsPaths = new List<string>();
            foreach (var card in _game.Player.ShowHand())
                playerCardsPaths.Add(card.ImagePath);

            foreach (var card in _game.Dealer.ShowHand())
                dealerCardsPaths.Add(card.ImagePath);

            var guid = Guid.NewGuid();
            var board = Image.Load(_assetService.GetImagePath("Cards", "board.png"));

            board.Mutate(processor =>
            {
                var playerCardCount = playerCardsPaths.Count;
                var playerStartWidth = ((board.Width / playerCardCount) / playerCardCount) - (playerCardCount * 10) / playerCardCount;

                var dealerCardCount = playerCardsPaths.Count;
                var dealerStartWidth = ((board.Width / dealerCardCount) / dealerCardCount) - (dealerCardCount * 10) / dealerCardCount;

                for (var i = 0; i < playerCardCount; i++)
                {
                    var image = Image.Load(playerCardsPaths[i]);
                    processor.DrawImage(image, new Size(image.Width, image.Height), new Point(playerStartWidth + (i+1) * image.Width + 10, board.Height - image.Height), new GraphicsOptions());
                }

                for (var i = 0; i < dealerCardCount; i++)
                {
                    var image = Image.Load(!showFullDealerCards && i == 0 ? _assetService.GetImagePath("Cards", "DealerHiddenCard.png") : dealerCardsPaths[i]);
                    processor.DrawImage(image, new Size(image.Width, image.Height), new Point(dealerStartWidth + ((i + 1) * image.Width) + 10, 0), new GraphicsOptions());
                }
            });
            board.Save($"{Directory.GetCurrentDirectory()}\\BlackJack\\board{guid}.png");

            return $"{guid}";
        }

        public void GivePlayerACard()
            => _game.Player.HitMe(_game.PlayingDeck.DealCard());

        public void GiveDealerACard()
            => _game.Dealer.HitMe(_game.PlayingDeck.DealCard());

        public int GetDealerScore()
            => _game.Dealer.GetScore();
    }
}
