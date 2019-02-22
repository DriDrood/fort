using System;
using System.Threading.Tasks;
using Fort;
using Fort.Database.Entities;
using Fort.Module.Army;
using Fort.Utils.Logger;

namespace Fort.Module
{
    public class ActionService
    {
        private RoundService _roundService = Program.GetService<RoundService>();
        private PlayerService _playerService = Program.GetService<PlayerService>();

        public Task PlayerTurn(ContextService context, int sourceCityId, int targetCityId, int amount)
        {
            if (context.Database.Cities.Find(sourceCityId).OwnerId != context.CurrentPlayer.Id)
                throw new FortException(ELogLevel.Warning, "Toto město není vaše");

            if (_roundService.State != RoundService.Status.Running && _roundService.State != RoundService.Status.Paused)
                throw new FortException(ELogLevel.Warning, "Kolo neběží");

            return context.GetArmyService().PlayerTurn((User)context.CurrentPlayer, sourceCityId, targetCityId, amount);
        }
        public Task PlayerReady(ContextService context, bool setReady)
        {
            return _playerService.PlayerReady(context, _roundService.CurrentRound.Id, setReady);
        }
        public void StartOrResumeGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste administrátor");

            if (_roundService.State == RoundService.Status.New)
                _roundService.StartGame();
            else if (_roundService.State == RoundService.Status.Paused)
                _roundService.Resume();
            else
                throw new Exception($"Nemůžete spustit hru, která má status '{_roundService.State}'");
        }
        public void PauseGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("Nejste administrátor");

            _roundService.Pause();
        }
        public void FinishTimer(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("Nejste administrátor");

            _roundService.FinishTimer();
        }
        public void RestartGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("Nejste administrátor");

            _roundService.ResetGame(context);
        }
        public void HistoryBack() { }
        public void HistoryForward() { }
        public void HistoryNow() { }
    }
}