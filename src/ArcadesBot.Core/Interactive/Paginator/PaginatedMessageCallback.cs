using System;
using System.Linq;
using System.Threading.Tasks;
using ArcadesBot.Common;
using ArcadesBot.Interactive.Callbacks;
using ArcadesBot.Interactive.Criteria;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ArcadesBot.Interactive.Paginator
{
    public class PaginatedMessageCallback : IReactionCallback
    {
        public CustomCommandContext Context { get; }
        public InteractiveService Interactive { get; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion { get; }

        public TimeSpan? Timeout => _options.Timeout;

        private readonly PaginatedMessage _pager;

        private PaginatedAppearanceOptions _options => _pager.Options;
        private readonly int _pages;
        private int _page = 1;
        

        public PaginatedMessageCallback(InteractiveService interactive,
            CustomCommandContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _pager = pager;
            _pages = _pager.Pages.Count();
        }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(_pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            await Task.Run(async () =>
            {
                await message.AddReactionAsync(_options.First);
                await message.AddReactionAsync(_options.Back);
                await message.AddReactionAsync(_options.Next);
                await message.AddReactionAsync(_options.Last);

                var manageMessages = (Context.Channel is IGuildChannel guildChannel)
                    ? (Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages
                    : false;

                if (_options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || (_options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                    await message.AddReactionAsync(_options.Jump);

                await message.AddReactionAsync(_options.Stop);

                if (_options.DisplayInformationIcon)
                    await message.AddReactionAsync(_options.Info);
            });
            if (Timeout.HasValue)
            {
                await Task.Delay(Timeout.Value).ContinueWith(async _ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    await Message.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(_options.First))
            {
                _page = 1;
            }
            else if (emote.Equals(_options.Next))
            {
                if (_page >= _pages)
                    return false;
                ++_page;
            }
            else if (emote.Equals(_options.Back))
            {
                if (_page <= 1)
                    return false;
                --_page;
            }
            else if (emote.Equals(_options.Last))
            {
                _page = _pages;
            }
            else if (emote.Equals(_options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(_options.Jump))
            {
                await Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > _pages)
                    {
                        await response.DeleteAsync().ConfigureAwait(false);
                        await Interactive.ReplyAndDeleteAsync(Context, _options.Stop.Name);
                        return;
                    }
                    _page = request;
                    await response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(_options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, _options.InformationText, timeout: _options.InfoTimeout);
                return false;
            }
            await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }
        
        protected Embed BuildEmbed()
        {
            return new EmbedBuilder()
                .WithAuthor(_pager.Author)
                .WithColor(_pager.Color)
                .WithDescription(_pager.Pages.ElementAt(_page-1).ToString())
                .WithFooter(f => f.Text = string.Format(_options.FooterFormat, _page, _pages))
                .WithTitle(_pager.Title)
                .Build();
        }
        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}
