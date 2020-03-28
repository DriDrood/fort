using System;
using System.Collections.Generic;

namespace Fort.Models.Store
{
    public class Turn
    {
        public Dictionary<Guid, CityOccupation> CityOccupations { get; set; }
        public Dictionary<string, Order> Orders { get; set; }
    }
}