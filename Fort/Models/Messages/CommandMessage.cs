namespace Fort.Models.Messages
{
    public class CommandMessage : IMessage
    {
        public ECommand Command { get; set; }
    }

    public enum ECommand
    {
        Start,
        
    }
}