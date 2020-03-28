using System.Collections.Generic;

namespace Fort.Models.Config
{
    public class GameConfig
    {
        public GameAnimationsConfig Animations { get; set; }
        public GameLifecycleConfig Lifecycle { get; set; }
        public GamePopulationConfig Population { get; set; }
        public IEnumerable<string> DeathStories { get; set; }
    }
}