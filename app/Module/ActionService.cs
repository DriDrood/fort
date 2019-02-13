using System;
using Fort;
using Fort.Database.Entities;

namespace Fort.Module
{
    public class ActionService
    {
        private RoundService _roundService = Program.GetService<RoundService>();
        private PlayerService _playerService = Program.GetService<PlayerService>();
        private ArmyService _armyService = Program.GetService<ArmyService>();

        public void PlayerTurn(ContextService context, string sourceCityId, string targetCityId, int amount)
        {
            _armyService.PlayerTurn();
        }
        public void PlayerReady()
        {
            _playerService.PlayerReady();
        }
        public void StartOrResumeGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("You are not admin");

            if (_roundService.State == RoundService.Status.New)
                _roundService.StartGame();
            else if (_roundService.State == RoundService.Status.Paused)
                _roundService.Resume();
            else
                throw new Exception($"You cannot start game in status '{_roundService.State}'");
        }
        public void PauseGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("You are not admin");

            _roundService.Pause();
        }
        public void FinishTimer(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("You are not admin");

            _roundService.FinishTimer();
        }
        public void RestartGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("You are not admin");

            _roundService.ResetGame(context);
        }
        public void HistoryBack() { }
        public void HistoryForward() { }
        public void HistoryNow() { }
    }
}