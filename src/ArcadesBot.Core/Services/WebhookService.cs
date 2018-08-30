using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class WebhookService
    {
        public WebhookService(HttpClient httpClient, GuildHandler guild, DiscordSocketClient client)
        {
            _guildhandler = guild;
            _client = client;
            _httpClient = httpClient;
        }

        private HttpClient _httpClient { get; }
        private GuildHandler _guildhandler { get; }
        private DiscordSocketClient _client { get; }

        private FileStream AvatarStream()
        {
            if (File.Exists("Avatar.jpg"))
                return new FileStream("Avatar.jpg", FileMode.Open, FileAccess.Read);
            return new FileStream(
                _httpClient.DownloadImageAsync(_client.CurrentUser.GetAvatarUrl()).GetAwaiter()
                    .GetResult(),
                FileMode.Open, FileAccess.Read);
        }

        public DiscordWebhookClient WebhookClient(ulong id, string token)
        {
            try
            {
                return new DiscordWebhookClient(id, token);
            }
            catch
            {
                PrettyConsole.Log(LogSeverity.Error, "Webhook", $"Webhook {id} Failed.");
                return null;
            }
        }

        public async Task SendMessageAsync(WebhookOptions options)
        {
            if (!(_client.GetChannel(options.Webhook.TextChannel) is SocketTextChannel channel)) return;
            var client = WebhookClient(options.Webhook.WebhookId, options.Webhook.WebhookToken);
            await WebhookFallbackAsync(client, channel, options);
        }

        public async Task<WebhookWrapper> CreateWebhookAsync(SocketTextChannel channel, string name)
        {
            var get = await GetWebhookAsync(channel, new WebhookOptions
            {
                Name = name
            });
            var webhook = get ?? await channel.CreateWebhookAsync(name, AvatarStream());
            return new WebhookWrapper
            {
                TextChannel = channel.Id,
                WebhookId = webhook.Id,
                WebhookToken = webhook.Token
            };
        }

        public async Task<RestWebhook> GetWebhookAsync(SocketGuild guild, WebhookOptions options)
        {
            return (await guild?.GetWebhooksAsync())?.FirstOrDefault(x =>
                x?.Name == options.Name || x?.Id == options.Webhook.WebhookId);
        }

        public async Task<RestWebhook> GetWebhookAsync(SocketTextChannel channel, WebhookOptions options)
        {
            return (await channel?.GetWebhooksAsync())?.FirstOrDefault(x =>
                x?.Name == options.Name || x?.Id == options.Webhook.WebhookId);
        }

        public Task WebhookFallbackAsync(DiscordWebhookClient client, ITextChannel channel, WebhookOptions options)
        {
            if (client == null && channel != null)
            {
                PrettyConsole.Log(LogSeverity.Error, "WebhookFallback", $"Falling back to Channel: {channel.Name}");
                return channel.SendMessageAsync(options.Message, embed: options.Embed);
            }

            return client.SendMessageAsync(options.Message,
                embeds: options.Embed == null ? null : new List<Embed> {options.Embed});
        }

        public async Task<WebhookWrapper> UpdateWebhookAsync(SocketTextChannel channel, WebhookWrapper old,
            WebhookOptions options)
        {
            var hook = !(_client.GetChannel(old.TextChannel) is SocketTextChannel getChannel)
                ? await GetWebhookAsync(channel.Guild, new WebhookOptions {Webhook = old})
                : await GetWebhookAsync(getChannel, new WebhookOptions {Webhook = old});
            if (channel.Id == old.TextChannel && hook != null) return old;
            if (hook != null) await hook.DeleteAsync();
            var New = await channel.CreateWebhookAsync(options.Name, AvatarStream());
            return new WebhookWrapper
            {
                TextChannel = channel.Id,
                WebhookId = New.Id,
                WebhookToken = New.Token
            };
        }
    }
}