using System;
using System.Collections.Generic;

namespace ArcadesBot
{
    public class TagModel
    {
        public string TagName { get; set; }
        public ulong OwnerId { get; set; }
        public List<string> Aliasses { get; set; } = new List<string>();
        public ulong Uses { get; set; } = 0;
        public string Content { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
}