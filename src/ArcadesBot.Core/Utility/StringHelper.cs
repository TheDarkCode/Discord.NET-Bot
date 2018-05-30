using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;

namespace ArcadesBot
{
    public static class StringHelper
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
        public static string MultipleOrNot(string input, ulong amount)
            => amount == 1 ? $"{input}" : $"{input}s";

        public static string ParametersInfo(IReadOnlyCollection<ParameterInfo> Parameters)
            => Parameters.Any() ?
            string.Join(" ", Parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` ")) : null;
    }
}