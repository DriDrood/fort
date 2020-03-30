using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils;
using Fort.Utils.Logger;
using Microsoft.EntityFrameworkCore;
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
            _rand = new Random();
            _currentDeaths = new List<string>();
        }

        private List<string> _deathStories;
        private Random _rand;
        private List<string> _currentDeaths;
        private FortDbContext _context;
        private CommService _commService => Program.GetService<CommService>();
        private Timer _timer;
        private Task _playTask;
        private bool _playTaskCanceled;

        public Round CurrentRound { get; private set; }
        public Timer.Status State => _timer.State;
        public TimeSpan? Remaining => _timer.Remains;

        #region  Setup
        public void Setup(IConfigurationSection config)
        {
            _deathStories = Program.GetService<IConfiguration>().GetSection("DeathStories").Get<List<string>>() ?? new List<string>();

            // load last round
            CurrentRound = _context.Rounds.OrderByDescending(r => r.Id).FirstOrDefault();
            if (CurrentRound == null)
            {
                // save to DB
                LoadStart();
                Init();
            }
        }
        private void LoadStart()
        {
            foreach (City city in _context.Cities.Include(c => c.Start))
            {
                // city has owner
                var start = city.Start.FirstOrDefault();
                if (start != null)
                {
                    city.Army = start.Army ?? Program.Config.DefaultPopulationStart;
                    city.OwnerId = start.UserId;
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
            _playTask = Task.Run(() =>
            {
                try
                {
                    while (_context.Teams.Count(t => t.Members.Any(m => m.Cities.Any())) > 1)
                    {
                        if (CurrentRound.RoundNumber > 1 || CurrentRound.Turns.Any())
                        {
                            _timer.SetTime(TimeSpan.FromSeconds(Program.Config.AfterVisualizationSec));
                            Init(_timer.Remains.Value);
                            _timer.Start().GetAwaiter().GetResult();
                            if (_playTaskCanceled) return;
                        }

                        var roundDuration = GetRoundDuration();
                        _timer.SetTime(roundDuration);
                        Start();
                        _timer.Start().GetAwaiter().GetResult();
                        if (_playTaskCanceled) return;

                        _timer.SetTime(TimeSpan.FromSeconds(Program.Config.BeforeVisualizationSec));
                        End();
                        var tt = _timer.Start();

                        CountResult();
                        tt.GetAwaiter().GetResult();
                        if (_playTaskCanceled) return;

                        ShowFinalize();
                    }

                    EndGame();
                }
                catch (Exception ex)
                {
                    Logger.Log(ELogLevel.Fatal, "round", ex.Message, ex.StackTrace);
                }
            });
        }
        public void EndGame()
        {
#warning TODO: show result
        }
        public void ResetGame()
        {
            _playTaskCanceled = true;
            _timer.End();

            CurrentRound = null;
            _timer = new Timer();

            LoadStart();
            Init();

            _commService.SendToEach("Restart", playerId =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print();
            });
        }

        public void Init(TimeSpan? duration = null)
        {
            int currentRoundNumber = CurrentRound?.RoundNumber + 1 ?? 1;
            CurrentRound = new Round
            {
                RoundNumber = currentRoundNumber,
                EndsAt = null
            };

            _context.Rounds.Add(CurrentRound);
            _context.SaveChanges();

            _commService.SendToAll("InitRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public void Start()
        {
            CurrentRound.StartsAt = DateTime.UtcNow;
            _context.SaveChanges();

            _commService.SendToAll("StartRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public void Pause()
        {
            _timer.Pause();
            _commService.SendToAll("Pause", new { roundNumber = CurrentRound.RoundNumber });
        }

        public void Resume()
        {
            _timer.Resume();
            _commService.SendToAll("Resume", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public void ForceEnd()
        {
            _timer.End();
        }

        public void End()
        {
            CurrentRound.EndsAt = DateTime.UtcNow;
            _context.SaveChanges();

            _commService.SendToAll("EndRound", new { duration = (int)_timer.Remains.Value.TotalSeconds, roundNumber = CurrentRound.RoundNumber });
        }

        public void CountResult()
        {
            // refresh context
            _context.SaveChanges();
            _context.Dispose();
            _context = new FortDbContext();
            CurrentRound = _context.Rounds.Find(CurrentRound.Id);

            var turns = _context.Turns.Include(t => t.User).Where(t => t.RoundId == CurrentRound.Id).ToList();
            var cities = _context.Cities.ToList();

            /// walk out
            foreach (City city in cities)
            {
                city.Army -= turns.Where(t => t.SourceCityId == city.Id).Sum(t => t.Amount);
            }
            // print
            _commService.SendToEach("map_walkOut", (playerId) =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print(false);
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
            _commService.SendToEach("turns", playerId =>
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
                    city.Army += city.Grow ?? city.Owner.Team.PopulationGrowth ?? Program.Config.DefaultPopulationGrow;
            }
            _context.SaveChanges();
            // print
            _commService.SendToEach("map_walkIn", (playerId) =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print(false);
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
        private void PlayerDeath(User user)
        {
            string story = _deathStories[_rand.Next() % _deathStories.Count].Replace("{playerName}", user.Name);
            _currentDeaths.Add(story);
        }

        public void ShowFinalize()
        {
            _commService.SendToAll("map_show", new { });
            List<Task> tasks = new List<Task>();

            foreach (string story in _currentDeaths)
                tasks.Add(Task.Run(() => _commService.SendToAll("notification", new { type = "death", message = story })));
            _currentDeaths.Clear();

            // show statistics to admins
            foreach (User user in _context.Users.Where(u => u.IsAdmin).ToList())
            {
                tasks.Add(Task.Run(() => _commService.SendToOne(user.Id, "statistics", MapBaseService.GetMapServiceForPlayer(_context, user).ShowStatistics())));
            }

            foreach (Task task in tasks)
                task.GetAwaiter().GetResult();
        }

        private TimeSpan GetRoundDuration()
        {
            // by duration
            if (Program.Config.RoundDurationSec.HasValue)
                return TimeSpan.FromSeconds(Program.Config.RoundDurationSec.Value);

            // by round end
            var time = TimeSpan.Parse(Program.Config.RoundEndsAt);

            TimeZoneInfo timezonePrague = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.Id == "Europe/Prague")
                ?? TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            var cestNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezonePrague);
            var endRound = cestNow.Date + time;
            if (endRound < cestNow)
                endRound = endRound.AddDays(1);
            var roundDuration = endRound - cestNow; // CEST
            return roundDuration;
        }
        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}