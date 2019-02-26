using System;
using System.Threading.Tasks;
using Fort;
using Fort.Database.Entities;
using Fort.Module.Army;
using Fort.Module.Comm;
using Fort.Utils.Logger;

namespace Fort.Module
{
    public class ActionService
    {
        private RoundService _roundService = Program.GetService<RoundService>();
        private PlayerService _playerService = Program.GetService<PlayerService>();
        private CommService _commService => Program.GetService<CommService>();

        public async Task PlayerTurn(ContextService context, int sourceCityId, int targetCityId, int amount)
        {
            if (context.Database.Cities.Find(sourceCityId).OwnerId != context.CurrentPlayer.Id)
                throw new FortException(ELogLevel.Warning, "Toto město není vaše");

            if (_roundService.State != RoundService.Status.Running && _roundService.State != RoundService.Status.Paused)
                throw new FortException(ELogLevel.Warning, "Kolo neběží");

            await context.GetArmyService().PlayerTurn((User)context.CurrentPlayer, sourceCityId, targetCityId, amount);
            await _commService.SendOne(context, context.CurrentPlayer.Id, "turn", new { sourceCityId, targetCityId, amount }, Lifetime.DataModification);
            await _commService.SendToAdmins(context, "turn", new { sourceCityId, targetCityId, amount }, Lifetime.DataModification);
        }
        public async Task PlayerReady(ContextService context, bool setReady)
        {
            await _playerService.PlayerReady(context, _roundService.CurrentRound.Id, setReady);

            await _commService.SendOne(context, context.CurrentPlayer.Id, "playerReady_ok", new { ready = setReady }, Lifetime.Notification);
            await _commService.SendToAdmins(context, "playerReady", new { ready = setReady, context.CurrentPlayer.Id }, Lifetime.DataModification);
        }
        public async Task StartOrResumeGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new FortException(ELogLevel.Warning, "Nejste administrátor");

            if (_roundService.State == RoundService.Status.New)
            {
                await _roundService.StartGame();
                await _commService.SendAll(context, "StartRound", new { roundNumber = _roundService.CurrentRound.RoundNumber, endsAt = _roundService.CurrentRound.EndsAt.ToString() }, Lifetime.DataModification);
            }
            else if (_roundService.State == RoundService.Status.Paused)
            {
                _roundService.Resume();
                await _commService.SendAll(context, "Resume", new { roundNumber = _roundService.CurrentRound.RoundNumber, endsAt = _roundService.CurrentRound.EndsAt.ToString() }, Lifetime.DataModification);
            }
            else
                throw new Exception($"Nemůžete spustit hru, která má status '{_roundService.State}'");
        }
        public Task PauseGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("Nejste administrátor");

            _roundService.Pause();
            return _commService.SendAll(context, "Pause", new { roundNumber = _roundService.CurrentRound.RoundNumber }, Lifetime.DataModification);
        }
        public Task FinishTimer(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("Nejste administrátor");

            _roundService.FinishTimer();
            return _commService.SendOne(context, context.CurrentPlayer.Id, "end_ok", new { }, Lifetime.Notification);
        }
        public Task RestartGame(ContextService context)
        {
            if (!context.CurrentPlayer.IsUser() || !(context.CurrentPlayer as User).IsAdmin)
                throw new Exception("Nejste administrátor");

            _roundService.ResetGame(context);
            return _commService.SendEach(context, "Restart", (cont) => new { map = cont.GetArmyService().GetInit() }, Lifetime.Notification);
        }
        public void HistoryBack() { }
        public void HistoryForward() { }
        public void HistoryNow() { }
    }
}