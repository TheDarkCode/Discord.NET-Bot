using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public static class PrettyConsole
    {
        /// <summary> Write a string to the console on an existing line. </summary>
        /// <param name="text">String written to the console.</param>
        /// <param name="foreground">The text color in the console.</param>
        /// <param name="background">The background color in the console.</param>
        public static void Append(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor) foreground;
            Console.BackgroundColor = (ConsoleColor) background;
            Console.Write(text);
        }

        /// <summary> Write a string to the console on an new line. </summary>
        /// <param name="text">String written to the console.</param>
        /// <param name="foreground">The text color in the console.</param>
        /// <param name="background">The background color in the console.</param>
        public static void NewLine(string text = "", ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor) foreground;
            Console.BackgroundColor = (ConsoleColor) background;
            Console.Write(Environment.NewLine + text);
        }

        public static void Log(object severity, string source, string message)
        {
            NewLine($"{DateTime.Now.ToString("hh:mm:ss")} ", ConsoleColor.DarkGray);
            Append($"[{severity}] ", ConsoleColor.Red);
            Append($"{source}: ", ConsoleColor.DarkGreen);
            Append(message, ConsoleColor.White);
        }

        public static Task LogAsync(object severity, string source, string message)
        {
            NewLine($"{DateTime.Now.ToString("hh:mm:ss")} ", ConsoleColor.DarkGray);
            Append($"[{severity}] ", ConsoleColor.Red);
            Append($"{source}: ", ConsoleColor.DarkGreen);
            Append(message, ConsoleColor.White);
            return Task.CompletedTask;
        }

        public static void Log(IUserMessage msg)
        {
            var channel = msg.Channel as IGuildChannel;
            NewLine($"{DateTime.Now.ToString("hh:mm:ss")} ", ConsoleColor.Gray);

            if (channel?.Guild == null)
                Append($"[PM] ", ConsoleColor.Magenta);
            else
                Append($"[{channel.Guild.Name} #{channel.Name}] ", ConsoleColor.DarkGreen);

            Append($"{msg.Author}: ", ConsoleColor.Green);
            Append(msg.Content, ConsoleColor.White);
        }

        public static void Log(CommandContext c)
        {
            var channel = c.Channel as SocketGuildChannel;
            NewLine($"{DateTime.Now.ToString("hh:mm:ss")} ", ConsoleColor.Gray);

            if (channel == null)
                Append($"[PM] ", ConsoleColor.Magenta);
            else
                Append($"[{c.Guild.Name} #{channel.Name}] ", ConsoleColor.DarkGreen);

            Append($"{c.User}: ", ConsoleColor.Green);
            Append(c.Message.Content, ConsoleColor.White);
        }
    }
}