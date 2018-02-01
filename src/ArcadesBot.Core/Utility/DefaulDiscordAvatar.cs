namespace ArcadesBot
{
    static public class DefaulDiscordAvatar
    {
        public static string GetURL(ulong discriminator)
        {
            var after = discriminator % 5;
            return $"https://cdn.discordapp.com/embed/avatars/{after}.png";
        }
    }
}
