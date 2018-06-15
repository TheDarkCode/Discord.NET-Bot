using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ArcadesBot
{
    public static class StringHelper
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string ParametersInfo(IReadOnlyCollection<ParameterInfo> parameters)
            => parameters.Any() ?
            string.Join(" ", parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` ")) : null;

        public static async Task<string> DownloadImageAsync(HttpClient httpClient, string url)
        {
            var get = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
            var fileName = $"Arcade-{Guid.NewGuid().ToString("n").Substring(0, 8)}";
            using (var userImage = File.Create($"{fileName}.png"))
                await userImage.WriteAsync(get, 0, get.Length).ConfigureAwait(false);
            return $"{fileName}.png";
        }

        public static string CheckUser(IDiscordClient client, ulong userId)
        {
            var client = client as DiscordSocketClient;
            var user = client.GetUser(userId);
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