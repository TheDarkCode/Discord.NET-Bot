//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using Discord;
//using Newtonsoft.Json;

//namespace ArcadesBot
//{
//    public class ChessChallenge : IEntity<ulong>
//    {
//        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public ulong Id { get; private set; }
//        [Required]
//        public ulong GuildId { get; set; }
//        [Required]
//        public ulong ChannelId { get; set; }
//        [Required]
//        public ulong ChallengerId { get; set; }
//        [Required]
//        public ulong ChallengeeId { get; set; }
//        public DateTime DateCreated { get; set; } = DateTime.Now;
//        [Required]
//        public DateTime TimeoutDate { get; set; }
//        public bool Accepted { get; set; } = false;
//        public ChessChallenge() { }
//        public ChessChallenge(ulong _guildId, ulong _channelId, ulong _challenger, ulong _challengee, DateTime _timeoutDate)
//        {
//            GuildId = _guildId;
//            ChannelId = _channelId;
//            ChallengerId = _challenger;
//            ChallengeeId = _challengee;
//            TimeoutDate = _timeoutDate;
//        }
//    }
//}
