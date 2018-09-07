using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Interactive.Criteria;
using Discord.WebSocket;

namespace ArcadesBot.Interactive.Paginator
{
    internal class EnsureIsIntegerCriterion : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(CustomCommandContext sourceContext, SocketMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}
