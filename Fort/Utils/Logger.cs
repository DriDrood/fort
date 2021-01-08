using System;
using System.IO;
using System.Text;
using Fort.Models.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Fort.Utils.Logger
{
  public class Logger
  {
    public void Setup(IConfigurationSection configLogger)
    {
      _config = new LoggerConfig();
      configLogger.Bind(_config);

      // create dir if not exists
      Directory.CreateDirectory(_config.Path);
    }
    public void Setup(LoggerConfig configLogger)
    {
      _config = configLogger;

      // create dir if not exists
      Directory.CreateDirectory(_config.Path);
    }

    private LoggerConfig _config;

    public void LogException(Exception ex)
    {
      if (!_config.LogToConsole && !_config.LogToFile)
        return;

      StringBuilder text = new StringBuilder();
      int level = 1;
      while (ex != null)
      {
        string intend = string.Empty.PadLeft(level, '\t');
        text.AppendFormat("{0}{1}{2}{3}{4}", ex.GetType().Name, ex.Message, Environment.NewLine, intend, ex.StackTrace.Replace("\n", $"\n{intend}"));

        ex = ex.InnerException;
      }

      _log(text.ToString(), "errors");
    }

    public void LogRequest(Guid id, string username, string requestMessage)
    {
      if (!_config.LogRequests || (!_config.LogToConsole && !_config.LogToFile))
        return;

      var text = $"REQ--{id}{_delimiter}{username}{_delimiter}{requestMessage.Replace("\n", "\\n")}";

      // exclude password
      var key = "\"password\":\"";
      var keyIndex = text.IndexOf(key);
      if (keyIndex != -1)
      {
        int iStart = keyIndex + key.Length;
        int iEnd = text.IndexOf("\"", iStart);
        text = $"{text.Substring(0, iStart)}****{text.Substring(iEnd)}";
      }

      _log(text, "req-resp");
    }

    public void LogResponse(Guid id, string body)
    {
      if (!_config.LogResponses || (!_config.LogToConsole && !_config.LogToFile))
        return;

      string text = $"RESP-{id}{_delimiter}{body.Replace("\n", "\\n")}";

      _log(text, "req-resp");
    }

    private void _log(string text, string filename)
    {
      DateTime now = DateTime.UtcNow;
      text = $"{now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}{_delimiter}{text}{Environment.NewLine}";

      if (_config.LogToFile)
      {
        lock (_fileLock)
        {
          File.AppendAllText(Path.Combine(_config.Path, $"{now.Year}-{now.Month}-{now.Day}_{filename}.log"), text);
        }
      }
      if (_config.LogToConsole)
      {
        Console.Write(text);
      }
    }

    private static string _delimiter = " :: ";
    private static object _fileLock = new object();
  }
}