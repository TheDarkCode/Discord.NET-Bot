using ArcadesBot.Services;

namespace ArcadesBot.Models.BlackJack
{
    public class CardModel
    {
        public CardModel(AssetService assetService)
        {
            _assetService = assetService;
            _cardValue = CardValue.None;
            _suit = CardSuit.None;
            ImagePath = null;
        }

        public CardModel(AssetService assetService, CardSuit suit, CardValue value)
        {
            _assetService = assetService;
            Suit = suit;
            Value = value;
        }

        public string ImagePath { get; private set; }
        private AssetService _assetService { get; }
        private CardValue _cardValue;
        private CardSuit _suit;

        public CardValue Value
        {
            get 
                => _cardValue;
            set
            {
                _cardValue = value;
                SetImagePath();
            }
        }
        public CardSuit Suit
        {
            get => _suit;
            set
            {
                _suit = value;
                SetImagePath();
            }
        }

        private void SetImagePath()
        {
            if (Suit == 0 || Value == 0)
                return;
            ImagePath = _assetService.GetImagePath("Cards", $"{Value.ToString()}{Suit.ToString()}.png");
        }
        public override string ToString()
        {
            var realValue = "";
            switch (_cardValue)
            {
                case CardValue.None:
                    return "Card Value not initialised";
                case CardValue.Deuce:
                case CardValue.Three:
                case CardValue.Four:
                case CardValue.Five:
                case CardValue.Six:
                case CardValue.Seven:
                case CardValue.Eight:
                case CardValue.Nine:
                case CardValue.Ten:
                    realValue = ((int) _cardValue).ToString();
                    break;
                case CardValue.Jack:
                case CardValue.Queen:
                case CardValue.King:
                case CardValue.Ace:
                    realValue = _cardValue.ToString();
                    break;
            }

            return $"{realValue} of {_suit}";
        }
    }

    public enum CardValue
    {
        None = 0,
        Ace = 1,
        Deuce = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        
    }

    public enum CardSuit
    {
        None = 0,
        Hearts = 1,
        Spades = 2,
        Clubs = 3,
        Diamonds = 4
    }
}