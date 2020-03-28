using Fort.Models.Config;
using Fort.Models.Store;
using Microsoft.Extensions.Configuration;

namespace Fort.Managers
{
    public static class ConfigManager
    {
        public static void Setup(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");

            configuration.GetSection("Comm").Bind(Comm);
            configuration.GetSection("Game").Bind(Game);
            configuration.GetSection("JwtToken").Bind(JwtToken);
            configuration.GetSection("Logger").Bind(Logger);
            Config = new Config
            {
                ArmyRunDuration = Game.Animations.ArmyRunSec,
                NotificationDuration = Game.Animations.NotificationSec
            };
        }

        public static string ConnectionString { get; private set; }
        public static CommConfig Comm { get; private set; }
        public static GameConfig Game { get; private set; }
        public static JwtTokenConfig JwtToken { get; private set; }
        public static LoggerConfig Logger { get; private set; }

        public static Config Config { get; set; }
    }
}