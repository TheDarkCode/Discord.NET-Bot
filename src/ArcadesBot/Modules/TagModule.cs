using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot
{

    [Group("tag"), Name("Tag")]
    [Summary("Tag commands")]
    public class TagModule : Base
    {

        private List<TagModel> Tags
            => Context.Server.Tags;

        public TagModule()
        {

        }

        [Command, Name("getTag"), Summary("Gets a tag with the given name."), Alias("t")]
        public async Task GetTagAsync([Summary("Name of the tag you want to retrieve from the database")]string tagName)
        {
            var tag = Tags.Where(x => x.TagName == tagName).FirstOrDefault();
            if(tag == null)
            {
                await ReplyEmbedAsync("Tag doesn't exist");
                return;
            }
            tag.Uses++;
            await ReplyEmbedAsync($"{tag.Content}", document: DocumentType.Server);
        }
        [Command("create", RunMode = RunMode.Async), Alias("make", "add"), Priority(10), Summary("Initiates Tag Creation wizard.")]
        public async Task CreateAsync([Summary("The name of the tag")]string name, [Remainder, Summary("The content of the tag")]string content)
        {
            if (Tags.Any(x => x.TagName == name) || NameCheck(name))
            {
                await ReplyEmbedAsync("Tag name already taken or is a reserved word");
                return;
            }
            Context.Server.Tags.Add(new TagModel
            {
                TagName = name,
                OwnerId = Context.User.Id,
                Content = content
            });
            await ReplyEmbedAsync($"Tag `{name}` has been created.", document: DocumentType.Server);
        }
        [Command("delete", RunMode = RunMode.Async), Alias("remove"), Priority(10), Summary("Initiates Tag Creation wizard.")]
        public async Task DeleteAsync([Summary("The name of the tag")]string name)
        {
            if (!Tags.Any(x => x.TagName == name))
            {
                await ReplyEmbedAsync("Tag doesn't exist or you don't own the tag");
                return;
            }
            Context.Server.Tags.RemoveAll(x => x.TagName == name);
            await ReplyEmbedAsync($"Tag `{name}` has been deleted.", document: DocumentType.Server);
        }
        private bool NameCheck(string name)
            => new[] { "help", "about", "tag", "delete", "remove", "delete", "info", "modify", "update", "user", "list" }
                .Any(x => name.ToLower().Contains(x));
    }
}
