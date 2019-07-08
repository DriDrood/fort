using System;
using System.Threading.Tasks;
using Fort;
using Fort.Database.Entities;
using Fort.Module.Army;
using Fort.Module.Comm;
using Fort.Utils.Logger;
using Newtonsoft.Json.Linq;

namespace Fort.Module
{
    public class ActionService
    {
        public ActionService(CommService commService, RoundService roundService, PlayerService playerService)
        {
            _roundService = roundService;
            _playerService = playerService;
            _commService = commService;
        }
        public void Init()
        {
            _commService.OnMessageEvent += HandleMessage;
        }

        private RoundService _roundService;
        private PlayerService _playerService;
        private CommService _commService;

        private async void HandleMessage(ContextService context, string method, JToken data)
        {
            string playerId = context.CurrentPlayer.Id;
            try
            {
                switch (method)
                {
                    case "play":
                        await StartOrResumeGame(context);
                        break;
                    case "pause":
                        await PauseGame(context);
                        break;
                    case "end":
                        await FinishTimer(context);
                        break;
                    case "restart":
                        await RestartGame(context);
                        break;
                    case "playerReady":
                        await PlayerReady(context, data["ready"].Value<bool>());
                        break;
                    case "turn":
                        int source = data["sourceCityId"].Value<int>();
                        int target = data["targetCityId"].Value<int>();
                        int amount = data["amount"].Value<int>();

                        await PlayerTurn(context, source, target, amount);
                        break;
                    case "jsError":
                        Logger.Log(ELogLevel.JS, context.CurrentPlayer.Id, data["message"].Value<string>(), $"{data["url"]} - line: {data["line"].Value<int>()}");
                        break;
                    default:
                        throw new FortException(ELogLevel.Warning, "Neznámá metoda");
                }
            }
            catch (FortException ex)
            {
                Logger.Log(ex.LogLevel, playerId, ex.Message, ex.StackTrace);
                await _commService.SendOne(context, playerId, "notification", new { type = ex.LogLevel.ToString().ToLower(), message = ex.Message }, Lifetime.Notification);
            }
            catch (Exception ex)
            {
                Logger.Log(ELogLevel.UnknownException, playerId, ex.Message, ex.StackTrace);
                await _commService.SendOne(context, playerId, "notification", new { type = "unknownexception", message = "Programátor to rozbil" }, Lifetime.Notification);
            }
        }

        public async Task PlayerTurn(ContextService context, int sourceCityId, int targetCityId, int amount)
        {
            if (context.Database.Cities.Find(sourceCityId).OwnerId != context.CurrentPlayer.Id)
                throw new FortException(ELogLevel.Warning, "Toto město není vaše");

            if (_roundService.State != RoundService.Status.Running && _roundService.State != RoundService.Status.Paused)
                throw new FortException(ELogLevel.Warning, "Kolo neběží");

            await context.ArmyService.PlayerTurn((User)context.CurrentPlayer, sourceCityId, targetCityId, amount, _roundService.CurrentRound?.Id ?? -1);
            await _commService.SendOne(context, context.CurrentPlayer.Id, "turnOk", new { sourceCityId, targetCityId, amount }, Lifetime.DataModification);
            await _commService.SendToAdmins(context, "turn", new { sourceCityId, targetCityId, amount }, Lifetime.DataModification);
        }
        public async Task PlayerReady(ContextService context, bool setReady)
        {
            if (_roundService.CurrentRound == null)
                throw new FortException(ELogLevel.Warning, "Kolo neběží");

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
            return _commService.SendEach(context, "Restart", (cont) => new { map = cont.ArmyService.GetInit(_roundService.CurrentRound.Id) }, Lifetime.Notification);
        }
        public void HistoryBack() { }
        public void HistoryForward() { }
        public void HistoryNow() { }
    }
}