namespace Fort.Database.Entities
{
    public class Path
    {
        public int Id { get; set; }

        public int SourceId { get; set; }
        public virtual City Source { get; set; }

        public int TargetId { get; set; }
        public virtual City Target { get; set; }
    }
}