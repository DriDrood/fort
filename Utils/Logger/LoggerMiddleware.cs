using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Microsoft.AspNetCore.Http;

namespace Fort.Utils.Logger
{
    public class LoggerMiddleware
    {
        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private RequestDelegate _next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (FortException ex)
            {
                Log(ex.LogLevel, ex.Player, ex.Message, ex.StackTrace);
            }
            catch (Exception ex)
            {
                // get player
                string player = $"URL:{context.Request.Path}";

                var paths = context.Request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries).Select(p => p.ToLower()).ToList();
                if (paths.Last() == "Map")
                {
                    if (paths.Count == 2)
                        player = $"U:{paths.First()}";
                    else if (paths.Count == 3)
                    {
                        if (paths.First() == "team")
                            player = $"T:{paths[1]}";
                        else if (paths.First() == "admin")
                            player = $"Admin";
                    }
                }
                
                Log(ELogLevel.UnknownException, player, ex.Message, ex.StackTrace);
            }
        }

        private void Log(ELogLevel logLevel, string player, string message, string stackTrace)
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