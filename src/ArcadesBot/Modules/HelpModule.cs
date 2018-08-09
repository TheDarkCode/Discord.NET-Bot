using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadesBot.Modules
{
    //[Group("help"), Summary("Help")]
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
            var prefix = GetPrefix(Context) ?? $"@{Context.Client.CurrentUser.Username} ";
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
            var prefix = GetPrefix(Context) ?? $"@{Context.Client.CurrentUser.Username} ";
            var module = _commands.Modules.FirstOrDefault(x => x.Name.ToLower() == moduleName.ToLower());

            if (module == null)
            {
                //try
                //{
                //    if (moduleName.Substring(0, prefix.Length) == prefix)
                //    {
                //        var commandName = moduleName.Substring(prefix.Length);
                //        var command = _commands.Commands.FirstOrDefault(x => x.Aliases.Any(z => string.Equals(z, commandName, StringComparison.CurrentCultureIgnoreCase)));
                //        await HelpAsync(command.Module.Name, command);
                //        return;
                //    }
                //}
                //catch (ArgumentOutOfRangeException) { }
                await ReplyEmbedAsync($"The module `{moduleName}` does not exist.");
                return;
            }


            var commands = module.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary))
                                 .GroupBy(x => x.Name)
                                 .Select(x => x.First());

            if (!commands.Any())
            {
                await ReplyEmbedAsync($"The module `{module.Name}` has no available commands :(");
                return;
            }

            var embed = new EmbedBuilder()
                .WithInfoColor();
                //.WithFooter(x => x.Text = $"Type `{prefix}help {prefix}<command>` for more information");

            foreach (var command in commands)
            {
                var result = await command.CheckPreconditionsAsync(Context, _provider);
                if (result.IsSuccess)
                    embed.AddField(prefix + command.Aliases.First(), command.Summary);
            }

            await ReplyEmbedAsync(embed: embed);
        }

        //private async Task HelpAsync(string moduleName, CommandInfo commandInfo)
        //{
        //    if(commandInfo == null)
        //    {
        //        await ReplyEmbedAsync($"The command `{commandInfo.Name}` is not a command that exists.");
        //        return;
        //    }

        //    var alias = $"{commandInfo.Name}".ToLower();
        //    var prefix = GetPrefix(Context) ?? $"@{Context.Client.CurrentUser.Username} ";

        //    var embed = new EmbedBuilder()
        //        .WithInfoColor();

        //    var aliases = new List<string>();
        //    foreach (var overload in command)
        //    {
        //        var result = await overload.CheckPreconditionsAsync(Context, _provider);
        //        if (result.IsSuccess)
        //        {
        //            var sbuilder = new StringBuilder()
        //                .Append(prefix + overload.Aliases.First());
        //            var fields = new List<EmbedFieldBuilder>();
        //            foreach (var parameter in overload.Parameters)
        //            {
        //                var p = parameter.Name;
        //                p.FirstCharToUpper();
        //                if (parameter.Summary != null)
        //                    fields.Add(new EmbedFieldBuilder().WithName(p).WithValue(parameter.Summary));
        //                if (parameter.IsRemainder)
        //                    p += "...";
        //                if (parameter.IsOptional)
        //                    p = $"[{p}]";
        //                else
        //                    p = $"<{p}>";
        //                sbuilder.Append(" " + p);
        //            }

        //            embed.AddField(sbuilder.ToString(), overload.Remarks ?? overload.Summary);
        //            for (var i = 0; i < fields.Count; i++)
        //                embed.AddField(fields[i]);
        //        }
        //        aliases.AddRange(overload.Aliases);
        //    }

        //    embed.WithFooter(x => x.Text = $"Aliases: {string.Join(", ", aliases)}");

        //    await ReplyEmbedAsync(embed: embed);
        //}
        private static string GetPrefix(CustomCommandContext context)
            => context.Server.Prefix ?? null;
    }
}
