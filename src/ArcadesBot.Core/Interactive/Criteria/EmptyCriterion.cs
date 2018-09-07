using System.Threading.Tasks;
using ArcadesBot.Common;

namespace ArcadesBot.Interactive.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(CustomCommandContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}
