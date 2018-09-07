using System;
using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Interactive.Criteria;
using Discord.Commands;
using Discord.WebSocket;

namespace ArcadesBot.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        CustomCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
