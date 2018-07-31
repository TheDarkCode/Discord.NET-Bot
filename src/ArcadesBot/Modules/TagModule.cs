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

    [Group("tag"), Name("Tag"), Alias("t")]
    [Summary("Tag commands")]
    public class TagModule : Base
    {

        private List<TagModel> Tags
            => Context.Server.Tags;

        public TagModule()
        {

        }

        [Command, Name("getTag"), Summary("Gets a tag with the given name.")]
        public async Task GetTagAsync([Summary("Name of the tag you want to retrieve from the database")]string tagName)
        {
            var tag = Tags.FirstOrDefault(x => x.TagName == tagName);
            if(tag == null)
            {
                await ReplyEmbedAsync("Tag doesn't exist");
                return;
            }
            tag.Uses++;
            await ReplyEmbedAsync($"{tag.Content}", document: DocumentType.Server);
        }

        [Command("create", RunMode = RunMode.Async), Alias("new", "add"), Priority(10), Summary("Creates tag with content .")]
        public async Task CreateAsync([Summary("The name of the tag")]string name, [Remainder, Summary("The content of the tag")]string content)
        {
            if (await TagExists(name))
                return;

            Tags.Add(new TagModel
            {
                TagName = name,
                OwnerId = Context.User.Id,
                Content = content
            });
            await ReplyEmbedAsync($"Tag `{name}` has been created.", document: DocumentType.Server);
        }

        [Command("delete", RunMode = RunMode.Async), Alias("remove"), Priority(10), Summary("Deletes given tag")]
        public async Task DeleteAsync([Summary("The name of the tag")]TagModel tag)
        {
            if (!await OwnerShipCheck(tag))
                return;
            Tags.Remove(tag);
            await ReplyEmbedAsync($"Tag `{tag.TagName}` has been deleted.", document: DocumentType.Server);
        }

        [Command("alias", RunMode = RunMode.Async), Alias("a"), Priority(10), Summary("Gives alias to given tag")]
        public async Task AliasAsync([Summary("The name of the tag")]TagModel tag, [Remainder, Summary("The name of the alias")]string aliasName)
        {
            if (!await OwnerShipCheck(tag) || await TagExists(aliasName))
                return;

            Tags.Remove(tag);
            tag.Aliasses.Add(aliasName);
            Tags.Add(tag);
            await ReplyEmbedAsync($"Tag `{tag.TagName}` has been deleted.", document: DocumentType.Server);
        }


        private async Task<bool> OwnerShipCheck(TagModel tag)
        {
            if (tag.OwnerId == Context.User.Id)
                return true;

            await ReplyEmbedAsync("You don't own this tag");
            return false;

        }

        private async Task<bool> TagExists(string name)
        {
            var isReserved = new[] { "help", "about", "tag", "delete", "remove", "delete", "info", "modify", "update", "user", "list", "new", "add" }
                .Any(x => name.ToLower().Contains(x));
            if (!Tags.Any(x => x.TagName == name || x.Aliasses.Contains(name)) && !isReserved)
                return false;
            await ReplyEmbedAsync("Tag name already taken or is a reserved word");
            return true;

        }
    }
}
