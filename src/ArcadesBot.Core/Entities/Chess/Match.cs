//using ChessDotNet;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Text.RegularExpressions;

//namespace ArcadesBot
//{
//    public class ChessMatch : IEntity<ulong>
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public ulong Id { get; set; }
//        [Required]
//        public ulong GuildId { get; set; }
//        [Required]
//        public ulong ChannelId { get; set; }
//        [Required]
//        public ulong ChallengerId { get; set; }
//        [Required]
//        public ulong ChallengeeId { get; set; }
//        [NotMapped]
//        public ChessGame ChessGame { get; set; }
//        public string ChessGameJson
//        {
//            get
//            {
//                return JsonConvert.SerializeObject(ChessGame)
//                    .Replace("\"File\":0,", "\"File\":\"A\",")
//                    .Replace("\"File\":1,", "\"File\":\"B\",")
//                    .Replace("\"File\":2,", "\"File\":\"C\",")
//                    .Replace("\"File\":3,", "\"File\":\"D\",")
//                    .Replace("\"File\":4,", "\"File\":\"E\",")
//                    .Replace("\"File\":5,", "\"File\":\"F\",")
//                    .Replace("\"File\":6,", "\"File\":\"G\",")
//                    .Replace("\"File\":7,", "\"File\":\"H\",")
//                    .Replace(",\"Player\":0,", ",\"Player\":\"Black\",")
//                    .Replace(",\"Player\":1,", ",\"Player\":\"White\",")
//                    .Replace(",\"Owner\":0", ",\"Owner\":\"Black\"")
//                    .Replace(",\"Owner\":1,", ",\"Owner\":\"White\"")
//                    .Replace(",\"Castling\":0", ",\"Castling\":\"None\"")
//                    .Replace(",\"Castling\":1,", ",\"Castling\":\"King\"")
//                    .Replace(",\"Castling\":2", ",\"Castling\":\"Queen\"");
//            }
//            set
//            {
//                List<Move> moveList = new List<Move>();
//                var FileMatches = Regex.Matches(value, "(\"File\":\")");
//                var RankMatches = Regex.Matches(value, "(\"Rank\":)");
//                var PlayerMatches = Regex.Matches(value, "(Player\":)");
//                for (int i = 0; i < FileMatches.Count; i += 2)
//                {
//                    var oPosFile = value.Substring(FileMatches[i].Groups[0].Index + FileMatches[i].Groups[0].Length, 1);
//                    var oPosRank = value.Substring(RankMatches[i].Groups[0].Index + RankMatches[i].Groups[0].Length, 1);
//                    var nPosFile = value.Substring(FileMatches[i + 1].Groups[1].Index + FileMatches[i + 1].Groups[1].Length, 1);
//                    var nPosRank = value.Substring(RankMatches[i + 1].Groups[1].Index + RankMatches[i + 1].Groups[1].Length, 1);
//                    Player player = value.Substring(PlayerMatches[i / 2].Index + PlayerMatches[i / 2].Length + 1, 1) == "W" ? Player.White : Player.Black;
//                    Move move = new Move(oPosFile + oPosRank, nPosFile + nPosRank, player);
//                    moveList.Add(move);
//                }
//                try
//                {
//                    if (moveList.Count == 0 && ChessGame == null)
//                        ChessGame = new ChessGame();
//                    else
//                        ChessGame = new ChessGame(moveList, true);
//                }
//                catch (InvalidOperationException)
//                {
//                    MoveList = moveList;
//                }
//            }
//        }
//        [NotMapped]
//        public List<Move> MoveList { get; set; }
//        [NotMapped]
//        public List<ChessMove> HistoryList { get; set; } = new List<ChessMove>();
//        public string History
//        {
//            get
//            {
//                return JsonConvert.SerializeObject(HistoryList)
//                    .Replace("\"File\":0,", "\"File\":\"A\",")
//                    .Replace("\"File\":1,", "\"File\":\"B\",")
//                    .Replace("\"File\":2,", "\"File\":\"C\",")
//                    .Replace("\"File\":3,", "\"File\":\"D\",")
//                    .Replace("\"File\":4,", "\"File\":\"E\",")
//                    .Replace("\"File\":5,", "\"File\":\"F\",")
//                    .Replace("\"File\":6,", "\"File\":\"G\",")
//                    .Replace("\"File\":7,", "\"File\":\"H\",")
//                    .Replace(",\"Player\":0,", ",\"Player\":\"Black\",")
//                    .Replace(",\"Player\":1,", ",\"Player\":\"White\",");
//            }
//            set
//            {
//                if (HistoryList != null)
//                    HistoryList.Clear();
//                var MoveMatches = Regex.Matches(value, "(Move)");
//                var FileMatches = Regex.Matches(value, "(\"File\":\")");
//                var RankMatches = Regex.Matches(value, "(\"Rank\":)");
//                var PlayerMatches = Regex.Matches(value, "(Player\":)");
//                var DateMatches = Regex.Matches(value, "(MoveDate\":\")");
//                int index = 0;
//                for (int i = 0; i < MoveMatches.Count; i += 2)
//                {
//                    string oPosFile = value.Substring(FileMatches[i].Groups[0].Index + FileMatches[i].Groups[0].Length, 1);
//                    string oPosRank = value.Substring(RankMatches[i].Groups[0].Index + RankMatches[i].Groups[0].Length, 1);
//                    string nPosFile = value.Substring(FileMatches[i + 1].Groups[1].Index + FileMatches[i + 1].Groups[0].Length, 1);
//                    string nPosRank = value.Substring(RankMatches[i + 1].Groups[1].Index + RankMatches[i + 1].Groups[0].Length, 1);
//                    string DateTime = value.Substring(DateMatches[i / 2].Groups[0].Index + DateMatches[i / 2].Groups[0].Length, 33);
//                    Player player = value.Substring(PlayerMatches[i / 2].Index + PlayerMatches[i / 2].Length + 1, 1) == "W" ? Player.White : Player.Black;
//                    Move move = new Move(oPosFile + oPosRank, nPosFile + nPosRank, player);
//                    string TrimmedDateTime = DateTime.TrimEnd('}', '"');
//                    HistoryList.Add(new ChessMove()
//                    {
//                        Move = move,
//                        MoveDate = Convert.ToDateTime(TrimmedDateTime)
//                    });
//                    index += 2;
//                }
//            }
//        }
//        public string WhiteAvatarURL { get; set; }
//        public string BlackAvatarURL { get; set; }
//        public ulong Winner { get; set; } = 1;
//        public bool Stalemate { get; set; }
//        public ChessMatch()
//        {}
//        public ChessMatch(ulong guildId, ulong channelId, ulong challenger, ulong challengee, ChessGame chessGame, string whiteAvatarURL, string blackAvatarURL)
//        {
//            GuildId = guildId;
//            ChannelId = channelId;
//            ChallengerId = challenger;
//            ChallengeeId = challengee;
//            ChessGame = chessGame;
//            History = "[]";
//            BlackAvatarURL = blackAvatarURL;
//            WhiteAvatarURL = whiteAvatarURL;
//        }
//    }
//}