using System;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils.Logger;

namespace Fort.Services
{
    public class ActionService
    {
        private RoundService _roundService => Program.GetService<RoundService>();

        public void Turn(User user, Turn turn)
        {
            if (_roundService.State != Utils.Timer.Status.Running)
                throw new FortException(ELogLevel.Warning, "Kolo neběží");

            using (FortDbContext context = new FortDbContext())
            {
                turn.User = user;
                turn.Round = _roundService.CurrentRound;
                turn.CreatedAt = DateTime.UtcNow;

                context.Turns.Add(turn);
                context.SaveChanges();
            }
        }

        public void Play(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            _roundService.StartGame();
        }
        public async Task Pause(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            await _roundService.Pause();
        }

        public async Task Resume(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            await _roundService.Resume();
        }

        public async Task EndRound(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            await _roundService.End();
        }

        public async Task RestartAll(User user)
        {
            if (!user.IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste správce");

            await _roundService.ResetGame();
        }
    }
}