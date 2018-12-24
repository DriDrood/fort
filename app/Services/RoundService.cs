using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils;
using Fort.Utils.Logger;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fort.Services
{
    public class RoundService : IDisposable
    {
        public RoundService()
        {
            _context = new FortDbContext();
            _timer = new Timer();
        }

        private FortDbContext _context;
        private CommService _commService => Program.GetService<CommService>();
        private Timer _timer;
        private Task _playTask;
        private bool _playTaskCanceled;

        private Dictionary<string, string> _startingPositions;

        public Round CurrentRound { get; private set; }
        public Timer.Status State => _timer.State;
        public TimeSpan? Remaining => _timer.Remains;

        #region  Setup
        public void Setup(IConfigurationSection config)
        {
            // load from config
            _startingPositions = new Dictionary<string, string>();
            config.Bind(_startingPositions);

            // load last round
            CurrentRound = _context.Rounds.OrderByDescending(r => r.Id).FirstOrDefault();
            if (CurrentRound == null)
            {
                // save to DB
                LoadStart();
                Init().GetAwaiter().GetResult();
            }
        }
        private void LoadStart()
        {
            foreach (City city in _context.Cities)
            {
                // city has owner
                if (_startingPositions.ContainsKey(city.Id.ToString()))
                {
                    city.Army = Program.Config.DefaultPopulationStart;
                    city.OwnerId = _startingPositions[city.Id.ToString()];
                }
                // city is neutral
                else
                {
                    city.Army = GetNeutralArmySize();
                    city.OwnerId = null;
                }
            }

            _context.SaveChanges();

            _playTaskCanceled = false;
        }
        private int GetNeutralArmySize()
        {
            Random rand = new Random();
            int min = Program.Config.NeutralCitiesPopulation["Min"];
            int max = Program.Config.NeutralCitiesPopulation["Max"];

            return
                min + (rand.Next() % (max - min));
        }
        #endregion

        #region Lifecycle
        public void StartGame()
        {
            _playTask = Task.Run(async () =>
            {
                while (_context.Teams.Count(t => t.Members.Any(m => m.Cities.Any())) > 1)
                {
                    if (CurrentRound.RoundNumber > 1 || CurrentRound.Turns.Any())
                    {
                        _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultAfterVisualizationSec));
                        await Init(_timer.Remains.Value);
                        await _timer.Start();
                        if (_playTaskCanceled) return;
                    }

                    _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultRoundDurationSec));
                    Start();
                    await _timer.Start();
                    if (_playTaskCanceled) return;

                    _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultBeforeVisualizationSec));
                    await End();
                    var tt = _timer.Start();

                    await Finalize();
                    await tt;
                    if (_playTaskCanceled) return;

                    await ShowFinalize();
                }

                EndGame();
            });
        }
        public void EndGame()
        {
#warning TODO: show result
        }
        public async Task ResetGame()
        {
            _playTaskCanceled = true;
            _timer.End();

            CurrentRound = null;
            _timer = new Timer();

            LoadStart();
            await Init();

            await _commService.SendToEach("Restart", playerId =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print();
            });
        }

        public async Task Init(TimeSpan? duration = null)
        {
            int currentRoundNumber = CurrentRound?.RoundNumber + 1 ?? 1;
            CurrentRound = new Round
            {
                RoundNumber = currentRoundNumber,
                EndsAt = null
            };

            _context.Rounds.Add(CurrentRound);
            _context.SaveChanges();

            await _commService.SendToAll("InitRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public void Start()
        {
            CurrentRound.StartsAt = DateTime.UtcNow;
            _context.SaveChanges();

            var startMessageTask = _commService.SendToAll("StartRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public async Task Pause()
        {
            _timer.Pause();
            await _commService.SendToAll("Pause", new { roundNumber = CurrentRound.RoundNumber });
        }

        public async Task Resume()
        {
            _timer.Resume();
            await _commService.SendToAll("Resume", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public void ForceEnd()
        {
            _timer.End();
        }

        public async Task End()
        {
            CurrentRound.EndsAt = DateTime.UtcNow;
            _context.SaveChanges();

            await _commService.SendToAll("EndRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public async Task Finalize()
        {
            // refresh context
            _context.Dispose();
            _context = new FortDbContext();

            var turns = _context.Turns.Where(t => t.RoundId == CurrentRound.Id).ToList();
            var cities = _context.Cities.ToList();

            /// walk out
            foreach (City city in cities)
            {
                city.Army -= turns.Where(t => t.SourceCityId == city.Id).Sum(t => t.Amount);
            }
            // print
            await _commService.SendToEach("map_walkOut", (playerId) =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print();
            });

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
            await _commService.SendToEach("turns", playerId =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                MapBaseService mapService = MapBaseService.GetMapServiceForPlayer(_context, player);
                List<string> result = new List<string>();
                foreach (Turn turn in turns)
                    result.AddRange(mapService.Army(turn));

                return JsonConvert.SerializeObject(result);
            });

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
                    city.Army += city.Grow ?? city.Owner.Team.PopulationGrowth ?? Program.Config.DefaultPopulationGrow;
            }
            _context.SaveChanges();
            // print
            await _commService.SendToEach("map_walkIn", (playerId) =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print();
            });
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

        public async Task ShowFinalize()
        {
            await _commService.SendToAll("map_show", new { });

            // show statistics to admins
            List<Task> tasks = new List<Task>();
            foreach (User user in _context.Users.Where(u => u.IsAdmin))
            {
                tasks.Add(_commService.SendToOne(user.Id, "statistics", MapBaseService.GetMapServiceForPlayer(_context, user).ShowStatistics()));
            }

            foreach (Task task in tasks)
                await task;
        }
        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}