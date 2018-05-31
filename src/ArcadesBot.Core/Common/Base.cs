using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class Base : ModuleBase<CustomCommandContext>
    {
        public async Task<IUserMessage> ReplyAsync(string Message, Embed Embed = null, DocumentType Document = DocumentType.None)
        {
            await Context.Channel.TriggerTypingAsync();
            _ = Task.Run(() => SaveDocuments(Document));
            return await base.ReplyAsync(Message, false, Embed, null);
        }
        private void SaveDocuments(DocumentType Document)
        {
            bool Check = false;
            switch (Document)
            {
                case DocumentType.None:
                    Check = true;
                    break;
                case DocumentType.Config:
                    Context.ConfigHandler.Save(Context.Config);
                    Check = !Context.Session.Advanced.HasChanges;
                    break;
                case DocumentType.Server:
                    Context.GuildHandler.Update(Context.Server);
                    Check = !Context.Session.Advanced.HasChanges;
                    break;
            }
            if (Check == false)
                PrettyConsole.Log(LogSeverity.Warning, "Database", $"Failed to save {Document} document.");
        }
        public enum DocumentType
        {
            None = 0,
            Config,
            Server
        }
    }
}
