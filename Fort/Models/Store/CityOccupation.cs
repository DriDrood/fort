using System;

namespace Fort.Models.Store
{
    public class CityOccupation
    {
        public Guid PlayerId { get; set; }
        public int Size { get; set; }
        public int? Army { get; set; }
    }
}