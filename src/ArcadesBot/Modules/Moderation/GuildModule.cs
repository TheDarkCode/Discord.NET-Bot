using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace ArcadesBot.Modules
{
    [Name("Config")]
    [Summary("Bot configuration options")]
    public class GuildModule : ModuleBase<CustomCommandContext>
    {
        private readonly ConfigDatabase _db;
        
        public GuildModule(IServiceProvider provider)
        {
            _db = provider.GetService<ConfigDatabase>();
        }

        [RequireContext(ContextType.Guild)]
        [Command("prefix")]
        [Summary("Check what prefix this guild has configured.")]
        public async Task PrefixAsync()
        {
            var embedText = "";
            var config = await _db.GetConfigAsync(Context.Guild.Id);

            if (config.Prefix == null)
                embedText = "This guild has no prefix";
            else
                embedText = $"This guild's prefix is `{config.Prefix}`";
            var embed = new EmbedBuilder().WithDescription(embedText).WithColor(EmbedColors.GetSuccessColor());
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [IsAdmin]
        [Command("setprefix")]
        [Summary("Change or remove this guild's string prefix.")]
        public async Task SetPrefixAsync([Remainder]string prefix)
        {
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            await _db.SetPrefixAsync(config, prefix);
            var embed = new EmbedBuilder().WithDescription($"This guild's prefix is now `{prefix}`").WithColor(EmbedColors.GetSuccessColor());
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [IsAdmin]
        [Command("blockchannel")]
        [Summary("Change or remove this guild's string prefix.")]
        public async Task BlockChannel(SocketTextChannel channel = null)
        {
            if (channel == null) channel = Context.Channel as SocketTextChannel;
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            await _db.BlockChannel(config, channel.Id);

            var embed = new EmbedBuilder().WithDescription($"Bot commands are now blocked in {channel.Mention}").WithColor(EmbedColors.GetSuccessColor());
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [IsAdmin]
        [Command("allowchannel")]
        [Summary("Change or remove this guild's string prefix.")]
        public async Task AllowChannel(SocketTextChannel channel = null)
        {
            if (channel == null) channel = Context.Channel as SocketTextChannel;
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            await _db.AllowChannel(config, channel.Id);

            var embed = new EmbedBuilder().WithDescription($"You can execute bot commands in {channel.Mention} again").WithColor(EmbedColors.GetSuccessColor());
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [IsAdmin]
        [Command("addmodrole")]
        [Summary("Adds Bot moderator to specified role.")]
        public async Task AddModRole(SocketRole role)
        {
            EmbedBuilder embed;
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            if (!config.ModRolesList.Contains(role.Id))
            {
                await _db.AddToModList(config, role.Id);
                embed = new EmbedBuilder().WithDescription($"{role.Mention} is now now a Bot moderator")
                    .WithColor(EmbedColors.GetSuccessColor());
            }
            else
            {
                embed = new EmbedBuilder().WithDescription($"{role.Mention} was already a Bot moderator")
                    .WithColor(EmbedColors.GetErrorColor());
            }
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [IsAdmin]
        [Command("removemodrole")]
        [Summary("Removes Bot moderator from specified role.")]
        public async Task RemoveModRole(SocketRole role)
        {
            EmbedBuilder embed;
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            await _db.RemoveFromModList(config, role.Id);
            if (config.ModRolesList.Contains(role.Id))
            {
                await _db.RemoveFromModList(config, role.Id);
                embed = new EmbedBuilder().WithDescription($"{role.Mention} is no longer a Bot moderator")
                    .WithColor(EmbedColors.GetSuccessColor());
            }
            else
            {
                embed = new EmbedBuilder().WithDescription($"{role.Mention} is not a Bot moderator")
                    .WithColor(EmbedColors.GetErrorColor());
            }
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        [Command("addadminrole")]
        [Summary("Adds Bot admin to specified role.")]
        public async Task AddAdminRole(SocketRole role)
        {
            EmbedBuilder embed;
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            if (!config.AdminRoleList.Contains(role.Id))
            {
                await _db.AddToAdminList(config, role.Id);
                embed = new EmbedBuilder().WithDescription($"{role.Mention} is no longer a Bot admin")
                    .WithColor(EmbedColors.GetSuccessColor());
            }
            else
            {
                embed = new EmbedBuilder().WithDescription($"{role.Mention} is already a Bot admin")
                    .WithColor(EmbedColors.GetErrorColor());
            }
                
            
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        [Command("removeadminrole")]
        [Summary("Removes Bot admin from specified role.")]
        public async Task RemoveAdminRole(SocketRole role)
        {
            EmbedBuilder embed;
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            await _db.RemoveFromAdminList(config, role.Id);
            if (config.ModRolesList.Contains(role.Id))
            {
                await _db.RemoveFromAdminList(config, role.Id);
                embed = new EmbedBuilder().WithDescription($"{role.Mention} is no longer a Bot admin")
                    .WithColor(EmbedColors.GetSuccessColor());
            }
            else
            {
                embed = new EmbedBuilder().WithDescription($"{role.Mention} is not a Bot admin")
                    .WithColor(EmbedColors.GetErrorColor());
            }
            await ReplyAsync("", embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [IsMod]
        [Command("listadmim")]
        [Alias("lsa", "lsadmin")]
        [Summary("Lists all bot admin roles.")]
        public async Task ListAllAdminRoles()
        {
            EmbedBuilder embed;
            var config = await _db.GetConfigAsync(Context.Guild.Id);
            
            if (config.AdminRoleList.Count != 0)
            {
                embed = new EmbedBuilder().WithDescription($"")
                    .WithColor(EmbedColors.GetSuccessColor());
            }
            else
            {
                embed = new EmbedBuilder().WithDescription($"There are no Bot admin roles")
                    .WithColor(EmbedColors.GetErrorColor());
            }
            await ReplyAsync("", embed: embed.Build());
        }
    }
}
