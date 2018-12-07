using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils;
using Fort.Utils.Logger;
using Microsoft.Extensions.Configuration;
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

            // save to DB
            LoadStart();
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
                    }

                    _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultRoundDurationSec));
                    Start();
                    await _timer.Start();

                    _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultBeforeVisualizationSec));
                    await End();
                    var tt = _timer.Start();

                    await Finalize();
                    await tt;

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
            CurrentRound = null;
            _timer = new Timer();
            await _commService.SendToAll("Reset", new { });

            LoadStart();
            await Init();
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
            var turns = _context.Turns.Where(t => t.RoundId == CurrentRound.Id).OrderBy(t => t.CreatedAt).ToList();

            /// walk out
            foreach (Turn turn in turns)
            {
                turn.SourceCity.Army -= turn.Amount;
            }
            // print
            await _commService.SendToEach("map_walkOut", (playerId) =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print();
            });

            /// fights in the middle
            JArray jTurns = new JArray();
            foreach (Turn turn in turns)
            {
                Turn second = turns.FirstOrDefault(t => t.SourceCityId == turn.TargetCityId && t.TargetCityId == turn.SourceCityId);
                // army destroyed
                if (second != null && turn.Amount < second.Amount)
                {
                    var middle = MapBaseService.GetMiddlePoint(turn.SourceCity.X, turn.SourceCity.Y, turn.TargetCity.X, turn.TargetCity.Y);
                    jTurns.Add(new JObject
                    {
                        { "sourceX", turn.SourceCity.X },
                        { "sourceY", turn.SourceCity.Y },
                        { "targetX", middle.x },
                        { "targetY", middle.y },
                        { "amount", turn.Amount },
                        { "isHalfWay", true }
                    });
                }
                // no turn on same path || bigger army
                else
                {
                    jTurns.Add(new JObject
                    {
                        { "sourceX", turn.SourceCity.X },
                        { "sourceY", turn.SourceCity.Y },
                        { "targetX", turn.TargetCity.X },
                        { "targetY", turn.TargetCity.Y },
                        { "amount", turn.Amount },
                        { "isHalfWay", false }
                    });
                }
            }
            await _commService.SendToAll("turns", jTurns);

            /// walk in
            foreach (Turn turn in turns)
            {
                // ally
                if (turn.TargetCity.Owner?.TeamId == turn.SourceCity.Owner.TeamId)
                    turn.TargetCity.Army += turn.Amount;
                // enenmy
                else
                    turn.TargetCity.Army -= turn.Amount;
            }
            _context.SaveChanges();
            /// city
            foreach (City city in _context.Cities)
            {
                // change owner
                if (city.Army == 0)
                    city.OwnerId = null;
                else if (city.Army < 0)
                {
                    city.Owner = turns.First(t => t.TargetCityId == city.Id && t.SourceCity.Owner.TeamId != city.Owner?.TeamId).SourceCity.Owner;
                    city.Army = -city.Army;
                }

                // grow
                if (city.Owner != null)
                    city.Army += city.Grow ?? Program.Config.DefaultPopulationGrow;
            }
            _context.SaveChanges();
            // print
            await _commService.SendToEach("map_walkIn", (playerId) =>
            {
                Player player = (Player)_context.Users.Find(playerId) ?? _context.Teams.Find(playerId);
                return MapBaseService.GetMapServiceForPlayer(_context, player).Print();
            });
        }
        public async Task ShowFinalize()
        {
            await _commService.SendToAll("map_show", new { });
        }
        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}