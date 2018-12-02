using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils;
using Fort.Utils.Logger;
using Microsoft.Extensions.Configuration;

namespace Fort.Services
{
    public class RoundService : IDisposable
    {
        public RoundService()
        {
            _context = new FortDbContext();
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
                    _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultRoundDurationSec));
                    Start();
                    await _timer.Start();

                    _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultBeforeVisualizationSec));
                    await End();
                    await _timer.Start();

                    await Finalize();

                    _timer.SetTime(TimeSpan.FromSeconds(Program.Config.DefaultAfterVisualizationSec));
                    Init(_timer.Remains.Value);
                    await _timer.Start();
                }
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
            Init();
        }

        public void Init(TimeSpan? duration = null)
        {
            _timer = new Timer();

            int currentRoundNumber = CurrentRound?.RoundNumber + 1 ?? 1;
            CurrentRound = new Round
            {
                RoundNumber = currentRoundNumber,
                EndsAt = null
            };

            _context.Rounds.Add(CurrentRound);
            _context.SaveChanges();
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
            await Task.Run(() => { });
#warning TODO: compute rounds, show to user
        }
        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}