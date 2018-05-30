//using System;
//using System.IO;
//using Microsoft.EntityFrameworkCore;

//namespace ArcadesBot
//{
//    public class TokenDatabase : DbContext
//    {
//        public DbSet<Token> Token { get; private set; }

//        public TokenDatabase()
//        {
//            Database.EnsureCreated();
//        }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            string baseDir = Path.Combine(AppContext.BaseDirectory, "data");
//            if (!Directory.Exists(baseDir))
//                Directory.CreateDirectory(baseDir);

//            string datadir = Path.Combine(baseDir, "token.sqlite.db");
//            optionsBuilder.UseSqlite($"Filename={datadir}");
//        }
//    }
//}