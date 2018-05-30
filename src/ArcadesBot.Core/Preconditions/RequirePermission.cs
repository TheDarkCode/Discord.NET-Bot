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
        {
            AccessLevel = accessLevel;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo Command, IServiceProvider Provider)
        {
            var Context = context as CustomCommandContext;
            var GuildUser = Context.User as SocketGuildUser;
            var Owner = Context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult();
            var AdminPerms = Context.Guild.OwnerId == Context.User.Id || GuildUser.GuildPermissions.Administrator || GuildUser.GuildPermissions.ManageGuild || GuildUser.Id == Owner.Owner.Id;
            var ModPerms = new[] 
            {
                GuildPermission.KickMembers,
                GuildPermission.BanMembers,
                GuildPermission.ManageChannels,
                GuildPermission.ManageMessages,
                GuildPermission.ManageRoles
            };
            if (AccessLevel >= AccessLevel.Administrator && AdminPerms)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else if (AccessLevel >= AccessLevel.Moderator && ModPerms.Any(x => GuildUser.GuildPermissions.Has(x)))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"{Command.Name} requires **{AccessLevel}** AccessLevel. To learn more on AccessLevel, use `{Context.Config.Prefix}Info` command."));
        }
    }

    public enum AccessLevel
    {
        Moderator,
        Administrator
    }
}
