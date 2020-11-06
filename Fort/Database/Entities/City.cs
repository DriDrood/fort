using System;
using System.Collections.Generic;

namespace Fort.Database.Entities
{
    public class City
    {
        public Guid Id { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public double PopulationGrowCoef { get; set; } = 1;
        public double DefenceCoef { get; set; } = 1;

        public ICollection<CityOccupation> CityOccupations { get; set; } = new HashSet<CityOccupation>();

        public ICollection<Order> StForOrders { get; set; } = new HashSet<Order>();
        public ICollection<Order> NdForOrders { get; set; } = new HashSet<Order>();

        public ICollection<Road> StForRoads { get; set; } = new HashSet<Road>();
        public ICollection<Road> NdForRoads { get; set; } = new HashSet<Road>();

        public ICollection<StartingPosition> StartingPositionFor { get; set; } = new HashSet<StartingPosition>();
    }
}