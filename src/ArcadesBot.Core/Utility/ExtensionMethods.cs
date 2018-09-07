using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace ArcadesBot.Utility
{
    /// <summary>
    /// The class that contains all extension methods I create
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Simply just makes the first character of a string uppercase
        /// </summary>
        /// <param name="input">N/A</param>
        /// <returns>Input with first character capitalized</returns>
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// Checks if role id exists in this guild.
        /// </summary>
        /// <param name="guild">N/A</param>
        /// <param name="id">Id of role you want to check</param>
        /// <returns>Name of role if exists otherwise returns "Unknown Role."</returns>
        public static string CheckRole(this SocketGuild guild, ulong id)
        {
            var role = guild.GetRole(id);
            return role == null ? "Unknown Role." : role.Name;
        }

        /// <summary>
        /// Templating for welcome/leave messages.
        /// </summary>
        /// <param name="message">N/A</param>
        /// <param name="guild">Replacement for "{guild}" in string</param>
        /// <param name="user">Replacement for "{user}" in string</param>
        /// <returns>String with replaced values</returns>
        public static string Replace(this string message, string guild = null, string user = null)
        {
            var builder = new StringBuilder(message);
            builder.Replace("{guild}", guild);
            builder.Replace("{user}", user);
            return builder.ToString();
        }

        /// <summary>
        /// Check if user id exists.
        /// </summary>
        /// <param name="client">N/A</param>
        /// <param name="userId">Id of user you want to check</param>
        /// <returns>Name of user if exists otherwise returns "Unknown Role."</returns>
        public static string CheckUser(this IDiscordClient client, ulong userId)
        {
            var clientDiscord = client as DiscordSocketClient;
            var user = clientDiscord?.GetUser(userId);
            return user == null ? "Unknown User." : user.Username;
        }

        /// <summary>
        /// Checks if channel id exists in this guild.
        /// </summary>
        /// <param name="guild">N/A</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string CheckChannel(this SocketGuild guild, ulong id)
        {
            var channel = guild.GetTextChannel(id);
            return channel == null ? "Unknown Channel." : channel.Name;
        }

        /// <summary>
        /// Create image of given URL.
        /// </summary>
        /// <param name="httpClient">N/A</param>
        /// <param name="url">URL of image you want to download.</param>
        /// <returns>Filename of created file.</returns>
        public static async Task<string> DownloadImageAsync(this HttpClient httpClient, string url)
        {
            var get = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
            var fileName = $"Arcade-{Guid.NewGuid().ToString("n").Substring(0, 8)}";
            using (var userImage = File.Create($"{fileName}.png"))
            {
                await userImage.WriteAsync(get, 0, get.Length).ConfigureAwait(false);
            }

            return $"{fileName}.png";
        }

        /// <summary>
        /// Gives EmbedBuilder the predefined error color
        /// </summary>
        /// <param name="builder">N/A</param>
        /// <returns>The EmbedBuilder</returns>
        public static EmbedBuilder WithErrorColor(this EmbedBuilder builder)
        {
            builder.Color = Color.DarkRed;
            return builder;
        }

        /// <summary>
        /// Gives EmbedBuilder the predefined info color
        /// </summary>
        /// <param name="builder">N/A</param>
        /// <returns>The EmbedBuilder</returns>
        public static EmbedBuilder WithInfoColor(this EmbedBuilder builder)
        {
            builder.Color = Color.DarkGrey;
            return builder;
        }

        /// <summary>
        /// Gives EmbedBuilder the predefined success color
        /// </summary>
        /// <param name="builder">N/A</param>
        /// <returns>The EmbedBuilder</returns>
        public static EmbedBuilder WithSuccessColor(this EmbedBuilder builder)
        {
            builder.Color = Color.DarkBlue;
            return builder;
        }
    }
}