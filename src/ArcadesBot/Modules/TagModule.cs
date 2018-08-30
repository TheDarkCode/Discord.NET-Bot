using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
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

        [Command, Name("getTag"), Summary("Gets a tag with the given name."), Usage("tag \"test tag\"")]
        public async Task GetTagAsync([Summary("Name of the tag you want to retrieve from the database"), Remainder]string tagName)
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

        [Command("create", RunMode = RunMode.Async), Alias("new", "add", "c"), Priority(10), Summary("Creates tag with content ."), Usage("tag create \"test tag\" This is a test tag")]
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

        [Command("delete", RunMode = RunMode.Async), Alias("remove", "r"), Priority(10), Summary("Deletes given tag"), Usage("tag delete \"test tag\"")]
        public async Task DeleteAsync([Summary("The name of the tag")]TagModel tag)
        {
            if (!await OwnerShipCheck(tag))
                return;
            Tags.Remove(tag);
            await ReplyEmbedAsync($"Tag `{tag.TagName}` has been deleted.", document: DocumentType.Server);
        }

        [Command("alias", RunMode = RunMode.Async), Alias("a"), Priority(10), Summary("Gives alias to given tag"), Usage("tag alias \"test tag\" \"test tag alias\"")]
        public async Task AliasAsync([Summary("The name of the tag")]TagModel tag, [Remainder, Summary("The name of the alias")]string aliasName)
        {
            if (!await OwnerShipCheck(tag) || await TagExists(aliasName))
                return;

            Tags.Remove(tag);
            tag.Aliasses.Add(aliasName);
            Tags.Add(tag);
            await ReplyEmbedAsync($"Tag `{tag.TagName}` has been deleted.", document: DocumentType.Server);
        }

        [Command("info", RunMode = RunMode.Async), Alias("i"), Priority(10), Summary("Get information about given tag"), Usage("tag info \"test tag\"")]
        public async Task GetInfoAsync([Summary("The name of the tag"), Remainder] TagModel tag)
        {
            var tagIndex = Tags.OrderByDescending(x => x.Uses).Select((item, index) => new { item, index });
            var tagWithIndex = tagIndex.FirstOrDefault(x => x.item.TagName == tag.TagName);
            var embed = new EmbedBuilder().WithSuccessColor();
            var str = "";

            foreach (var alias in tag.Aliasses)
                str += alias + ", ";

            if (str.Length < 2)
                str = ", ";

            var user = Context.Guild.Users.FirstOrDefault(x => x.Id == tag.OwnerId + 2)?.Mention; // I have no clue why I need to add 2 to the userId here but the database seems to remove 2 from the user's Id 
            str = str.Substring(0, str.Length - 2);
            embed.WithDescription($"**{tag.TagName}**");
            embed.AddField("Owner", user, true);
            embed.AddField("Uses", tag.Uses, true);
            embed.AddField("Rank", tagWithIndex.index + 1, true);
            embed.WithFooter(str);
            

            await ReplyEmbedAsync(embed: embed);
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
            if (name.Length <= 4)
                return false;
            var isReserved = new[] { "help", "about", "delete", "remove", "delete", "info", "modify", "update", "user", "list" }
                .Any(x => name.ToLower().Contains(x));
            if (!Tags.Any(x => x.TagName == name || x.Aliasses.Contains(name)) && !isReserved)
                return false;
            await ReplyEmbedAsync("Tag name already taken, is shorter than 4 charaters or is a reserved word");
            return true;

        }
    }
}
