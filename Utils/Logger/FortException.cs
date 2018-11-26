namespace Fort.Utils.Logger
{
    public class FortException : System.Exception
    {
        public FortException(ELogLevel logLevel, string player, string message) : base(message)
        {
            LogLevel = logLevel;
            Player = player;
        }
        public FortException(ELogLevel logLevel, string player, string message, System.Exception inner) : base(message, inner)
        {
            LogLevel = logLevel;
            Player = player;
        }

        public string Player { get; private set; }
        public ELogLevel LogLevel { get; private set; }
    }
}