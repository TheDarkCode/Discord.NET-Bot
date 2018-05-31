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
        public static string MultipleOrNot(string input, ulong amount)
            => amount == 1 ? $"{input}" : $"{input}s";

        public static string ParametersInfo(IReadOnlyCollection<ParameterInfo> Parameters)
            => Parameters.Any() ?
            string.Join(" ", Parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` ")) : null;

        public static async Task<string> DownloadImageAsync(HttpClient HttpClient, string URL)
        {
            var Get = await HttpClient.GetByteArrayAsync(URL).ConfigureAwait(false);
            string FileName = $"Arcade-{Guid.NewGuid().ToString("n").Substring(0, 8)}";
            using (var UserImage = File.Create($"{FileName}.png"))
                await UserImage.WriteAsync(Get, 0, Get.Length).ConfigureAwait(false);
            return $"{FileName}.png";
        }

        public static string CheckUser(IDiscordClient client, ulong UserId)
        {
            var Client = client as DiscordSocketClient;
            var User = Client.GetUser(UserId);
            return User == null ? "Unknown User." : User.Username;
        }

        public static string CheckRole(SocketGuild Guild, ulong Id)
        {
            var Role = Guild.GetRole(Id);
            return Role == null ? "Unknown Role." : Role.Name;
        }

        public static string CheckChannel(SocketGuild Guild, ulong Id)
        {
            var Channel = Guild.GetTextChannel(Id);
            return Channel == null ? "Unknown Channel." : Channel.Name;
        }

        public static string Replace(string Message, string Guild = null, string User = null)
        {
            StringBuilder Builder = new StringBuilder(Message);
            Builder.Replace("{guild}", Guild);
            Builder.Replace("{user}", User);
            return Builder.ToString();
        }
    }
}