using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class TagModelTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var Context = context as CustomCommandContext;

            var tag = Context.Server.Tags.FirstOrDefault(x => x.TagName == input || x.Aliasses.Contains(input));

            return Task.FromResult(tag == null 
                ? TypeReaderResult.FromError(CommandError.ObjectNotFound, "Tag doesn't exist") 
                : TypeReaderResult.FromSuccess(tag));
        }
    }
}
