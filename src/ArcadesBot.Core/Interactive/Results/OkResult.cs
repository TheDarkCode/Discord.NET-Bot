﻿using Discord.Commands;

namespace ArcadesBot.Interactive.Results
{
    public class OkResult : RuntimeResult
    {
        public OkResult(string reason = null) : base(null, reason) { }
    }
}
