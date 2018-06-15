using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class RequirePermission : PreconditionAttribute
    {
        AccessLevel AccessLevel { get; }
        public RequirePermission(AccessLevel accessLevel) 
            => AccessLevel = accessLevel;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider provider)
        {
            var contextCustom = context as CustomCommandContext;
            var guildUser = contextCustom.User as SocketGuildUser;
            var owner = contextCustom.Client.GetApplicationInfoAsync().GetAwaiter().GetResult();
            var adminPerms = contextCustom.Guild.OwnerId == contextCustom.User.Id || guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild || guildUser.Id == owner.Owner.Id;
            var modPerms = new[] 
            {
                GuildPermission.KickMembers,
                GuildPermission.BanMembers,
                GuildPermission.ManageChannels,
                GuildPermission.ManageMessages,
                GuildPermission.ManageRoles
            };
            if (AccessLevel >= AccessLevel.Administrator && adminPerms)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else if (AccessLevel >= AccessLevel.Moderator && modPerms.Any(x => guildUser.GuildPermissions.Has(x)))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"{command.Name} requires **{AccessLevel}** AccessLevel. To learn more on AccessLevel, use `{contextCustom.Config.Prefix}Info` command."));
        }
    }

    public enum AccessLevel
    {
        Moderator,
        Administrator
    }
}
