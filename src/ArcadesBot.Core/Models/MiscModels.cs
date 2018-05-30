using System;
using System.Collections.Generic;

namespace ArcadesBot
{
    public class MessageWrapper
    {
        public string Content { get; set; }
        public ulong AuthorId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public DateTime DateTime { get; set; }
    }
    public class UserProfile
    {
        public bool IsBlacklisted { get; set; }
        public Dictionary<string, int> Commands { get; set; } = new Dictionary<string, int>();
    }
}
