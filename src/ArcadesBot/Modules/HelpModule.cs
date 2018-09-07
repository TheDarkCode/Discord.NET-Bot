using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcadesBot.CommandExtensions.Attribute;
using ArcadesBot.Common;
using ArcadesBot.Utility;

namespace ArcadesBot.Modules
{
    public class HelpModule : Base
    {
        private CommandService _commands { get; }
        private IServiceProvider _provider { get; }

        public HelpModule(IServiceProvider provider)
        {
            _commands = provider.GetService<CommandService>();
            _provider = provider;
        }

        [Command("?"), Alias("help")]
        public async Task HelpAsync()
        {
            var prefix = GetPrefix(Context);
            var modules = _commands.Modules.Where(x => !string.IsNullOrWhiteSpace(x.Summary));

            var embed = new EmbedBuilder()
                .WithInfoColor()
                .WithFooter(x => x.Text = $"Type `{prefix}? <module>` for more information");

            foreach (var module in modules)
            {
                var success = false;
                foreach (var command in module.Commands)
                {
                    var result = await command.CheckPreconditionsAsync(Context, _provider);
                    if (!result.IsSuccess)
                        continue;
                    success = true;
                    break;
                }

                if (!success)
                    continue;

                embed.AddField(module.Name, module.Summary);
            }

            await ReplyEmbedAsync(embed: embed);
        }

        [Command("?"), Alias("help"), Priority(10)]
        public async Task HelpAsync([Remainder]string moduleName)
        {
            var prefix = GetPrefix(Context);
            var module = _commands.Modules.FirstOrDefault(x => string.Equals(x.Name, moduleName, StringComparison.CurrentCultureIgnoreCase));

            if (module == null)
            {
                try
                {
                    if (moduleName.Substring(0, prefix.Length) == prefix)
                    {
                        var commandName = moduleName.Substring(prefix.Length);
                            
                        var command = _commands.Commands.FirstOrDefault(x => x.Aliases.Any(z => string.Equals(z, commandName, StringComparison.CurrentCultureIgnoreCase)));
                        await HelpAsync(command);
                        return;
                    }
                }
                catch (ArgumentOutOfRangeException) { }
                await ReplyEmbedAsync($"The module `{moduleName}` does not exist.");
                return;
            }


            var commands = module.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary))
                                 .GroupBy(x => x.Name)
                                 .Select(x => x.First()).ToList();

            if (!commands.Any())
            {
                await ReplyEmbedAsync($"The module `{module.Name}` has no available commands :(");
                return;
            }

            var embed = new EmbedBuilder()
                .WithInfoColor()
                .WithFooter(x => x.Text = $"Type `{prefix}help {prefix}<command>` for more information");

            foreach (var command in commands)
            {
                var result = await command.CheckPreconditionsAsync(Context, _provider);
                if (result.IsSuccess)
                    embed.AddField(prefix + command.Aliases.First(), command.Summary);
            }

            await ReplyEmbedAsync(embed: embed);
        }

        private async Task HelpAsync(CommandInfo commandInfo)
        {
            if (commandInfo == null)
            {
                await ReplyEmbedAsync($"The command `{commandInfo.Name}` is not a command that exists.");
                return;
            }

            var usageAttribute = commandInfo.Attributes.FirstOrDefault(x => x is UsageAttribute) as UsageAttribute;
            var prefix = GetPrefix(Context) ?? $"@{Context.Client.CurrentUser.Username} ";
            var embed = new EmbedBuilder().WithInfoColor();
            var result = await commandInfo.CheckPreconditionsAsync(Context, _provider);

            if (result.IsSuccess)
            {
                var sbuilder = new StringBuilder()
                    .Append(prefix + commandInfo.Aliases.First());
                var fields = new List<EmbedFieldBuilder>();
                foreach (var parameter in commandInfo.Parameters)
                {
                    var p = parameter.Name;
                    p = p.FirstCharToUpper();
                    if (parameter.Summary != null)
                        fields.Add(new EmbedFieldBuilder().WithName(p).WithValue(parameter.Summary));
                    if (parameter.IsRemainder)
                        p += "...";
                    p = parameter.IsOptional ? $"[{p}]" : $"<{p}>";
                    sbuilder.Append(" " + p);
                }

                embed.AddField(sbuilder.ToString(), commandInfo.Remarks ?? commandInfo.Summary);
                foreach (var field in fields)
                    embed.AddField(field);

                if (usageAttribute != null)
                    embed.AddField("Usage", $"`{prefix}{usageAttribute.Text}`");
            }

            embed.WithFooter(x => x.Text = $"Aliases: {string.Join(", ", commandInfo.Aliases)}");

            await ReplyEmbedAsync(embed: embed);
        }
        private static string GetPrefix(CustomCommandContext context)
            => context.Server.Prefix;
    }
}
