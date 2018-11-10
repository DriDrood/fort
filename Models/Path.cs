namespace Fort.Models
{
    public class Path
    {
        public string Id { get; set; }
        public Fortress Source { get; set; }
        public Fortress Target { get; set; }
    }
}