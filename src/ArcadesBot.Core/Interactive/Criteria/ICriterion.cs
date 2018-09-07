using System.Threading.Tasks;
using ArcadesBot.Common;

namespace ArcadesBot.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(CustomCommandContext sourceContext, T parameter);
    }
}
