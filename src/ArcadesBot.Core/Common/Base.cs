using System;
using System.IO;
using System.Threading.Tasks;
using ArcadesBot.Models;
using ArcadesBot.Utility;
using Discord;
using Discord.Commands;

namespace ArcadesBot.Common
{
    public class Base : ModuleBase<CustomCommandContext>
    {
        public enum DocumentType
        {
            None = 0,
            Config,
            Server
        }

        public async Task<IUserMessage> ReplyEmbedAsync(string description = "", EmbedBuilder embed = null,
            DocumentType document = DocumentType.None)
        {
            if (embed == null)
                embed = new EmbedBuilder().WithSuccessColor().WithDescription(description);
            return await ReplyAsync("", embed, document);
        }

        public async Task<IUserMessage> ReplyAsync(string message, EmbedBuilder embed = null,
            DocumentType document = DocumentType.None)
        {
            await Context.Channel.TriggerTypingAsync();
            await Task.Run(() => SaveDocuments(document));
            if(embed == null)
                return await base.ReplyAsync(message);

            return await base.ReplyAsync(message, false, embed.Build());
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<IUserMessage> SendFileAsync(Stream stream, string fileName, string text = "",
            EmbedBuilder embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await Context.Channel.SendFileAsync(stream, fileName, text, false, embed.Build());
        }

        public async Task<IUserMessage> SendFileAsync(string fileName, string text = "", EmbedBuilder embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await Context.Channel.SendFileAsync(fileName, text, false, embed.Build());
        }

        private void SaveDocuments(DocumentType document)
        {
            switch (document)
            {
                case DocumentType.None:
                    break;
                case DocumentType.Config:
                    Context.Database.Update<ConfigModel>("Config", Context.Config);
                    break;
                case DocumentType.Server:
                    Context.Database.Update<GuildModel>(Context.Guild.Id, Context.Server);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(document), document, null);
            }
        }
    }
}