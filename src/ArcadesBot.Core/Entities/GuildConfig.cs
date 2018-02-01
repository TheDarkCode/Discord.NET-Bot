using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ArcadesBot
{
    public class GuildConfig : IEntity<ulong>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; private set; }
        [Required]
        public ulong GuildId { get; set; }
        public string Prefix { get; set; }
        [NotMapped]
        public List<ulong> NotAllowedChannelList { get; set; }
        public string BlockedChannels
        {
            get => JsonConvert.SerializeObject(NotAllowedChannelList);
            set => NotAllowedChannelList = JsonConvert.DeserializeObject<List<ulong>>(value);
        }
        [NotMapped]
        public List<ulong> ModRolesList { get; set; }
        public string ModRoles
        {
            get => JsonConvert.SerializeObject(ModRolesList);
            set => ModRolesList = JsonConvert.DeserializeObject<List<ulong>>(value);
        }
        [NotMapped]
        public List<ulong> AdminRoleList { get; set; }
        public string AdminRoles
        {
            get => JsonConvert.SerializeObject(AdminRoleList);
            set => AdminRoleList = JsonConvert.DeserializeObject<List<ulong>>(value);
        }
        public ulong TimeoutInSeconds { get; set; } = 30;
        public GuildConfig() { }
        public GuildConfig(ulong guildId)
        {
            GuildId = guildId;
            BlockedChannels = "[]";
            ModRoles = "[]";
            AdminRoles = "[]";
        }
    }
}
