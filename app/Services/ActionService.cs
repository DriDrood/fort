using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils.Logger;

namespace Fort.Services
{
    public class ActionService
    {
        private RoundService _roundService => Program.GetService<RoundService>();
        private CommService _commService => Program.GetService<CommService>();

        public void Turn(User user, Turn turn)
        {
            if (_roundService.State != Utils.Timer.Status.Running && _roundService.State != Utils.Timer.Status.Paused)
                throw new FortException(ELogLevel.Warning, "Kolo neběží");

            if (!user.Cities.Any(c => c.Id == turn.SourceCityId))
                throw new FortException(ELogLevel.Warning, "Zdrojové město není vaše");

            using (FortDbContext context = new FortDbContext())
            {
                if (!context.Paths.Any(p => (p.SourceId == turn.SourceCityId && p.TargetId == turn.TargetCityId) || (p.SourceId == turn.TargetCityId && p.TargetId == turn.SourceCityId)))
                    throw new FortException(ELogLevel.Warning, "Zde není cesta");

                Turn dbTurn = context.Turns.SingleOrDefault(t => t.SourceCityId == turn.SourceCityId && t.TargetCityId == turn.TargetCityId && t.RoundId == _roundService.CurrentRound.Id);
                if (dbTurn != null)
                {
                    if (turn.Amount == 0)
                    {
                        context.Turns.Remove(dbTurn);
                    }
                    else
                    {
                        dbTurn.Amount = turn.Amount;
                        dbTurn.CreatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    turn.UserId = user.Id;
                    turn.RoundId = _roundService.CurrentRound.Id;
                    turn.CreatedAt = DateTime.UtcNow;
                    context.Turns.Add(turn);
                }

                context.SaveChanges();
            }
        }

        public void Ready(User user, bool ready)
        {
            if (ready)
                _roundService.CurrentRound.ReadyUsers.Add(user);
            else
                _roundService.CurrentRound.ReadyUsers.Remove(user);

            _commService.SendToAll("playerReady", new { ready = ready, playerId = user.Id });
        }

        public void Play(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            if (_roundService.State == Utils.Timer.Status.Begin)
                _roundService.StartGame();

            if (_roundService.State == Utils.Timer.Status.Paused)
                _roundService.Resume();
        }
        public void Pause(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _roundService.Pause();
        }

        public void End(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _roundService.ForceEnd();
        }

        public void RestartAll(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _roundService.ResetGame();
        }
    }
}