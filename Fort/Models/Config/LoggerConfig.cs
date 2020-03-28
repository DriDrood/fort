namespace Fort.Models.Config
{
    public class LoggerConfig
    {
        public string Path { get; set; }
        public bool LogToConsole { get; set; } = false;
        public bool LogToFile { get; set; } = true;
        public bool LogRequests { get; set; } = true;
        public bool LogResponses { get; set; } = true;
    }
}