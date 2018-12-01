namespace Fort.Utils.Logger
{
    public class FortException : System.Exception
    {
        public FortException(ELogLevel logLevel, string message) : base(message)
        {
            LogLevel = logLevel;
        }
        public FortException(ELogLevel logLevel, string message, System.Exception inner) : base(message, inner)
        {
            LogLevel = logLevel;
        }

        public ELogLevel LogLevel { get; private set; }
    }
}