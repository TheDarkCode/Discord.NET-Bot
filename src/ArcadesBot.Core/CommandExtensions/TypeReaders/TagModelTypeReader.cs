using System;
using System.Linq;
using System.Threading.Tasks;
using ArcadesBot.Common;
using Discord.Commands;

namespace ArcadesBot.CommandExtensions.TypeReaders
{
    public class TagModelTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext contextParam, string input, IServiceProvider services)
        {
            var context = contextParam as CustomCommandContext;

            var tag = context.Server.Tags.FirstOrDefault(x => x.TagName == input || x.Aliasses.Contains(input));

            return Task.FromResult(tag == null 
                ? TypeReaderResult.FromError(CommandError.ObjectNotFound, "Tag doesn't exist") 
                : TypeReaderResult.FromSuccess(tag));
        }
    }
}
