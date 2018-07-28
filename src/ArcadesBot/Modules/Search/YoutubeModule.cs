using Discord.Commands;
using Google.Apis.YouTube.v3;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Youtube = Google.Apis.YouTube.v3.Data;


namespace ArcadesBot
{
    [Name("Youtube")]
    [Summary("Search for things on youtube.")]
    public class YoutubeModule : ModuleBase<CustomCommandContext>
    {
        private YouTubeService _youtube { get; }

        public YoutubeModule(YouTubeService youtube) 
            => _youtube = youtube;

        [Command("youtube"), Alias("yt")]
        [Summary("Search for a video matching the provided text")]
        public async Task SearchAsync([Remainder, Summary("The query you want to make to YouTube")]string query)
        {
            var video = await SearchYoutubeAsync(query, "youtube#video");

            if (video == null)
                await ReplyAsync($"I could not find a video like `{query}`");
            else
                await ReplyAsync($"http://youtube.com/watch?v={video.Id.VideoId}");
        }

        private async Task<Youtube.SearchResult> SearchYoutubeAsync(string query, string dataType)
        {
            var request = _youtube.Search.List("snippet");
            request.Q = query;
            request.MaxResults = 2;

            var result = await request.ExecuteAsync();
            return result.Items.FirstOrDefault(x => x.Id.Kind == dataType);
        }
    }
}
