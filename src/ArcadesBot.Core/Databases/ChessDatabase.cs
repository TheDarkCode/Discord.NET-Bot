using Discord;
using Microsoft.EntityFrameworkCore;
using ChessDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class ChessDatabase : DbContext
    {
        public DbSet<ChessMatch> Matches  { get; private set; }
        public DbSet<ChessChallenge> Challenges { get; private set; }
        public ChessDatabase()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "data");
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            string datadir = Path.Combine(baseDir, "chess.sqlite.db");
            optionsBuilder.UseSqlite($"Filename={datadir}");
        }

        public async Task<ChessChallenge> GetChallengeAsync(ulong guildId, ulong channelId, ulong invokerId)
            => await Challenges.Where(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId)).OrderByDescending(x => x.TimeoutDate).FirstAsync();

        public async Task<bool> CheckPlayerInMatchAsync(ulong guildId, ulong invokerId)
        {
            ulong number = 1;
            return await Matches.AnyAsync(x => x.GuildId == guildId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == number);
        }

        public async Task<ChessMatch> GetMatchAsync(ulong guildId, ulong channelId, ulong invokerId)
        {
            ulong number = 1;
            return await Matches.FirstOrDefaultAsync(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId) && x.Winner == number);
        }

        public async Task<ChessMatch> GetMatchAsync(ulong? Id)
        {
            return await Matches.FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<ChessChallenge> CreateChallengeAsync(ulong guildId, ulong channelId, IUser challenger, IUser challengee, ulong timeoutInSeconds)
        {
            var challenge = new ChessChallenge(guildId, channelId, challenger.Id, challengee.Id, DateTime.Now.AddSeconds(timeoutInSeconds));
            var entityEntry = await Challenges.AddAsync(challenge);
            int num = await SaveChangesAsync();
            return await GetChallengeAsync(guildId, channelId, challenger.Id);
        }

        public async Task<ChessMatch> GetChessMatchAsync(ulong guildId, ulong channelId, ulong invokerId)
            => await Matches.FirstOrDefaultAsync(x => x.GuildId == guildId && x.ChannelId == channelId && (x.ChallengeeId == invokerId || x.ChallengerId == invokerId));

        public async Task UpdateChessGameAsync(ChessMatch chessMatch, ChessMatchStatus matchStatus)
        {
            Player player = chessMatch.ChessGame.WhoseTurn == Player.White ? Player.Black : Player.White;

            if (matchStatus.WinnerId != null)
                chessMatch.Winner = (ulong)matchStatus.WinnerId;

            if (chessMatch.ChessGame.IsStalemated(player))
                chessMatch.Stalemate = true;

            Matches.Update(chessMatch);
            await SaveChangesAsync();
        }

        public async Task<ChessMatch> AcceptChallengeAsync(ChessChallenge challenge, ChessGame chessGame, string blackURL, string whiteURL)
        {
            challenge.Accepted = true;
            Challenges.Update(challenge);
            ChessMatch entity = new ChessMatch(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId, challenge.ChallengeeId, chessGame, whiteURL, blackURL);
            var entityEntry = await Matches.AddAsync(entity);
            int num = await SaveChangesAsync();
            return await GetMatchAsync(challenge.GuildId, challenge.ChannelId, challenge.ChallengerId);
        }

        public async Task<ulong> ResignAsync(ulong guildId, ulong channelId, ulong invokerId)
        {
            ChessDatabase chessDatabase = this;
            ChessMatch chessMatch = await chessDatabase.GetMatchAsync(guildId, channelId, invokerId);
            chessMatch.Winner = (long)chessMatch.ChallengeeId == (long)invokerId ? chessMatch.ChallengerId : chessMatch.ChallengeeId;
            chessDatabase.Matches.Update(chessMatch);
            int num = await chessDatabase.SaveChangesAsync();
            return chessMatch.Winner;
        }
    }
}
