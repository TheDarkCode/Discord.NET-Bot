using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace ArcadesBot.Interactive.ReactionResponse
{
    public class ReactionResponseAppearanceOptions
    {
        public IEmote StandEmote = new Emoji("\U0001f1f8");
        public IEmote HitEmote = new Emoji("\U0001f1ed");
    }
}
