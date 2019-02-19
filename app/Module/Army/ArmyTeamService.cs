using System.Collections.Generic;
using Fort.Database.Entities;

namespace Fort.Module.Army
{
    public class ArmyTeamService : ArmyService
    {
        public ArmyTeamService(ContextService context) : base(context)
        {
        }

        protected override int GetArmy(City city)
        {
            throw new System.NotImplementedException();
        }

        protected override string GetCityColor(City city)
        {
            throw new System.NotImplementedException();
        }

        protected override int GetImage(City city)
        {
            throw new System.NotImplementedException();
        }

        protected override string GetTurnColor(Turn turn)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<Turn> GetVisibleTurn()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsOwned(City city)
        {
            throw new System.NotImplementedException();
        }
    }
}