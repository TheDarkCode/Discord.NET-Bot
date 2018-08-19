using System;
using System.Threading.Tasks;
using ArcadesBot;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
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
