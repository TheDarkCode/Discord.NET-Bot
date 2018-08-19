using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace ArcadesBot
{
    public class BlackJackManager
    {
        private BlackJackHelper _blackJackHelper { get; }
        private AssetService _assetService { get; }

        public BlackJackManager(AssetService assetService, BlackJackHelper blackJackHelper)
        {
            _blackJackHelper = blackJackHelper;
            _assetService = assetService;
            _playingDeck = new DeckModel(_assetService);
            DealFirstTwoCards();
        }
        private readonly DeckModel _playingDeck;
        private readonly PlayerModel _player = new PlayerModel();
        private readonly PlayerModel _dealer = new PlayerModel();

        public void DealFirstTwoCards()
        {
            _playingDeck.Shuffle();

            _dealer.HitMe(_playingDeck.DealCard());
            _dealer.HitMe(_playingDeck.DealCard());

            _player.HitMe(_playingDeck.DealCard());
            _player.HitMe(_playingDeck.DealCard());
        }

        public CardModel DealCard()
            => _playingDeck.DealCard();

        private int GetHandScore(List<CardModel> cards)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));
            var score = 0;
            foreach (var t in cards)
            {
                var cardValue = (int)t.Value;
                if (cardValue > 10)
                {
                    cardValue = 10;
                }
                score += cardValue;
            }

            if (score > 21)
            {
                score = -1;
            }

            //Blackjack returns 0
            if (cards.Count == 2 && (cards[0].Value == CardValue.Ace && (int)cards[1].Value >= 10 || cards[1].Value == CardValue.Ace && (int)cards[0].Value >= 10))
            {
                return 0;
            }

            return score;
        }

        public string DisplayScores(bool fullDealerCards = false)
        {
            var dealerScore = GetDealerScore();
            if (!fullDealerCards)
                dealerScore = (int)_dealer.ShowHand()[0].Value;
            var sb = new StringBuilder();
            sb.AppendLine(fullDealerCards ? $"Dealer has : {dealerScore}" : $"Dealer has : {dealerScore} but 1 card is hidden");
            sb.AppendLine($"Player has : {_player.GetScore()}");
            return sb.ToString();
        }

        public string CreateImageAsync(bool showFullDealerCards = false)
        {
            var playerCardsPaths = new List<string>();
            var dealerCardsPaths = new List<string>();
            foreach (var card in _player.ShowHand())
                playerCardsPaths.Add(card.ImagePath);

            foreach (var card in _dealer.ShowHand())
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

            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\BlackJack\\"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\BlackJack\\");
            board.Save($"{Directory.GetCurrentDirectory()}\\BlackJack\\board{guid}.png");

            return $"{guid}";
        }

        public void GiveDealerACard()
            => _dealer.HitMe(_playingDeck.DealCard());

        public int GetDealerScore()
            => _dealer.GetScore();
    }
}
