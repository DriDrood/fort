using System;
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
                ArmyMoveDuration = Game.Animations.ArmyMoveSec,
                NotificationDuration = Game.Animations.NotificationSec
            };
        }

        public static string ConnectionString { get; private set; }
        public static CommConfig Comm { get; private set; } = new CommConfig();
        public static GameConfig Game { get; private set; } = new GameConfig();
        public static JwtTokenConfig JwtToken { get; private set; } = new JwtTokenConfig();
        public static LoggerConfig Logger { get; private set; } = new LoggerConfig();

        public static Config Config { get; set; }

        public static DateTime GetTurnEnd(DateTime now)
        {
            var duration = _configDuration();
            return duration != null
                ? (now + duration.Value)
                : (_configEndsAt(now)
                    ?? throw new Exception("Turn end not configured!"));
        }

        public static TimeSpan GetDuration(DateTime now)
        {
            var duration = _configDuration();
            if (duration != null)
                return duration.Value;
            
            var endsAt = _configEndsAt(now);
            if (endsAt != null)
                return endsAt.Value - now;

            throw new Exception("Turn end not configured!");
        }

        private static TimeSpan? _configDuration()
        {
            return TimeSpan.TryParse(Game.Lifecycle.TurnDuration, out var duration)
                ? (TimeSpan?)duration
                : null;
        }

        private static DateTime? _configEndsAt(DateTime now)
        {
            if (!TimeSpan.TryParse(Game.Lifecycle.TurnEndsAt, out var endsAt))
                return null;

            // ends tomorow
            if (now.TimeOfDay > endsAt)
                return now.Date.AddDays(1) + endsAt;

            // ends today
            return now.Date + endsAt;
        }
    }
}