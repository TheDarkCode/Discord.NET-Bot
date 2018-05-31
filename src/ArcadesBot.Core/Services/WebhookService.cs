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
        HttpClient _httpclient { get; }
        GuildHandler _guildhandler { get; }
        DiscordSocketClient _client { get; }
        private FileStream AvatarStream()
        {
            if (File.Exists("Avatar.jpg"))
                return new FileStream("Avatar.jpg", FileMode.Open, FileAccess.Read);
            else
                return new FileStream(
                    StringHelper.DownloadImageAsync(_httpclient, _client.CurrentUser.GetAvatarUrl()).GetAwaiter().GetResult(),
                    FileMode.Open, FileAccess.Read);
        }

        public WebhookService(HttpClient httpClient, GuildHandler guild, DiscordSocketClient client)
        {
            _guildhandler = guild;
            _client = client;
            _httpclient = httpClient;
        }

        public DiscordWebhookClient WebhookClient(ulong Id, string Token)
        {
            try
            {
                return new DiscordWebhookClient(Id, Token);
            }
            catch
            {
                PrettyConsole.Log(LogSeverity.Error, "Webhook", $"Webhook {Id} Failed.");
                return null;
            }
        }

        public async Task SendMessageAsync(WebhookOptions Options)
        {
            if (!(_client.GetChannel(Options.Webhook.TextChannel) is SocketTextChannel Channel)) return;
            var Client = WebhookClient(Options.Webhook.WebhookId, Options.Webhook.WebhookToken);
            await WebhookFallbackAsync(Client, Channel, Options);
        }

        public async Task<WebhookWrapper> CreateWebhookAsync(SocketTextChannel Channel, string Name)
        {
            var Get = await GetWebhookAsync(Channel, new WebhookOptions
            {
                Name = Name
            });
            var Webhook = Get ?? await Channel.CreateWebhookAsync(Name, AvatarStream());
            return new WebhookWrapper
            {
                TextChannel = Channel.Id,
                WebhookId = Webhook.Id,
                WebhookToken = Webhook.Token
            };
        }

        public async Task<RestWebhook> GetWebhookAsync(SocketGuild Guild, WebhookOptions Options)
            => (await Guild?.GetWebhooksAsync())?.FirstOrDefault(x => x?.Name == Options.Name || x?.Id == Options.Webhook.WebhookId);

        public async Task<RestWebhook> GetWebhookAsync(SocketTextChannel Channel, WebhookOptions Options)
            => (await Channel?.GetWebhooksAsync())?.FirstOrDefault(x => x?.Name == Options.Name || x?.Id == Options.Webhook.WebhookId);

        public Task WebhookFallbackAsync(DiscordWebhookClient Client, ITextChannel Channel, WebhookOptions Options)
        {
            if (Client == null && Channel != null)
            {
                PrettyConsole.Log(LogSeverity.Error, "WebhookFallback", $"Falling back to Channel: {Channel.Name}");
                return Channel.SendMessageAsync(Options.Message, embed: Options.Embed);
            }
            return Client.SendMessageAsync(Options.Message, embeds: Options.Embed == null ? null : new List<Embed>() { Options.Embed });
        }

        public async Task<WebhookWrapper> UpdateWebhookAsync(SocketTextChannel Channel, WebhookWrapper Old, WebhookOptions Options)
        {
            var Hook = !(_client.GetChannel(Old.TextChannel) is SocketTextChannel GetChannel) ?
                await GetWebhookAsync(Channel.Guild, new WebhookOptions { Webhook = Old }) :
                await GetWebhookAsync(GetChannel, new WebhookOptions { Webhook = Old });
            if (Channel.Id == Old.TextChannel && Hook != null) return Old;
            else if (Hook != null) await Hook.DeleteAsync();
            var New = await Channel.CreateWebhookAsync(Options.Name, AvatarStream());
            return new WebhookWrapper
            {
                TextChannel = Channel.Id,
                WebhookId = New.Id,
                WebhookToken = New.Token
            };
        }
    }
}