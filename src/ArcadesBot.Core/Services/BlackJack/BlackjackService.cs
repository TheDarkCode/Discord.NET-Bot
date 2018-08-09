using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadesBot
{
    public class BlackjackService
    {
        private BlackJackHelper _blackJackHelper { get; }
        private AssetService _assetService { get; }

        public BlackjackService(AssetService assetService, BlackJackHelper blackJackHelper)
        {
            _blackJackHelper = blackJackHelper;
            _assetService = assetService;
            _playingDeck = new DeckModel(_assetService);
        }
        private DeckModel _playingDeck;
        private List<PlayerModel> _players = new List<PlayerModel>();
        private List<int> _playerScores = new List<int>();
        private PlayerModel _dealer = new PlayerModel();
        public bool Playing { get; set; }

        public CardModel DealerVisibleCard 
            => _dealer.ShowHand()[0];

        public CardModel DealerLastCard 
            => _dealer.ShowHand()[_dealer.ShowHand().Count - 1];

        public List<CardModel> GetDealerCards() 
            => _dealer.ShowHand();

        public BlackjackService(PlayerModel player1)
        {
            _players.Add(player1);
        }

        public void DealFirstTwoCards()
        {
            _playingDeck.Shuffle();

            _dealer.HitMe(_playingDeck.DealCard());
            _dealer.HitMe(_playingDeck.DealCard());

            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].HitMe(_playingDeck.DealCard());
                _players[i].HitMe(_playingDeck.DealCard());
            }
        }

        public CardModel DealCard() 
            => _playingDeck.DealCard();

        public void StartNewDeal()
        {
            _playingDeck = new DeckModel(_assetService);
            foreach (var t in _players)
            {
                t.ThrowCards();
                _dealer.ThrowCards();
            }
            DealFirstTwoCards();
        }

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
            if (cards.Count == 2 && ((cards[0].Value == CardValue.Ace && (int)cards[1].Value >= 10) || (cards[1].Value == CardValue.Ace && (int)cards[0].Value >= 10)))
            {
                return 0;
            }

            return score;
        }

        public string DisplayScores()
        {
            StringBuilder showScores = new StringBuilder();
            for (int i = 0; i < _players.Count; i++)
            {
                showScores.AppendLine(String.Format("Player {0} score: {1}", i, _playerScores[i]));
            }
            return showScores.ToString();
        }

        public void GiveDealerACard() 
            => _dealer.HitMe(_playingDeck.DealCard());

        public int GetDealerScore() 
            => _dealer.GetScore();
    }
}
