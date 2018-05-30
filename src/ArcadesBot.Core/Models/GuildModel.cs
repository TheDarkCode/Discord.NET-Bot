using System.Collections.Generic;

namespace ArcadesBot
{
    public class GuildModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public bool IsConfigured { get; set; }
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public List<ulong> AssignableRoles { get; set; } = new List<ulong>(10);
        public Dictionary<ulong, string> AFK { get; set; } = new Dictionary<ulong, string>();
        public List<MessageWrapper> DeletedMessages { get; set; } = new List<MessageWrapper>();
        public Dictionary<ulong, UserProfile> Profiles { get; set; } = new Dictionary<ulong, UserProfile>();
    }
}