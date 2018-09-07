using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Services;

namespace ArcadesBot.Modules.Eval
{
    [Group("eval"), Name("Eval")]
    [RequireOwner]
    public class EvalModule : ModuleBase<CustomCommandContext>
    {
        private RoslynManager _roslyn { get; }

        public EvalModule(IServiceProvider provider) 
            => _roslyn = provider.GetService<RoslynManager>();

        [Command(RunMode = RunMode.Async)]
        public async Task EvalAsync([Remainder]string code)
        {
            var result = await _roslyn.EvalAsync(Context, code);
            await ReplyAsync("", embed: result);
        }
    }
}
