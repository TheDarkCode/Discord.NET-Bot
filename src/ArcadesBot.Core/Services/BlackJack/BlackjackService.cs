using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace ArcadesBot
{
    public class BlackJackService
    {
        private AssetService _assetService { get; }
        private BlackJackHelper _blackJackHelper { get; }
        public BlackJackService(AssetService assetService, BlackJackHelper blackJackHelper)
        {
            _assetService = assetService;
            _blackJackHelper = blackJackHelper;
        }

        private readonly Dictionary<ulong, BlackJackManager> _matchList = new Dictionary<ulong, BlackJackManager>();

        public bool StartMatch(ulong playerId)
        {
            if (_matchList.All(x => x.Key != playerId) && _matchList.Count != 0)
                return false;
            _matchList.Add(playerId, new BlackJackManager(_assetService, _blackJackHelper));
            return true;
        }

        public string CreateImage(ulong playerId, bool showFullDealerCards = false)
        {
            if (_matchList.All(x => x.Key != playerId) && _matchList.Count != 0)
                return "";
            _matchList.TryGetValue(playerId, out var match);

            return match.CreateImageAsync(showFullDealerCards);
        }


        public string GetScoreFromMatch(ulong playerId)
        {
            if (_matchList.All(x => x.Key != playerId))
                return "Player isn't in match";
            _matchList.TryGetValue(playerId, out var match);

            return match.DisplayScores();

        }

        public void ClearMatches() 
            => _matchList.Clear();
    }
} 