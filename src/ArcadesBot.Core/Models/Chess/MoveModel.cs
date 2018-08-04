using System;
using ChessDotNet;

namespace ArcadesBot
{
    public class ChessMoveModel
    {
        public Move Move { get; set; }
        public Player Player { get; set; }
        public DateTime MoveDate { get; set; }
        public char MovedfenChar { get; set; }
    }
}