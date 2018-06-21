using System;

namespace ArcadesBot
{
    public class ChessChallengeModel
    {
        public string Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ChallengerId { get; set; }
        public ulong ChallengeeId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime TimeoutDate { get; set; }
        public bool Accepted { get; set; } = false;
    }
}
