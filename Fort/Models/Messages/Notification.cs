namespace Fort.Models.Messages
{
    public class Notification : IMessage
    {
        public string Level { get; set; }
        public string Text { get; set; }
        public bool Permanent { get; set; }
    }
}