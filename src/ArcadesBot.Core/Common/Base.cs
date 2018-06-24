using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class Base : ModuleBase<CustomCommandContext>
    {
        public async Task<IUserMessage> ReplyEmbedAsync(string description = "", Embed embed = null, DocumentType document = DocumentType.None)
        {
            if(embed == null)
                embed = new EmbedBuilder().WithDescription(description).Build();
            return await ReplyAsync("", embed, document);
        }
        public async Task<IUserMessage> ReplyAsync(string message, Embed embed = null, DocumentType document = DocumentType.None)
        {
            await Context.Channel.TriggerTypingAsync();
            _ = Task.Run(() => SaveDocuments(document));
            return await base.ReplyAsync(message, false, embed);
        }
        public async Task<IUserMessage> SendFileAsync(Stream stream, string fileName, string text = "", Embed embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await Context.Channel.SendFileAsync(stream, fileName, text, false, embed);
        }
        public async Task<IUserMessage> SendFileAsync(string fileName, string text = "", Embed embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await Context.Channel.SendFileAsync(fileName, text, false, embed);
        }
        private void SaveDocuments(DocumentType document)
        {
            bool check;
            switch (document)
            {
                case DocumentType.None:
                    check = true;
                    break;
                case DocumentType.Config:
                    Context.ConfigHandler.Save(Context.Config);
                    check = !Context.Session.Advanced.HasChanges;
                    break;
                case DocumentType.Server:
                    Context.GuildHandler.Update(Context.Server);
                    check = !Context.Session.Advanced.HasChanges;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(document), document, null);
            }
            if (check == false)
                PrettyConsole.Log(LogSeverity.Warning, "Database", $"Failed to save {document} document.");
        }
        public enum DocumentType
        {
            None = 0,
            Config,
            Server
        }
    }
}
