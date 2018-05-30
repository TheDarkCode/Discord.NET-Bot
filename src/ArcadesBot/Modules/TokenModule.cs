//using System;
//using System.Linq;
//using Discord;
//using Discord.Commands;
//using System.Threading.Tasks;
//using Discord.WebSocket;
//using Microsoft.CodeAnalysis.Semantics;

//Very WIP and unfinished

//namespace ArcadesBot.Modules
//{
//    [Name("Tokens")]
//    [Summary("Everything related to tokens")]
//    public class TokenModule : ModuleBase<CustomCommandContext>
//    {
//        private readonly TokenManager _manager;
//        private readonly Random _r;

//        public TokenModule(TokenManager manager, Random r)
//        {
//            _r = r;
//            _manager = manager;
//        }

//        [Command("currency")]
//        [Alias("$", "cur")]
//        public async Task TokenAmount([Remainder] SocketUser user = null)
//        {
//            if (user == null)
//                user = Context.User;
//            var tokens = await _manager.GetTokenObject(user.Id, Context.Guild.Id);
//            var embed = new EmbedBuilder().WithDescription($"{user.Mention} has `{tokens.Tokens}` {StringHelper.MultipleOrNot("token", tokens.Tokens)}").WithColor(EmbedColors.GetSuccessColor());
//            await ReplyAsync("", embed: embed.Build());
//        }

//        [Command("givetokens")]
//        [Alias("donate", "give")]
//        public async Task GiveToken(ulong amount, [Remainder] SocketUser recipient)
//        {
//            var embed = new EmbedBuilder();
//            if (recipient.IsBot)
//            {
//                embed.WithDescription("Bots can't recieve tokens").WithColor(EmbedColors.GetErrorColor());
//            }
//            else
//            {
//                Token giver = await _manager.GetTokenObject(Context);
//                Token reciever = await _manager.GetTokenObject(recipient.Id, Context.Guild.Id);
//                if (giver.Equals(reciever))
//                {
//                    embed.WithDescription("You can't give tokens to yourself").WithColor(EmbedColors.GetErrorColor());
//                }
//                else if (giver.Tokens < amount)
//                {
//                    embed.WithDescription("You don't have that amount of tokens to give away")
//                        .WithColor(EmbedColors.GetErrorColor());
//                }
//                else
//                {
//                    await _manager.RemoveTokens(giver, amount);
//                    await _manager.AddTokens(reciever, amount);
//                    embed.WithDescription(
//                            $"{Context.User.Mention} has given {amount} {StringHelper.MultipleOrNot("token", amount)} to {recipient.Mention}")
//                        .WithColor(EmbedColors.GetSuccessColor());
//                }
//            }

//            await ReplyAsync("", embed: embed.Build());
//        }

//        [Command("betroll")]
//        [Alias("br")]
//        public async Task Betroll(ulong amount)
//        {

//            EmbedBuilder embed = new EmbedBuilder();
//            var roll = _r.Next(100);
//            var user = await _manager.GetTokenObject(Context);
//            if (user.Tokens < amount)
//            {
//                embed.WithDescription("You don't have that amount of tokens to bet").WithColor(EmbedColors.GetErrorColor());
//            }
//            else if (amount == 0)
//            {
//                return;
//            }
//            else
//            {
//                if (roll == 100)
//                {
//                    await _manager.AddTokens(user, amount * 10);
//                    embed.WithDescription($"{Context.User.Mention} `You rolled 100` Congratulations! You won {amount * 10} for rolling exactly 100").WithColor(EmbedColors.GetSuccessColor());
//                }
//                else if (roll > 90)
//                {
//                    await _manager.AddTokens(user, amount * 4);
//                    embed.WithDescription($"{Context.User.Mention} `You rolled {roll}` Congratulations! You won {amount * 4} for rolling above 90").WithColor(EmbedColors.GetSuccessColor());
//                }
//                else if (roll > 60)
//                {
//                    await _manager.AddTokens(user, amount * 2);
//                    embed.WithDescription($"{Context.User.Mention} `You rolled {roll}` Congratulations! You won {amount * 2} for rolling above 60").WithColor(EmbedColors.GetSuccessColor());
//                }
//                else
//                {
//                    await _manager.RemoveTokens(user, amount);
//                    embed.WithDescription($"{Context.User.Mention} `You rolled {roll}`. Better luck next time").WithColor(EmbedColors.GetInfoColor());
//                }


//            }
//            await ReplyAsync("", embed: embed.Build());
//        }
//    }
//}
