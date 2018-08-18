using System;
using System.Collections.Generic;
using System.Text;

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

        public CardModel DealerVisibleCard
            => _dealer.ShowHand()[0];

        public CardModel DealerLastCard
            => _dealer.ShowHand()[_dealer.ShowHand().Count - 1];

        public List<CardModel> GetDealerCards()
            => _dealer.ShowHand();


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

            var test = GetDealerScore();
            if (!fullDealerCards)
                test = (int) DealerVisibleCard.Value;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(fullDealerCards ? $"Dealer has :{test}" : $"Dealer has :{test} but 1 card is hidden");
            sb.AppendLine($"Player has :{_player.GetScore()}");
            return sb.ToString();
        }

        public void GiveDealerACard()
            => _dealer.HitMe(_playingDeck.DealCard());

        public int GetDealerScore()
            => _dealer.GetScore();
    }
}
