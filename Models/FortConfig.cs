using System.Collections.Generic;

namespace Fort.Models
{
    public class FortConfig
    {   
        public Dictionary<string, string> ColorsForAdmin { get; set; }

        public int DefaultPopulationStart { get; set; }
        public int DefaultPopulationGrow { get; set; }
        public Dictionary<string, int> NeutralCitiesPopulation { get; set; }

        public int DefaultRoundDurationSec { get; set; }
        public int DefaultBeforeVisualizationSec { get; set; }
        public int DefaultAfterVisualizationSec { get; set; }
    }
}