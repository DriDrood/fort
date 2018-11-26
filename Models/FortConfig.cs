using System.Collections.Generic;

namespace Fort.Models
{
    public class FortConfig
    {
        public int RealHeight { get; set; }
        public int RealWidth { get; set; }
        public Dictionary<string, string> ColorsForAdmin { get; set; }
    }
}