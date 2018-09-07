using System;
using System.Linq;
using System.Threading.Tasks;
using ArcadesBot.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ArcadesBot.CommandExtensions.Preconditions
{
    public class RequirePermission : PreconditionAttribute
    {
        public RequirePermission(AccessLevel accessLevel)
        {
            _accessLevel = accessLevel;
        }

        private AccessLevel _accessLevel { get; }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider provider)
        {
            var contextCustom = context as CustomCommandContext;
            var guildUser = contextCustom.User as SocketGuildUser;
            var owner = contextCustom.Client.GetApplicationInfoAsync().GetAwaiter().GetResult();
            var adminPerms = contextCustom.Guild.OwnerId == contextCustom.User.Id ||
                             guildUser.GuildPermissions.Administrator || guildUser.GuildPermissions.ManageGuild ||
                             guildUser.Id == owner.Owner.Id;
            var modPerms = new[]
            {
                GuildPermission.KickMembers,
                GuildPermission.BanMembers,
                GuildPermission.ManageChannels,
                GuildPermission.ManageMessages,
                GuildPermission.ManageRoles
            };
            if (_accessLevel >= AccessLevel.Administrator && adminPerms)
                return Task.FromResult(PreconditionResult.FromSuccess());
            if (_accessLevel >= AccessLevel.Moderator && modPerms.Any(x => guildUser.GuildPermissions.Has(x)))
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError(
                $"{command.Name} requires **{_accessLevel}** AccessLevel. To learn more on AccessLevel, use `{contextCustom.Config.Prefix}Info` command."));
        }
    }

    public enum AccessLevel
    {
        Moderator,
        Administrator
    }
}