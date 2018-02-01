﻿using Discord;

namespace ArcadesBot
{
    public static class EmbedColors
    {
        public static Color GetErrorColor()
        {
            return Color.DarkRed;
        }
        public static Color GetInfoColor()
        {
            return Color.DarkGrey;
        }
        public static Color GetSuccessColor()
        {
            return Color.DarkBlue;
        }
    }
}