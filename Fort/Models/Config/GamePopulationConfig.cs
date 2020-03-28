namespace Fort.Models.Config
{
    public class GamePopulationConfig
    {
        public int DefaultPlayerStartPopulation { get; set; } = 10;
        public int DefaultTurnGrow { get; set; } = 10;
        public int NeutralCitiesPopulationMin { get; set; } = 10;
        public int NeutralCitiesPopulationMax { get; set; } = 20;
    }
}