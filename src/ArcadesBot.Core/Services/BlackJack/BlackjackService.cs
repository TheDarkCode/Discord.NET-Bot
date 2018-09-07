using System.Collections.Generic;
using System.Linq;
using ArcadesBot.Handlers;

namespace ArcadesBot.Services.BlackJack
{
    public class BlackJackService
    {
        private AssetService _assetService { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private BlackJackHandler _blackJackHandler { get; }
        public BlackJackService(AssetService assetService, BlackJackHandler blackJackHandler)
        {
            _assetService = assetService;
            _blackJackHandler = blackJackHandler;
        }

        private readonly Dictionary<ulong, BlackJackManager> _matchList = new Dictionary<ulong, BlackJackManager>();

        public bool StartMatch(ulong playerId)
        {
            if (_matchList.All(x => x.Key != playerId) && _matchList.Count != 0)
                return false;
            _matchList.Add(playerId, new BlackJackManager(_assetService));
            return true;
        }

        public string CreateImage(ulong playerId, bool showFullDealerCards = false)
        {
            if (_matchList.All(x => x.Key != playerId) && _matchList.Count != 0)
                return "";
            _matchList.TryGetValue(playerId, out var match);

            return match.CreateImageAsync(showFullDealerCards);
        }

        public string DrawCard(ulong playerId, bool dealer = false)
        {
            if (_matchList.All(x => x.Key != playerId))
                return "Player isn't in match";
            _matchList.TryGetValue(playerId, out var match);

            if(dealer)
                match.GiveDealerACard();
            else
                match.GivePlayerACard();

            return CreateImage(playerId);
        }


        public string GetScoreFromMatch(ulong playerId)
        {
            if (_matchList.All(x => x.Key != playerId))
                return "Player isn't in match";
            _matchList.TryGetValue(playerId, out var match);

            return match.DisplayScores();
        }

        public bool RemovePlayerFromMatch(ulong playerId)
        {
            if (_matchList.All(x => x.Key != playerId))
                return false;

            _matchList.Remove(playerId);
            return true;
        }

        public void ClearMatches() 
            => _matchList.Clear();
    }
} 