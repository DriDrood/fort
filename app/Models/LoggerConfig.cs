using System.Collections.Generic;

namespace Fort.Models
{
    public class LoggerConfig
    {
        public string Path { get; set; }
        public List<string> Console { get; set; }
        public List<string> File { get; set; }
        public List<string> DB { get; set; }
    }
}