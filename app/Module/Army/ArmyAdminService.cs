using System.Collections.Generic;
using System.Linq;
using Fort.Database.Entities;

namespace Fort.Module.Army
{
    public class ArmyAdminService : ArmyService
    {
        public ArmyAdminService(ContextService context) : base(context)
        {
        }

        protected override int GetArmy(City city)
        {
            return city.Army;
        }

        protected override string GetCityColor(City city)
        {
            return city.Owner?.Team?.Color ?? "gray";
        }

        protected override User GetOwner(City city)
        {
            return city.Owner;
        }

        protected override IEnumerable<Turn> GetVisibleTurn()
        {
            return _context.Database.Turns.Where(t => t.RoundId == _roundService.CurrentRound.Id);
        }
    }
}