using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Models;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils;
using Fort.Utils.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fort.Module
{
    public class RoundService
    {
        public RoundService()
        {
            _config = new Config();
            _timer = new Timer();
            _cancelRound = new System.Threading.CancellationTokenSource();
            State = Status.None;
        }
        public Status State { get; private set; }
        public CurrentRound CurrentRound { get; private set; }
        private System.Threading.CancellationTokenSource _cancelRound;
        private Task _gameTask;
        private Timer _timer;
        private Config _config;
        public void Init(ContextService context, IConfigurationSection config)
        {
            // load config
            config.Bind(_config);

            // create game
            if (_config.StartMode == StartMode.NewGame)
                ResetGame(context);

            // init round
            else if (_config.StartMode == StartMode.NewRound)
                InitRound(context);
            else
                CurrentRound = context.Database.Rounds.OrderByDescending(r => r.Id).First();
        }
        public async Task StartGame()
        {
            _cancelRound = new System.Threading.CancellationTokenSource();
            await StartRound();

            _gameTask = Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        // round running
                        await _timer.NewStart(CurrentRound.EndsAt.Value - DateTime.UtcNow);
                        if (_cancelRound.Token.IsCancellationRequested)
                            return;

                        await EndRound();
                        if (_cancelRound.Token.IsCancellationRequested)
                            return;

                        await ShowResult();
                        if (_cancelRound.Token.IsCancellationRequested)
                            return;

                        using (var context = new ContextService())
                        {
                            // end
                            if (await context.Database.Teams.CountAsync(t => t.Members.Any(m => m.Cities.Any())) == 1)
                                break;

                            InitRound(context);
                        }

                        await StartRound();
                    }
                    EndGame();
                }
                catch (Exception ex)
                {
                    Logger.Log(ELogLevel.Fatal, "round", ex.Message, ex.StackTrace);
                }
            });
        }
        public void ResetGame(ContextService context)
        {
            // reset
            _cancelRound.Cancel();
            _timer.End();
            _timer = new Timer();
            CurrentRound = null;

            // load start
            foreach (City city in context.Database.Cities.Include(c => c.Start))
            {
                // city has owner
                var start = city.Start.FirstOrDefault();
                if (start != null)
                {
                    city.Army = start.Army ?? _config.DefaultPopulationStart;
                    city.OwnerId = start.UserId;
                }
                // city is neutral
                else
                {
                    city.Army = _config.Neutral_MinArmy + (new Random().Next() % (_config.Neutral_MaxArmy - _config.Neutral_MinArmy));
                    city.OwnerId = null;
                }
            }

            context.Database.SaveChanges();

            // TODO: send to users

            InitRound(context);
        }
        public void Pause()
        {
            _timer.Pause();
            State = Status.Paused;
        }
        public void Resume()
        {
            _timer.Resume();
            State = Status.Running;
        }
        public void FinishTimer()
        {
            _timer.End();
        }

        private void InitRound(ContextService context)
        {
            int currentRoundNumber = CurrentRound?.RoundNumber + 1 ?? 1;
            var newRound = new Round
            {
                RoundNumber = currentRoundNumber,
                EndsAt = null
            };

            context.Database.Rounds.Add(newRound);
            context.Database.SaveChanges();

            CurrentRound = newRound;
            State = Status.New;
            // TODO
            // _commService.SendToAll("InitRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }
        private async Task StartRound()
        {
            State = Status.Starting;
            using (var context = new ContextService())
            {
                var round = context.Database.Rounds.Find(CurrentRound.Id);
                round.StartsAt = DateTime.UtcNow;
                round.EndsAt = _config.GetNextRoundEnd();
                await context.Database.SaveChangesAsync();
                CurrentRound = round;
            }

            // TODO
            // _commService.SendToAll("StartRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });

            // set round duration
            State = Status.Running;
        }
        private async Task EndRound()
        {
            State = Status.Ending;
            using (var context = new ContextService())
            {
                var round = await context.Database.Rounds.FindAsync(CurrentRound.Id);
                round.EndsAt = DateTime.UtcNow;
                await context.Database.SaveChangesAsync();
                CurrentRound = round;
                CountResult(context);
            }

            State = Status.Ended;
            // TODO
            // _commService.SendToAll("EndRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });

            await _timer.NewStart(_config.BreateOutBreakTime); // breathe out
        }
        private async Task ShowResult()
        {
            State = Status.ShowingResult;

            await _timer.NewStart(_config.AnimationTime);
        }
        private void EndGame()
        {

            // _commService.SendToAll("map_show", new { });
            // List<Task> tasks = new List<Task>();

            // foreach (string story in _currentDeaths)
            //     tasks.Add(Task.Run(() => _commService.SendToAll("notification", new { type = "death", message = story })));
            // _currentDeaths.Clear();

            // // show statistics to admins
            // foreach (User user in _context.Users.Where(u => u.IsAdmin).ToList())
            // {
            //     tasks.Add(Task.Run(() => _commService.SendToOne(user.Id, "statistics", MapBaseService.GetMapServiceForPlayer(_context, user).ShowStatistics())));
            // }

            // foreach (Task task in tasks)
            //     task.GetAwaiter().GetResult();
        }
        public void CountResult(ContextService context)
        {
            var turns = context.Database.Turns.Include(t => t.User).Where(t => t.RoundId == CurrentRound.Id).ToList();
            var cities = context.Database.Cities.ToList();

            /// walk out
            foreach (City city in cities)
            {
                city.Army -= turns.Where(t => t.SourceCityId == city.Id).Sum(t => t.Amount);
            }
            // print
            // _commService.SendToEach("map_walkOut", (playerId) =>
            // {
            //     Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
            //     return MapBaseService.GetMapServiceForPlayer(_context, player).Print(false);
            // });

            /// fights in the middle
            foreach (Turn turn in turns)
            {
                // same path, different teams
                Turn second = turns.FirstOrDefault(t => t.SourceCityId == turn.TargetCityId && t.TargetCityId == turn.SourceCityId && t.User.TeamId != turn.User.TeamId);
                if (second != null)
                {
                    (int finalArmy, var winner) = fight(turn, second);

                    turn.ModifiedAmount = (winner == turn)
                        ? finalArmy
                        : 0;
                }
                // no turn on same path
                else
                {
                    turn.ModifiedAmount = turn.Amount;
                }
            }
            // _commService.SendToEach("turns", playerId =>
            // {
            //     Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
            //     MapBaseService mapService = MapBaseService.GetMapServiceForPlayer(_context, player);
            //     List<string> result = new List<string>();
            //     foreach (Turn turn in turns)
            //         result.AddRange(mapService.Army(turn));

            //     return JsonConvert.SerializeObject(result);
            // });

            /// walk in
            foreach (City city in cities)
            {
                var incoming = turns.Where(t => t.TargetCityId == city.Id).GroupBy(t => t.User.Team).ToList();

                if (incoming.Any())
                {
                    var enemyArmies = incoming.Where(gr => gr.Key != city.Owner?.Team).ToList();

                    // fights before gates
                    if (enemyArmies.Any() && enemyArmies.Sum(a => a.Sum(t => t.ModifiedAmount ?? t.Amount)) > 0)
                    {
                        (int winnerArmy, IGrouping<Team, Turn> winnerFightBeforeGates) = fight(enemyArmies.ToArray());

                        // fights for city
                        var fightForCityWinner = fight(city, incoming.SingleOrDefault(gr => gr.Key == city.Owner?.Team), winnerFightBeforeGates, winnerArmy);

                        city.Army = fightForCityWinner.army;
                        // owner changed
                        if (fightForCityWinner.winnerId != city.OwnerId)
                        {
                            // last players city
                            if (city.Owner?.Cities.Count() == 1)
                                PlayerDeath(city.Owner);

                            city.Owner = winnerFightBeforeGates.OrderBy(t => t.CreatedAt).First().User;
                        }
                        city.Owner = fightForCityWinner.winnerId == city.OwnerId
                            ? city.Owner
                            : winnerFightBeforeGates.OrderBy(t => t.CreatedAt).First().User;
                    }
                    // no fight, just ally
                    else
                    {
                        city.Army += incoming.First().Sum(t => t.ModifiedAmount ?? t.Amount);
                    }
                }

                // grow
                if (city.Owner != null)
                    city.Army += city.Grow ?? city.Owner.Team.PopulationGrowth ?? _config.DefaultPopulationGrowth;
            }
            context.Database.SaveChanges();
            // print
            // _commService.SendToEach("map_walkIn", (playerId) =>
            // {
            //     Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
            //     return MapBaseService.GetMapServiceForPlayer(_context, player).Print(false);
            // });
        }
        private (int army, string winnerId) fight(City city, IGrouping<Team, Turn> ally, IGrouping<Team, Turn> attacker, int attackerRealArmy)
        {
            var defender = (city.Army + (ally?.Sum(a => a.ModifiedAmount ?? a.Amount) ?? 0), city.Owner?.Team.ArmyStrengthCoef ?? 1, city.OwnerId);
            var result = fight(defender, (attackerRealArmy, attacker.Key.ArmyStrengthCoef, attacker.Key.Id));

            return result;
        }
        private (int army, Turn winner) fight(params Turn[] sides)
        {
            var result = fight(sides.Select(s => (s.Amount, s.User.Team.ArmyStrengthCoef, s.UserId)).ToArray());

            return (result.army, sides.FirstOrDefault(s => s.UserId == result.winnerId));
        }
        private (int army, IGrouping<Team, Turn> winner) fight(params IGrouping<Team, Turn>[] sides)
        {
            var result = fight(sides.Select(s => (s.Sum(t => t.ModifiedAmount ?? t.Amount), s.Key.ArmyStrengthCoef, s.Key.Id)).ToArray());

            return (result.army, sides.FirstOrDefault(s => s.Key.Id == result.winnerId));
        }
        private (int army, string winnerId) fight(params (int army, double coef, string id)[] sides)
        {
            if (sides.Count() == 0)
                return (0, null);
            if (sides.Count() == 1)
                return (sides[0].army, sides[0].id);

            var orderedSides = sides.OrderByDescending(s => s.army * s.coef);
            var winner = orderedSides.First();
            var second = orderedSides.ElementAt(1);

            if (winner.army * winner.coef == second.army * second.coef)
                return (0, null);

            return (winner.army - (int)(second.army * second.coef / winner.coef), winner.id);
        }
        private void PlayerDeath(User user)
        {
            string story = _config.GetRandomStory(user.Name);

            // TODO: sendAll
        }

        public enum StartMode
        {
            NewGame,
            NewRound,
            ContinueLastRound
        }

        public enum Status
        {
            None,
            New,
            Starting,
            Running,
            Paused,
            Ending,
            Ended,
            ShowingResult
        }

        private class Config
        {
            public StartMode StartMode { get; set; }
            public int DefaultPopulationStart { get; set; }
            public int DefaultPopulationGrowth { get; set; }
            public int Neutral_MinArmy { get; set; }
            public int Neutral_MaxArmy { get; set; }
            public TimeSpan RoundEnd { get; set; }
            public TimeSpan BreateOutBreakTime { get; set; }
            public TimeSpan AnimationTime { get; set; }
            public List<string> DeathStories { get; set; }

            public DateTime GetNextRoundEnd()
            {
                var result = DateTime.UtcNow.Date + RoundEnd;
                if (result < DateTime.UtcNow)
                    result.AddDays(1);

                return result;
            }
            public string GetRandomStory(string userName)
            {
                return DeathStories[new Random().Next() % DeathStories.Count()].Replace("{playerName}", userName);
            }
        }
    }
}