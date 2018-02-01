using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class IsAdmin : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ownerId = (await services.GetService<DiscordSocketClient>().GetApplicationInfoAsync()).Owner.Id;
            if (ownerId == context.User.Id)
                return PreconditionResult.FromSuccess();
            var config = await services.GetService<ConfigDatabase>().GetConfigAsync(context.Guild.Id);
            if(!(context.User is SocketGuildUser))
                return PreconditionResult.FromError("Not invoked in a Guild");
            var user = (SocketGuildUser)context.User;
            return user.Roles.Any(role => config.AdminRoleList.Contains(role.Id)) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("User does not have a Botadmin role");
        }
    }
}
