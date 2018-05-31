﻿using Discord;
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
            var Embed = new EmbedBuilder()
               .WithAuthor($"{Context.Guild} Settings", Context.Guild.IconUrl)
               .AddField("General Information",
                $"```ebnf\n" +
                $"Prefix                : {Context.Server.Prefix}\n" +
                $"Join Channel          : #{StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.JoinWebhook.TextChannel)}\n" +
                $"Leave Channel         : #{StringHelper.CheckChannel(Context.Guild as SocketGuild, Context.Server.LeaveWebhook.TextChannel)}\n" +
                $"Join Messages         : {Context.Server.JoinMessages.Count}\n" +
                $"Leave Messages        : {Context.Server.LeaveMessages.Count}\n" +
                $"AFK Users             : {Context.Server.AFK.Count}\n" +
                $"Self Assignable Roles : {Context.Server.AssignableRoles.Count}\n" +
                $"```", false)
                .AddField("Admin Information",
                $"```diff\n" +
                $"+ Join Role           : @{StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.JoinRole)}\n" +
                $"+ Mute Role           : @{StringHelper.CheckRole(Context.Guild as SocketGuild, Context.Server.Mod.MuteRole)}\n" +
                $"+ Blacklisted Users   : {Context.Server.Profiles.Where(x => x.Value.IsBlacklisted).Count()}\n" +
                $"```", false)
               .Build();
            return ReplyAsync(string.Empty, Embed);
        }
        [Command("Setup"), Summary("Set ups the bot for your server.")]
        public async Task SetupAsync()
        {
            if (Context.Server.IsConfigured == true)
            {
                await ReplyAsync($"{Context.Guild} has already been configured.");
                return;
            }
            var SetupMessage = await ReplyAsync($"Initializing *{Context.Guild}'s* config .... ");
            OverwritePermissions Permissions = new OverwritePermissions(sendMessages: PermValue.Deny);
            OverwritePermissions VPermissions = new OverwritePermissions(sendMessages: PermValue.Allow);
            var DefaultChannel = Context.GuildHelper.DefaultChannel(Context.Guild.Id) as SocketTextChannel;
            var DefaultWebhook = await Context.WebhookService.CreateWebhookAsync(DefaultChannel, Context.Client.CurrentUser.Username);
            Context.Server.JoinWebhook = DefaultWebhook;
            Context.Server.LeaveWebhook = DefaultWebhook;
            Context.Server.JoinMessages.Add("**{user}** in da houuuuuuseeeee! Turn up!");
            Context.Server.JoinMessages.Add("Whalecum to **{guild}**, **{user}**! Make yourself comfy wink wink.");
            Context.Server.LeaveMessages.Add("**{user}** abandoned us ... Fake frens :((");
            Context.Server.LeaveMessages.Add("Oh man, we lost **{user}**... Press F to pay respects.");
            Context.Server.IsConfigured = true;
            await ReplyAsync($"Configuration for {Context.Guild} is finished.", Document: DocumentType.Server);
        }

        [Command("Set"), Summary("Sets certain values for current server's config.")]
        public Task SetAsync(SettingType SettingType, [Remainder] string Value = null)
        {
            Value = Value ?? string.Empty;
            var IntCheck = int.TryParse(Value, out int Result);
            var ChannelCheck = Context.GuildHelper.GetChannelId(Context.Guild as SocketGuild, Value);
            var RoleCheck = Context.GuildHelper.GetRoleId(Context.Guild as SocketGuild, Value);
            var GetChannel = (Context.Guild as SocketGuild).GetTextChannel(ChannelCheck.Item2) as SocketTextChannel;
            switch (SettingType)
            {
                case SettingType.Prefix:
                    Context.Server.Prefix = Value;
                    break;
                case SettingType.JoinChannel:
                    if (ChannelCheck.Item1 == false)
                        return ReplyAsync($" ❌ {SettingType} value was provided in incorrect format. try mentioning the channel?");
                    Context.Server.JoinWebhook =
                        Context.WebhookService.UpdateWebhookAsync(GetChannel, Context.Server.JoinWebhook, new WebhookOptions
                        {
                            Name = Context.Client.CurrentUser.Username
                        }).Result;
                    break;
                case SettingType.JoinRole:
                    if (RoleCheck.Item1 == false)
                        return ReplyAsync($" ❌ {SettingType} value was provided in incorrect format. try mentioning the role?");
                    Context.Server.Mod.JoinRole = RoleCheck.Item2;
                    break;
                case SettingType.MuteRole:
                    if (RoleCheck.Item1 == false) return ReplyAsync($" ❌ {SettingType} value was provided in incorrect format. try mentioning the role?");
                    Context.Server.Mod.MuteRole = RoleCheck.Item2; break;
                case SettingType.LeaveChannel:
                    if (ChannelCheck.Item1 == false)
                        return ReplyAsync($" ❌ {SettingType} value was provided in incorrect format. try mentioning the channel?");
                    Context.Server.LeaveWebhook =
                        Context.WebhookService.UpdateWebhookAsync(GetChannel, Context.Server.LeaveWebhook, new WebhookOptions
                        {
                            Name = Context.Client.CurrentUser.Username
                        }).Result;
                    break;
            }
            return ReplyAsync($"{SettingType} has been updated 👍", Document: DocumentType.Server);
        }


        [Command("Export"), Summary("Exports your server config as a json file.")]
        public async Task ExportAsync()
        {
            var Owner = await (Context.Guild as SocketGuild).Owner.GetOrCreateDMChannelAsync();
            if (Context.Guild.OwnerId != Context.User.Id)
            {
                await ReplyAsync($"Requires Server's Owner.");
                return;
            }
            var Serialize = JsonConvert.SerializeObject(Context.Server, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            });
            await Owner.SendFileAsync(new MemoryStream(Encoding.Unicode.GetBytes(Serialize)), $"{Context.Guild.Id}-Config.json");
        }

        [Command("Reset"), Summary("Resets your server config.")]
        public Task ResetAsync()
        {
            if (Context.Guild.OwnerId != Context.User.Id) return ReplyAsync($"Requires Server's Owner.");
            var Properties = Context.Server.GetType().GetProperties();
            foreach (var Property in Properties.Where(x => x.Name != "Id" && x.Name != "Prefix"))
            {
                if (Property.PropertyType == typeof(bool)) Property.SetValue(Context.Server, false);
                if (Property.PropertyType == typeof(List<string>)) Property.SetValue(Context.Server, new List<string>());
                if (Property.PropertyType == typeof(List<ulong>)) Property.SetValue(Context.Server, new List<ulong>());
                if (Property.PropertyType == typeof(Dictionary<ulong, string>)) Property.SetValue(Context.Server, new Dictionary<ulong, string>());
                if (Property.PropertyType == typeof(WebhookWrapper)) Property.SetValue(Context.Server, new WebhookWrapper());
                if (Property.PropertyType == typeof(List<MessageWrapper>)) Property.SetValue(Context.Server, new List<MessageWrapper>());
                if (Property.PropertyType == typeof(Dictionary<ulong, UserProfile>)) Property.SetValue(Context.Server, new Dictionary<ulong, UserProfile>());
            }
            return ReplyAsync($"Guild Config has been recreated 👍", Document: DocumentType.Server);
        }

        [Command("SelfRoles"), Summary("Adds/Removes role to/from self assingable roles.")]
        public Task SelfRoleAsync(string Action, IRole Role)
        {
            if (Role == Context.Guild.EveryoneRole) return ReplyAsync($"Role can't be everyone role.");
            var Check = Context.GuildHelper.ListCheck(Context.Server.AssignableRoles, Role.Id, Role.Name, "assignable roles");
            switch (Action.ToLower())
            {
                case "a":
                case "add":
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.AssignableRoles.Add(Role.Id);
                    return ReplyAsync(Check.Item2, Document: DocumentType.Server);
                case "remove":
                case "rem":
                case "r":
                    if (!Context.Server.AssignableRoles.Contains(Role.Id)) return ReplyAsync($"{Role.Name} isn't an assignable role !");
                    Context.Server.AssignableRoles.Remove(Role.Id);
                    return ReplyAsync($"`{Role.Name}` is no longer an assignable role.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("JoinMessages"), Summary("Add/Removes join message. {user} to mention user. {guild} to print server name.")]
        public Task JoinMessagesAsync(string Action, [Remainder] string Message)
        {
            var Check = Context.GuildHelper.ListCheck(Context.Server.JoinMessages, Message, $"```{Message}```", "join messages");
            switch (Action.ToLower())
            {
                case "a":
                case "add":
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.JoinMessages.Add(Message);
                    return ReplyAsync("Join message has been added.", Document: DocumentType.Server);
                case "remove":
                case "rem":
                case "r":
                    if (!Context.Server.JoinMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified join message.");
                    Context.Server.JoinMessages.Remove(Message);
                    return ReplyAsync("Join message has been removed.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("LeaveMessages"), Summary("Add/Removes leave message. {user} to mention user. {guild} to print server name.")]
        public Task LeaveMessagesAsync(string Action, [Remainder] string Message)
        {
            var Check = Context.GuildHelper.ListCheck(Context.Server.LeaveMessages, Message, $"```{Message}```", "leave messages");
            switch (Action)
            {
                case "a":
                case "add":
                    if (!Check.Item1) return ReplyAsync(Check.Item2);
                    Context.Server.LeaveMessages.Add(Message);
                    return ReplyAsync("Leave message has been added.", Document: DocumentType.Server);
                case "remove":
                case "rem":
                case "r":
                    if (!Context.Server.LeaveMessages.Contains(Message)) return ReplyAsync("I couldn't find the specified leave message.");
                    Context.Server.LeaveMessages.Remove(Message);
                    return ReplyAsync("Leave message has been removed.", Document: DocumentType.Server);
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

        [Command("MessageLog"), Summary("Retrives messages from deleted messages.")]
        public Task MessageLogAsync(int Recent = 0)
        {
            if (!Context.Server.DeletedMessages.Any() || Context.Server.DeletedMessages.Count < Recent)
                return ReplyAsync("Failed to retrive deleted messages.");
            var Get = Recent == 0 ? Context.Server.DeletedMessages.LastOrDefault() : Context.Server.DeletedMessages[Recent];
            var User = StringHelper.CheckUser(Context.Client, Get.AuthorId);
            var GetUser = (Context.Client as DiscordSocketClient).GetUser(Get.AuthorId);
            var Embed = new EmbedBuilder()
                .WithAuthor($"{User} - {Get.DateTime}", GetUser != null ? GetUser.GetAvatarUrl() : Context.Client.CurrentUser.GetAvatarUrl())
                .WithDescription(Get.Content)
                .WithFooter($"Channel: {StringHelper.CheckChannel(Context.Guild as SocketGuild, Get.ChannelId)} | Message Id: {Get.MessageId}");
            return ReplyAsync(string.Empty, Embed.Build());
        }

        [Command("MessageLog"), Summary("Retrives messages from deleted messages.")]
        public Task MessageLogAsync(SocketGuildUser User = null, int Recent = 0)
        {
            User = User ?? Context.User as SocketGuildUser;
            if (!Context.Server.DeletedMessages.Any(x => x.AuthorId == User.Id)) return ReplyAsync($"Coudln't find any deleted messages from user {User.Username}.");
            if (Context.Server.DeletedMessages.Where(x => x.AuthorId == User.Id).Count() < Recent) return ReplyAsync($"Failed to retrive message.");
            var Get = Recent == 0 ? Context.Server.DeletedMessages.Where(x => x.AuthorId == User.Id).LastOrDefault()
                : Context.Server.DeletedMessages.Where(x => x.AuthorId == User.Id).ToList()[Recent];
            var GetUser = (Context.Client as DiscordSocketClient).GetUser(Get.AuthorId);
            var Embed = new EmbedBuilder()
                .WithAuthor($"{User} - {Get.DateTime}", GetUser != null ? GetUser.GetAvatarUrl() : Context.Client.CurrentUser.GetAvatarUrl())
                .WithDescription(Get.Content)
                .WithFooter($"Channel: {StringHelper.CheckChannel(Context.Guild as SocketGuild, Get.ChannelId)} | Message Id: {Get.MessageId}");
            return ReplyAsync(string.Empty, Embed.Build());
        }
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
