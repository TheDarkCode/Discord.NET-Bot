using System;
using System.Collections.Generic;
using ArcadesBot.Services;

namespace ArcadesBot.Models.BlackJack
{
    public class DeckModel
    {
        public DeckModel(AssetService assetService) 
            => _assetService = assetService;

        private Queue<CardModel> _deckOfCards;
        private List<int> _cardsAsInt;
        private AssetService _assetService { get; }
        private readonly Random _generator = new Random();

        public void Shuffle()
        {
            _deckOfCards = new Queue<CardModel>();
            GenerateCardsAsInt();
            for (var i = 51; i >= 0; i--)
            {
                var index = _generator.Next(0, i);
                var temp = _cardsAsInt[i];
                _cardsAsInt[i] = _cardsAsInt[index];
                _cardsAsInt[index] = temp;
            }
            FillDeck();
        }

        public CardModel DealCard() 
            => _deckOfCards.Dequeue();

        private void GenerateCardsAsInt()
        {
            _cardsAsInt = new List<int>();
            for (var i = 0; i < 52; i++)
            {
                _cardsAsInt.Add(i);
            }
        }

        private void FillDeck()
        {
            foreach (var t in _cardsAsInt)
            {
                var suit = (CardSuit)(t % 4 + 1);
                var value = (CardValue)(t % 13 + 1);
                _deckOfCards.Enqueue(new CardModel(_assetService, suit, value));
            }
        }
    }
}