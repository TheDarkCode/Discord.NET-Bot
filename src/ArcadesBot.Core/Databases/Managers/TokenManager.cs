//using System.Linq;
//using System.Threading.Tasks;
//using Discord.Commands;

//namespace ArcadesBot
//{
//    public class TokenManager : DbManager<TokenDatabase>
//    {
//        public TokenManager(TokenDatabase db) 
//            : base(db) { }

//        public async Task<Token> GetTokenObject(ICommandContext context)
//        {
//            var token = _db.Token.FirstOrDefault(x => x.UserId == context.User.Id && x.GuildId == context.Guild.Id);

//            if (token == null)
//            {
//                await CreateTokenAccount(context.User.Id, context.Guild.Id);
//            }
//            return token;
//        }
//        public async Task<Token> GetTokenObject(ulong userId, ulong guildId)
//        {
//            var token = _db.Token.FirstOrDefault(x => x.UserId == userId && x.GuildId == guildId);

//            if (token == null)
//                token = await CreateTokenAccount(userId, guildId);

//            return token;
//        }

//        private async Task<Token> CreateTokenAccount(ulong userId, ulong guildId)
//        {
//            var token = new Token
//            {
//                UserId = userId,
//                GuildId = guildId,
//                Tokens = 0
//            };
//            _db.Token.Add(token);
//            await _db.SaveChangesAsync();
//            return GetTokenObject(userId, guildId).Result;
//        }

//        public async Task AddTokens(Token user, ulong amount)
//        {
//            user.AddTokens(amount);
//            _db.Update(user);
//            await _db.SaveChangesAsync();
//        }

//        public async Task RemoveTokens(Token user, ulong amount)
//        {
//            user.RemoveTokens(amount);
//            _db.Update(user);
//            await _db.SaveChangesAsync();
//        }
//    }
//}