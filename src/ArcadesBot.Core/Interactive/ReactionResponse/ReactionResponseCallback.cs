using System;
using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Interactive.Callbacks;
using ArcadesBot.Interactive.Criteria;
using ArcadesBot.Services.BlackJack;
using ArcadesBot.Utility;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ArcadesBot.Interactive.ReactionResponse
{
    public class ReactionResponseCallback : IReactionCallback
    {
        public CustomCommandContext Context { get; }
        public InteractiveService Interactive { get; }
        public InteractiveBase InteractiveBase { get; }
        public IUserMessage Message { get; private set; }
        
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion { get; }
        public BlackJackService BlackJackService { get; }
        public TimeSpan? Timeout { get; }
        private bool _userClicked;
        private readonly ReactionResponseAppearanceOptions _options = new ReactionResponseAppearanceOptions();

        public ReactionResponseCallback(InteractiveService interactive,
            InteractiveBase interactiveBase,
            CustomCommandContext sourceContext,
            ICriterion<SocketReaction> criterion = null,
            BlackJackService blackJackService = null)
        {
            InteractiveBase = interactiveBase;
            Timeout = TimeSpan.FromSeconds(30);
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            BlackJackService = blackJackService;
        }

        public async Task DisplayAsync()
        {
            var guid = BlackJackService.CreateImage(Context.User.Id);
            var embedBuilder = new EmbedBuilder()
                .WithSuccessColor()
                .WithImageUrl($"attachment://board{guid}.png")
                .WithDescription(BlackJackService.GetScoreFromMatch(Context.User.Id))
                .WithFooter("React with the action you want to perform. (timeout = 30 seconds)");
            var message = await InteractiveBase.SendFileAsync($"BlackJack/board{guid}.png", embed: embedBuilder);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            await Task.Run(async () =>
            {
                await message.AddReactionAsync(_options.HitEmote);
                await message.AddReactionAsync(_options.StandEmote);
            });

            if (Timeout != null)
            {
                await Task.Delay(Timeout.Value).ContinueWith(async _ =>
                {
                    //BlackJackService.RemovePlayerFromMatch(Context.User.Id);
                    Interactive.RemoveReactionCallback(message);
                    if(!_userClicked)
                        await Message.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(_options.HitEmote))
            {
                _userClicked = true;
                await Message.DeleteAsync();
                Interactive.RemoveReactionCallback(Message);
                await Task.Run(async () =>
                {
                    await DisplayAsync();
                });
                

                //BlackJackService.RemovePlayerFromMatch(Context.User.Id);
                PrettyConsole.Log(LogSeverity.Info, "Callback", "User clicked HitEmote");
            }
            if (emote.Equals(_options.StandEmote))
            {
                _userClicked = true;
                //BlackJackService.RemovePlayerFromMatch(Context.User.Id);
                PrettyConsole.Log(LogSeverity.Info, "Callback", "User clicked StandEmote");
            }
            await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            //await RenderAsync().ConfigureAwait(false);
            return false;
        }


        //private async Task RenderAsync()
        //{
        //    var embed = BuildEmbed();
        //    await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        //}
    }
}
