using System.Collections.Generic;

namespace Fort.Models
{
    public class FortConfig
    {   
        public Dictionary<string, string> ColorsForAdmin { get; set; }

        public int DefaultPopulationStart { get; set; }
        public int DefaultPopulationGrow { get; set; }
        public Dictionary<string, int> NeutralCitiesPopulation { get; set; }

        public string RoundEndsAt { get; set; }
        public int? RoundDurationSec { get; set; }
        public int BeforeVisualizationSec { get; set; }
        public int AfterVisualizationSec { get; set; }
    }
}