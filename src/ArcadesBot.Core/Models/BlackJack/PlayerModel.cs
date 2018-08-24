using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sparrow.Platform.Posix.macOS;

namespace ArcadesBot
{
    public class PlayerModel
    {
        private readonly List<CardModel> _hand = new List<CardModel>();
        private int _myScore;

        public bool Busted { get; set; }

        public PlayerModel()
        {
        }

        public void HitMe(CardModel dealtCard) 
            => _hand.Add(dealtCard);

        public List<CardModel> ShowHand() 
            => _hand;

        public CardModel LastCard() 
            => _hand[_hand.Count - 1];

        public string ShoutHand()
        {
            var handString = new StringBuilder();
            foreach (var card in _hand)
            {
                handString.AppendLine(card.Suit.ToString() + ' ' + card.Value);
            }
            return handString.ToString();
        }

        public int GetScore()
        {
            _myScore = 0;
            foreach (var t in _hand)
            {
                if ((int)t.Value < 10)
                {
                    _myScore += (int)t.Value;
                }
                else
                {
                    _myScore += 10;
                }
            }

            if (_hand.Count == 2 && (_hand[0].Value == CardValue.Ace && (int)_hand[1].Value >= 10 || _hand[1].Value == CardValue.Ace && (int)_hand[0].Value >= 10))
            {
                return 21;
            }
          
            if (_myScore > 21)
            {
                Busted = true;
            }

            if (_myScore + 10 < 21 && _hand.Any(x => x.Value == CardValue.Ace))
                _myScore += 10;

            return _myScore;
        }

        public void ThrowCards()
        {
            _hand.Clear();
            Busted = false;
        }
    }
}