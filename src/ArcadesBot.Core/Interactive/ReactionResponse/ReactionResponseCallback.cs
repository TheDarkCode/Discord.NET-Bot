using System;
using System.Linq;
using System.Threading.Tasks;
using ArcadesBot;
using ArcadesBot.Interactive.ReactionResponse;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
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
        public TimeSpan? Timeout { get; private set; }
        private bool _userClicked = false;
        private ReactionResponseAppearanceOptions _options = new ReactionResponseAppearanceOptions();

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
            BlackJackService = blackJackService ?? null;
        }

        public async Task DisplayAsync()
        {
            var guid = BlackJackService.CreateImage(Context.User.Id);
            var embedBuilder = new EmbedBuilder()
                .WithSuccessColor()
                .WithImageUrl($"attachment://board{guid}.png")
                .WithDescription(BlackJackService.GetScoreFromMatch(Context.User.Id))
                .WithFooter("Available options: stand and hit (timeout = 30 seconds)");
            var message = await InteractiveBase.SendFileAsync($"BlackJack/board{guid}.png", embed: embedBuilder);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                await message.AddReactionAsync(_options.HitEmote);
                await message.AddReactionAsync(_options.StandEmote);
            });

            if (Timeout != null && !_userClicked)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    BlackJackService.RemovePlayerFromMatch(Context.User.Id);
                    Interactive.RemoveReactionCallback(message);
                    _ = Message.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(_options.HitEmote))
            {
                _userClicked = true;
                BlackJackService.RemovePlayerFromMatch(Context.User.Id);
                PrettyConsole.Log(LogSeverity.Info, "Callback", "User clicked HitEmote");
            }
            if (emote.Equals(_options.StandEmote))
            {
                _userClicked = true;
                BlackJackService.RemovePlayerFromMatch(Context.User.Id);
                PrettyConsole.Log(LogSeverity.Info, "Callback", "User clicked StandEmote");
            }
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
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
