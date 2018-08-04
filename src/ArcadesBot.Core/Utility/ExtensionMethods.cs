using Discord;
using System;
using System.Linq;

namespace ArcadesBot
{
    public static class ExtensionMethods
    {
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static EmbedBuilder WithErrorColor(this EmbedBuilder builder)
        {
            builder.Color = Color.DarkRed;
            return builder;
        }

        public static EmbedBuilder WithInfoColor(this EmbedBuilder builder)
        {
            builder.Color = Color.DarkGrey;
            return builder;
        }

        public static EmbedBuilder WithSuccessColor(this EmbedBuilder builder)
        {
            builder.Color = Color.DarkBlue;
            return builder;
        }
    }
}