using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot.Modules
{
    
    [Group("info"), Name("Info")]
    [Summary("")]
    public class InfoModule : Base
    {
        private readonly Process _process;

        public InfoModule() 
            => _process = Process.GetCurrentProcess();

        [Command, Summary("Get some information about the bot")]
        public async Task InfoAsync()
        {
            var app = await Context.Client.GetApplicationInfoAsync();

            var builder = new EmbedBuilder()
                .WithAuthor(x => 
                {
                    x.Name = app.Owner.ToString();
                    x.IconUrl = app.Owner.GetAvatarUrl();
                })
                .AddField("Memory", GetMemoryUsage(), true)
                .AddField("Latency", GetLatency(), true)
                .AddField("Uptime", GetUptime(), true)
                .WithFooter(x => x.Text = GetLibrary());

            await ReplyEmbedAsync(embed: builder.Build());
        }

        public string GetUptime()
        {
            var uptime = (DateTime.Now - _process.StartTime);
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        }

        public string GetLibrary()
            => $"Discord.Net ({DiscordConfig.Version})";
        public string GetMemoryUsage()
            => $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)}mb";
        public string GetLatency()
            => $"{Context.Client.Latency}ms";

    }
}
