namespace Fort.Models
{
    public class Fortress
    {
        public string Name { get; set; }
        
        public int X { get; set; }
        public int Y { get; set; }

        public int Population { get; set; } = 5;

        public Owner Owner { get; set; } = new Owner();
    }
}