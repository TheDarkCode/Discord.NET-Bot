using System;
using System.ComponentModel.DataAnnotations;

namespace ArcadesBot
{
    public class Token
    {
        [Required]
        public ulong Id { get; set; }
        [Required]
        public ulong UserId { get; set; }
        [Required]
        public ulong GuildId { get; set; }

        public ulong Tokens { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public DateTime LastPaydayTimeStamp { get; set; }

        /// <summary>Remove amount from user</summary>
        /// <param name="amount">Amount to remove</param>
        internal void RemoveTokens(ulong amount)
            => Tokens -= amount;

        /// <summary>Add amount from user</summary>
        /// <param name="amount">Amount to add</param>
        internal void AddTokens(ulong amount)
            => Tokens += amount;
    }
}