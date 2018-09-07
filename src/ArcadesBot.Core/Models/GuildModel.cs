using System.Collections.Generic;

namespace ArcadesBot.Models
{
    public class GuildModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public bool IsConfigured { get; set; }
        public List<string> JoinMessages { get; set; } = new List<string>(5);
        public List<string> LeaveMessages { get; set; } = new List<string>(5);
        public ModWrapper Mod { get; set; } = new ModWrapper();
        public List<ulong> AssignableRoles { get; set; } = new List<ulong>(10);
        public Dictionary<ulong, string> Afk { get; set; } = new Dictionary<ulong, string>();
        public List<ulong> BlackListedChannels { get; set; } = new List<ulong>();
        public WebhookWrapper JoinWebhook { get; set; } = new WebhookWrapper();
        public WebhookWrapper LeaveWebhook { get; set; } = new WebhookWrapper();
        public Dictionary<ulong, UserProfile> Profiles { get; set; } = new Dictionary<ulong, UserProfile>();
        public List<TagModel> Tags { get; set; } = new List<TagModel>();
    }
}