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

        public ICollection<Order> SourceForOrders { get; set; } = new HashSet<Order>();
        public ICollection<Order> TargetForOrders { get; set; } = new HashSet<Order>();

        public ICollection<Road> SourceForRoads { get; set; } = new HashSet<Road>();
        public ICollection<Road> TargetForRoads { get; set; } = new HashSet<Road>();

        public ICollection<StartingPosition> StartingPositionFor { get; set; } = new HashSet<StartingPosition>();

        // public virtual ICollection<Path> SourceToPaths { get; set; }
        // public virtual ICollection<Path> TargetToPaths { get; set; }
        // public virtual ICollection<Turn> SourceToTurns { get; set; }
        // public virtual ICollection<Turn> TargetToTurns { get; set; }

        // public virtual ICollection<StartingPosition> Start { get; set; }

        // [NotMapped]
        // public IEnumerable<City> Neighbour => SourceToPaths.Select(p => p.Target).Union(TargetToPaths.Select(p => p.Source));
    }
}