using System;
using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Utils.Logger
{
    public static class Logger
    {
        public static void Log(ELogLevel logLevel, string player, string message, string stackTrace = "")
        {
            // log to console
            switch (logLevel)
            {
                case ELogLevel.UnknownException:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ELogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case ELogLevel.Connection:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case ELogLevel.JS:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case ELogLevel.Message:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            Console.WriteLine($"{logLevel}-{player}::{message}");
            foreach (string line in stackTrace.Split('\n'))
                Console.WriteLine($"    {line}");

            Console.ResetColor();

            // log to DB
            using (FortDbContext context = new FortDbContext())
            {
                context.Logs.Add(new Log
                {
                    LevelE = logLevel,
                    Player = player,
                    Message = message,
                    StackTrace = stackTrace,
                    At = DateTime.UtcNow
                });
                context.SaveChanges();
            }
        }
    }
}