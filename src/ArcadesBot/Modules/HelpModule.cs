using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot.Modules
{
    [Group("help"), Name("Help")]
    public class HelpModule : ModuleBase<CustomCommandContext>
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        public HelpModule(IServiceProvider provider)
        {
            _commands = provider.GetService<CommandService>();
            _provider = provider;
        }

        [Command]
        public async Task HelpAsync()
        {
            string prefix = GetPrefix(Context) ?? $"@{Context.Client.CurrentUser.Username} ";
            var modules = _commands.Modules.Where(x => !string.IsNullOrWhiteSpace(x.Summary));

            var embed = new EmbedBuilder()
                .WithFooter(x => x.Text = $"Type `{prefix}help <module>` for more information");

            foreach (var module in modules)
            {
                bool success = false;
                foreach (var command in module.Commands)
                {
                    var result = await command.CheckPreconditionsAsync(Context, _provider);
                    if (result.IsSuccess)
                    {
                        success = true;
                        break;
                    }
                }

                if (!success)
                    continue;

                embed.AddField(module.Name, module.Summary);
            }

            embed.WithColor(EmbedColors.GetSuccessColor());

            await ReplyAsync("", embed: embed.Build());
        }

        [Command]
        public async Task HelpAsync(string moduleName)
        {
            string prefix = GetPrefix(Context) ?? $"@{Context.Client.CurrentUser.Username} ";
            var module = _commands.Modules.FirstOrDefault(x => x.Name.ToLower() == moduleName.ToLower());

            if (module == null)
            {
                try
                {
                    if (moduleName.Substring(0, prefix.Length) == prefix)
                    {
                        var commandName = moduleName.Substring(prefix.Length);
                        var command = _commands.Commands.FirstOrDefault(x => x.Aliases.Any(z => z.ToLower() == commandName.ToLower()));
                        await HelpAsync(command.Module.Name, moduleName.Substring(1));
                        return;
                    }
                }
                catch(ArgumentOutOfRangeException) {}
                await ReplyAsync($"The module `{moduleName}` does not exist.");
                return;
            }


            var commands = module.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary))
                                 .GroupBy(x => x.Name)
                                 .Select(x => x.First());

            if (!commands.Any())
            {
                await ReplyAsync($"The module `{module.Name}` has no available commands :(");
                return;
            }

            var embed = new EmbedBuilder()
                .WithFooter(x => x.Text = $"Type `{prefix}help {prefix}<command>` for more information");

            foreach (var command in commands)
            {
                var result = await command.CheckPreconditionsAsync(Context, _provider);
                if (result.IsSuccess)
                    embed.AddField(prefix + command.Aliases.First(), command.Summary);
            }

            embed.WithColor(EmbedColors.GetSuccessColor());

            await ReplyAsync("", embed: embed.Build());
        }

        private async Task HelpAsync(string moduleName, string commandName)
        {
            string alias = $"{commandName}".ToLower();
            string prefix = GetPrefix(Context) ?? $"@{Context.Client.CurrentUser.Username} ";
            var module = _commands.Modules.FirstOrDefault(x => x.Name.ToLower() == moduleName.ToLower());

            if (module == null)
            {
                await ReplyAsync($"The module `{moduleName}` does not exist.");
                return;
            }

            var commands = module.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary));

            if (commands.Count() == 0)
            {
                await ReplyAsync($"The module `{module.Name}` has no available commands :(");
                return;
            }

            var command = commands.Where(x => x.Aliases.Contains(alias));
            var embed = new EmbedBuilder();

            var aliases = new List<string>();
            foreach (var overload in command)
            {
                var result = await overload.CheckPreconditionsAsync(Context, _provider);
                if (result.IsSuccess)
                {
                    var sbuilder = new StringBuilder()
                        .Append(prefix + overload.Aliases.First());
                    List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
                    foreach (var parameter in overload.Parameters)
                    {
                        string p = parameter.Name;
                        p = StringHelper.FirstCharToUpper(p);
                        if (parameter.Summary != null)
                            fields.Add(new EmbedFieldBuilder().WithName(p).WithValue(parameter.Summary));
                        if (parameter.IsRemainder)
                            p += "...";
                        if (parameter.IsOptional)
                            p = $"[{p}]";
                        else
                            p = $"<{p}>";
                        sbuilder.Append(" " + p);
                    }

                    embed.AddField(sbuilder.ToString(), overload.Remarks ?? overload.Summary);
                    for (int i = 0; i < fields.Count; i++)
                        embed.AddField(fields[i]);
                }
                aliases.AddRange(overload.Aliases);
            }

            embed.WithFooter(x => x.Text = $"Aliases: {string.Join(", ", aliases)}");

            embed.WithColor(EmbedColors.GetSuccessColor());

            await ReplyAsync("", embed: embed.Build());
        }
        private string GetPrefix(CustomCommandContext context)
            => context.Server.Prefix == null ? null : context.Server.Prefix;
    }
}
