using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Models;
using Microsoft.Extensions.Configuration;

namespace Fort.Utils.Logger
{
    public static class Logger
    {
        private static LoggerConfig _config;

        public static void Log(ELogLevel logLevel, string player, string message, string stackTrace = null)
        {
            try
            {
                StringBuilder fullText = new StringBuilder();
                fullText.AppendLine($"{DateTime.UtcNow} :: {logLevel.ToString().PadRight(17)} :: {player.PadRight(6)} :: {message}");
                foreach (string line in stackTrace?.Split(Environment.NewLine) ?? new string[] { })
                    fullText.AppendLine($"    {line}");

                // log to console
                if (_config.Console.Contains(logLevel.ToString()))
                {
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
                        case ELogLevel.MessageSend:
                        case ELogLevel.MessageReceive:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                    }
                    Console.Write(fullText.ToString());
                    Console.ResetColor();
                }

                // log to file
                if (_config.File.Contains(logLevel.ToString()))
                {
                    File.AppendAllText(
                        System.IO.Path.Combine(_config.Path, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.log"),
                        fullText.ToString());
                }

                // log to DB
                if (_config.DB.Contains(logLevel.ToString()))
                {
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
            catch (Exception ex)
            {
                #warning What TODO?
            }
        }

        public static void Configure(IConfigurationSection configLogger)
        {
            _config = new LoggerConfig();
            configLogger.Bind(_config);

            // create dir
            Directory.CreateDirectory(_config.Path);
        }
    }
}