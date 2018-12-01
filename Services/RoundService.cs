using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils;
using Fort.Utils.Logger;

namespace Fort.Services
{
    public class RoundService
    {
        public RoundService()
        {
            _timer = new Timer();
        }

        private CommService _commService => Program.GetService<CommService>();
        private Timer _timer;
        private Task _playTask;

        public Round CurrentRound { get; private set; }

        public void Turn(User user, Turn turn)
        {
            if (CurrentRound.EndsAt != null)
                throw new FortException(ELogLevel.Warning, "Kolo již skončilo");

            using (FortDbContext context = new FortDbContext())
            {
                turn.User = user;
                turn.Round = CurrentRound;
                turn.CreatedAt = DateTime.UtcNow;

                context.Turns.Add(turn);
                context.SaveChanges();
            }
        }

        #region Admin
        public void Pause(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _timer.Pause();
        }

        public void Resume(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _timer.Resume();
        }

        public void EndRound(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _timer.End();
        }

        public void RestartAll(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");
#warning TODO
            throw new NotImplementedException();
        }

        public void Play(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _playTask = Task.Run(async () =>
            {
                using (FortDbContext context = new FortDbContext())
                {
                    while (context.Teams.Count(t => t.Members.Any(m => m.Cities.Any())) > 1)
                    {
                        StartRound(context);
                        await _timer.NewStart(TimeSpan.FromSeconds(Program.Config.DefaultRoundDurationSec));

                        EndRound(context);
                        await _timer.NewStart(TimeSpan.FromSeconds(Program.Config.DefaultBeforeVisualizationSec));

                        Visualize(context);
                        await _timer.NewStart(TimeSpan.FromSeconds(Program.Config.DefaultAfterVisualizationSec));
                    }
                }
            });
        }
        private void StartRound(FortDbContext context)
        {
            CurrentRound = new Round
            {
                StartsAt = DateTime.UtcNow,
                EndsAt = null
            };
            context.Add(CurrentRound);
            context.SaveChanges();

            var startMessageTask = _commService.SendToAll("StartRound", Program.Config.DefaultRoundDurationSec);
        }
        private void EndRound(FortDbContext context)
        {
            CurrentRound.EndsAt = DateTime.UtcNow;
            context.SaveChanges();

            var startMessageTask = _commService.SendToAll("EndRound", CurrentRound);

#warning TODO: turns
        }
        private void Visualize(FortDbContext context)
        {
#warning TODO: Foreach display Start, Move, End
        }
        #endregion
    }
}