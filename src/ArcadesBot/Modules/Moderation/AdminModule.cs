using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace ArcadesBot
{
    [Name("Admin"), RequirePermission(AccessLevel.Administrator), RequireBotPermission(ChannelPermission.SendMessages)]
    [Summary("Several Admin commands")]
    public class AdminModule : Base
    {
        [Command("Settings"), Summary("Displays current server's settings.")]
        public Task SettingsAsync()
        {
            var embed = new EmbedBuilder()
               .WithAuthor($"{Context.Guild} Settings", Context.Guild.IconUrl)
               .AddField("General Information",
                $"```ebnf\n" +
                $"Prefix                : {Context.Server.Prefix}\n" +
                $"Join Channel          : #{StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.JoinWebhook.TextChannel)}\n" +
                $"Leave Channel         : #{StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.LeaveWebhook.TextChannel)}\n" +
                $"Join Messages         : {Context.Server.JoinMessages.Count}\n" +
                $"Leave Messages        : {Context.Server.LeaveMessages.Count}\n" +
                $"AFK Users             : {Context.Server.Afk.Count}\n" +
                $"Self Assignable Roles : {Context.Server.AssignableRoles.Count}\n" +
                $"```", false)
                .AddField("Admin Information",
                $"```diff\n" +
                $"+ Join Role           : @{StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.JoinRole)}\n" +
                $"+ Mute Role           : @{StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.MuteRole)}\n" +
                $"+ Blacklisted Users   : {Context.Server.Profiles.Where(x => x.Value.IsBlacklisted).Count()}\n" +
                $"+ Blacklisted Channels: {Context.Server.BlackListedChannels.Count}\n" +
                $"```", false)
               .Build();
            return ReplyAsync(string.Empty, embed);
        }

        [Command("Blacklist"), Summary("Blacklist the current channel or mentioned channel if specified")]
        public async Task ToggleBlackListAsync(SocketTextChannel channel = null)
        {
            channel = channel ?? Context.Channel as SocketTextChannel;
            if (Context.GuildHelper.ToggleBlackList(Context.Server, channel.Id))
                await ReplyAsync($"{channel.Mention} was added to the blacklist", document: DocumentType.Server);
            else
                await ReplyAsync($"{channel.Mention} was removed from the blacklist" , document: DocumentType.Server);
        }

        [Command("Setup"), Summary("Set ups the bot for your server.")]
        public async Task SetupAsync()
        {
            if (Context.Server.IsConfigured == true)
            {
                await ReplyAsync($"{Context.Guild} has already been configured.");
                return;
            }
            var setupMessage = await ReplyAsync($"Initializing *{Context.Guild}'s* config .... ");
            var defaultChannel = Context.GuildHelper.DefaultChannel(Context.Guild.Id) as SocketTextChannel;
            var defaultWebhook = await Context.WebhookService.CreateWebhookAsync(defaultChannel, Context.Client.CurrentUser.Username);
            Context.Server.JoinWebhook = defaultWebhook;
            Context.Server.LeaveWebhook = defaultWebhook;
            Context.Server.JoinMessages.Add("**{user}** in da houuuuuuseeeee! Turn up!");
            Context.Server.JoinMessages.Add("Whalecum to **{guild}**, **{user}**! Make yourself comfy wink wink.");
            Context.Server.LeaveMessages.Add("**{user}** abandoned us ... Fake frens :((");
            Context.Server.LeaveMessages.Add("Oh man, we lost **{user}**... Press F to pay respects.");
            Context.Server.IsConfigured = true;
            await ReplyAsync($"Configuration for {Context.Guild} is finished.", document: DocumentType.Server);
        }

        [Command("Set"), Summary("Sets certain values for current server's config.")]
        public Task SetAsync([Summary("Valid setting types are: \n - Prefix\n - JoinChannel\n - JoinRole\n - MuteRole\n - LeaveChannel")]SettingType settingType, [Remainder, Summary("The value of the setting you are changing")] string value = null)
        {
            value = value ?? string.Empty;
            var channelCheck = Context.GuildHelper.GetChannelId(Context.Guild, value);
            var roleCheck = Context.GuildHelper.GetRoleId(Context.Guild, value);
            var getChannel = Context.Guild.GetTextChannel(channelCheck.Item2);
            switch (settingType)
            {
                case SettingType.Prefix:
                    Context.Server.Prefix = value;
                    break;
                case SettingType.JoinChannel:
                    if (channelCheck.Item1 == false)
                        return ReplyAsync($" ❌ {settingType} value was provided in incorrect format. try mentioning the channel?");
                    Context.Server.JoinWebhook =
                        Context.WebhookService.UpdateWebhookAsync(getChannel, Context.Server.JoinWebhook, new WebhookOptions
                        {
                            Name = Context.Client.CurrentUser.Username
                        }).Result;
                    break;
                case SettingType.JoinRole:
                    if (roleCheck.Item1 == false)
                        return ReplyAsync($" ❌ {settingType} value was provided in incorrect format. try mentioning the role?");
                    Context.Server.Mod.JoinRole = roleCheck.Item2;
                    break;
                case SettingType.MuteRole:
                    if (roleCheck.Item1 == false) return ReplyAsync($" ❌ {settingType} value was provided in incorrect format. try mentioning the role?");
                    Context.Server.Mod.MuteRole = roleCheck.Item2; break;
                case SettingType.LeaveChannel:
                    if (channelCheck.Item1 == false)
                        return ReplyAsync($" ❌ {settingType} value was provided in incorrect format. try mentioning the channel?");
                    Context.Server.LeaveWebhook =
                        Context.WebhookService.UpdateWebhookAsync(getChannel, Context.Server.LeaveWebhook, new WebhookOptions
                        {
                            Name = Context.Client.CurrentUser.Username
                        }).Result;
                    break;
            }
            return ReplyAsync($"{settingType} has been updated 👍", document: DocumentType.Server);
        }


        [Command("Export"), Summary("Exports your server config as a json file.")]
        public async Task ExportAsync()
        {
            var owner = await Context.Guild.Owner.GetOrCreateDMChannelAsync();
            if (Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync("Requires Server Owner.");
                return;
            }
            var serialize = JsonConvert.SerializeObject(Context.Server, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            });
            await owner.SendFileAsync(new MemoryStream(Encoding.Unicode.GetBytes(serialize)), $"{Context.Guild.Id}-Config.json");
        }

        [Command("Reset"), Summary("Resets your server config.")]
        public Task ResetAsync()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                return ReplyAsync("Requires Server's Owner.");

            var properties = Context.Server.GetType().GetProperties();
            foreach (var property in properties.Where(x => x.Name != "Id" && x.Name != "Prefix"))
            {
                if (property.PropertyType == typeof(bool))
                    property.SetValue(Context.Server, false);
                if (property.PropertyType == typeof(List<string>))
                    property.SetValue(Context.Server, new List<string>());
                if (property.PropertyType == typeof(List<ulong>))
                    property.SetValue(Context.Server, new List<ulong>());
                if (property.PropertyType == typeof(Dictionary<ulong, string>))
                    property.SetValue(Context.Server, new Dictionary<ulong, string>());
                if (property.PropertyType == typeof(WebhookWrapper))
                    property.SetValue(Context.Server, new WebhookWrapper());
                if (property.PropertyType == typeof(List<MessageWrapper>))
                    property.SetValue(Context.Server, new List<MessageWrapper>());
                if (property.PropertyType == typeof(Dictionary<ulong, UserProfile>))
                    property.SetValue(Context.Server, new Dictionary<ulong, UserProfile>());
            }
            return ReplyAsync($"Guild Config has been recreated 👍", document: DocumentType.Server);
        }

        [Command("SelfRoles")]
        [Summary("Adds/Removes role to/from self assingable roles.")]
        public Task SelfRoleAsync([Summary("Valid actions are:\n - add\n - remove")]string action, [Remainder, Summary("The role you want to add or remove")]IRole role)
        {
            if (role == Context.Guild.EveryoneRole)
                return ReplyAsync("Role can't be everyone role.");
            var check = Context.GuildHelper.ListCheck(Context.Server.AssignableRoles, role.Id, role.Name, "assignable roles");
            switch (action.ToLower())
            {
                case "a":
                case "add":
                    if (!check.Item1)
                        return ReplyAsync(check.Item2);
                    Context.Server.AssignableRoles.Add(role.Id);
                    return ReplyAsync(check.Item2, document: DocumentType.Server);
                case "remove":
                case "rem":
                case "r":
                    if (!Context.Server.AssignableRoles.Contains(role.Id))
                        return ReplyAsync($"{role.Name} isn't an assignable role !");
                    Context.Server.AssignableRoles.Remove(role.Id);
                    return ReplyAsync($"`{role.Name}` is no longer an assignable role.", document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("JoinMessages"), Summary("Add/Removes join message.\n{user} to mention user.\n{guild} to print server name.")]
        public Task JoinMessagesAsync([Summary("Valid actions are:\n - add\n - remove")]string action, [Remainder, Summary("The message you want to add or remove")] string message)
        {
            var check = Context.GuildHelper.ListCheck(Context.Server.JoinMessages, message, $"```{message}```", "join messages");
            switch (action.ToLower())
            {
                case "a":
                case "add":
                    if (!check.Item1)
                        return ReplyAsync(check.Item2);
                    Context.Server.JoinMessages.Add(message);
                    return ReplyAsync("Join message has been added.", document: DocumentType.Server);
                case "remove":
                case "rem":
                case "r":
                    if (!Context.Server.JoinMessages.Contains(message)) return ReplyAsync("I couldn't find the specified join message.");
                    Context.Server.JoinMessages.Remove(message);
                    return ReplyAsync("Join message has been removed.", document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("LeaveMessages"), Summary("Add/Removes leave message. {user} to mention user. {guild} to print server name.")]
        public Task LeaveMessagesAsync([Summary("Valid actions are:\n - add\n - remove")]string action, [Remainder, Summary("The message you want to add or remove")] string message)
        {
            var check = Context.GuildHelper.ListCheck(Context.Server.LeaveMessages, message, $"```{message}```", "leave messages");
            switch (action)
            {
                case "a":
                case "add":
                    if (!check.Item1)
                        return ReplyAsync(check.Item2);
                    Context.Server.LeaveMessages.Add(message);
                    return ReplyAsync("Leave message has been added.", document: DocumentType.Server);
                case "remove":
                case "rem":
                case "r":
                    if (!Context.Server.LeaveMessages.Contains(message))
                        return ReplyAsync("I couldn't find the specified leave message.");
                    Context.Server.LeaveMessages.Remove(message);
                    return ReplyAsync("Leave message has been removed.", document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("JoinMessages"), Summary("Shows all the join messages for this server.")]
        public Task JoinMessagesAsync()
            => ReplyAsync(!Context.Server.JoinMessages.Any() ? $"{Context.Server} doesn't have any user join messages!" :
                $"**Join Messages**\n{string.Join("\n", $"-> {Context.Server.JoinMessages}")}");

        [Command("LeaveMessages"), Summary("Shows all the join messages for this server.")]
        public Task LeaveMessagesAsync()
            => ReplyAsync(!Context.Server.JoinMessages.Any() ? $"{Context.Server} doesn't have any user leave messages! " :
                $"**Leave Messages**\n{string.Join("\n", $"-> {Context.Server.LeaveMessages}")}");

        public enum SettingType
        {
            Prefix,
            JoinRole,
            JoinChannel,
            LeaveChannel,
            MuteRole
        }
    }
}
