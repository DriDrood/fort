using System;

namespace Fort.Models.Store
{
    public class Order
    {
        public Guid PlayerId { get; set; }
        public int Size { get; set; }
        public int? Amount { get; set; }
    }
}