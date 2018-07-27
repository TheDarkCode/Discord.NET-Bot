using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot.Modules
{

    [Group("tag"), Name("Tag commands")]
    [Summary("")]
    public class TagModule : Base
    {

        public TagModule()
        {

        }

        [Command, Summary("Gets a tag with the given name."), Alias("t")]
        public async Task GetTagAsync()
        {

            await ReplyAsync("");
        }

    }
}
