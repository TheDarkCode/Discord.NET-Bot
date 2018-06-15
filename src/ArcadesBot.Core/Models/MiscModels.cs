using Discord;
using System;
using System.Collections.Generic;

namespace ArcadesBot
{
    public enum Scope
    {
        User,
        Guild,
        Global
    }
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
        public MutedInfo MutedInfo { get; set; } = new MutedInfo();
        public Dictionary<string, int> Commands { get; set; } = new Dictionary<string, int>();
    }
    public class WebhookOptions
    {
        public string Name { get; set; }
        public Embed Embed { get; set; }
        public string Message { get; set; }
        public WebhookWrapper Webhook { get; set; } = new WebhookWrapper();
    }
    public class WebhookWrapper
    {
        public ulong TextChannel { get; set; }
        public ulong WebhookId { get; set; }
        public string WebhookToken { get; set; }
    }
    public class ModWrapper
    {
        public ulong JoinRole { get; set; }
        public ulong MuteRole { get; set; }
        public ulong TextChannel { get; set; }
        public bool LogDeletedMessages { get; set; }
    }
    public class MutedInfo
    {
        public bool IsMuted { get; set; } = false;
        public DateTime? MutedUntill { get; set; } = null;
        public List<ulong> Roles { get; set; } = null;
    }
}
