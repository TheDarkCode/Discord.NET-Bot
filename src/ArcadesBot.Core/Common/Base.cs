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
        public async Task<IUserMessage> ReplyAsync(string message, Embed embed = null, DocumentType document = DocumentType.None)
        {
            await Context.Channel.TriggerTypingAsync();
            _ = Task.Run(() => SaveDocuments(document));
            return await base.ReplyAsync(message, false, embed, null);
        }
        public async Task<IUserMessage> SendFileAsync(Stream stream, string fileName, string text = "", Embed embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await Context.Channel.SendFileAsync(stream, fileName, text, false, embed, null);
        }
        public async Task<IUserMessage> SendFileAsync(string fileName, string text = "", Embed embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await Context.Channel.SendFileAsync(fileName, text, false, embed, null);
        }
        private void SaveDocuments(DocumentType document)
        {
            var check = false;
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
