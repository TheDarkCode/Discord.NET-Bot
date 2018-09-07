using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Interactive.Criteria;
using Discord.WebSocket;

namespace ArcadesBot.Interactive.Paginator
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {

        public Task<bool> JudgeAsync(CustomCommandContext sourceContext, SocketReaction parameter)
        {
            bool ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}
