using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace ArcadesBot
{
    public static class StringHelper
    {
        public static async Task<string> DownloadImageAsync(HttpClient httpClient, string url)
        {
            var get = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
            var fileName = $"Arcade-{Guid.NewGuid().ToString("n").Substring(0, 8)}";
            using (var userImage = File.Create($"{fileName}.png"))
            {
                await userImage.WriteAsync(get, 0, get.Length).ConfigureAwait(false);
            }

            return $"{fileName}.png";
        }

        public static string CheckUser(IDiscordClient client, ulong userId)
        {
            var clientDiscord = client as DiscordSocketClient;
            var user = clientDiscord.GetUser(userId);
            return user == null ? "Unknown User." : user.Username;
        }

        public static string CheckRole(SocketGuild guild, ulong id)
        {
            var role = guild.GetRole(id);
            return role == null ? "Unknown Role." : role.Name;
        }

        public static string CheckChannel(SocketGuild guild, ulong id)
        {
            var channel = guild.GetTextChannel(id);
            return channel == null ? "Unknown Channel." : channel.Name;
        }

        public static string Replace(string message, string guild = null, string user = null)
        {
            var builder = new StringBuilder(message);
            builder.Replace("{guild}", guild);
            builder.Replace("{user}", user);
            return builder.ToString();
        }
    }
}