using System;
using System.Collections.Generic;
using System.Linq;
using Career;
using Common.Managers;
using Extensions;
using Game.Config;
using Game.Data;
using Game.Objects;
using Garage;
using UniRx;
using Zenject;

namespace Game.Controllers {
    public class RaceGoalsController : IRaceGoalsController {
        [Inject] private ICarController _carController;
        [Inject] private IGameManager _gameManager;
        [Inject] private IGameController _gameController;
        [Inject] private ILevelController _levelController;
        [Inject] private IGarageManager _garageManager;
        [Inject] private ICareerManager _careerManager;

        private Car _car;

        public event Action<RaceGoal, double> GoalUpdated;
        public List<Flag> CompletedGoals { get; } = new();

        public void Initialize() {
            if (_carController.Car) {
                CarReady(default);
            }
            else {
                _carController.CarReady.Subscribe(CarReady);
            }

            _gameController.Finished.Subscribe(OnRaceFinished);
        }

        public double GetValue(Flag flag) {
            return flag.Goal switch {
                RaceGoal.NitroTime => _car.NitroTime.Value,
                RaceGoal.PerfectNitro => _car.PerfectNitros.Value,
                RaceGoal.PerformSpin => _car.Spins.Value,
                RaceGoal.ShockWaves => _car.Shockwaves.Value,
                RaceGoal.BeatRaceTime => TimeSpan.FromMilliseconds(_car.RacingTime).TotalSeconds,
                RaceGoal.FinishInPosition => _car.Position,
                RaceGoal.WinTheRace => _car.Position,
                RaceGoal.PerformSpinInOneJump => _car.SpinsHistory.Any(s => s.Key >= flag.Value) ? flag.Value : 0,
                RaceGoal.NitroBottles => _car.NitroBottles.Value,
                RaceGoal.Coins => _gameController.CollectedCoinsAmount.Value,
                RaceGoal.UseCarWithStars => _garageManager.GetStars(_car.Config.Id),
                RaceGoal.UseRarityCar => (int) _garageManager.GetRarity(_car.Config.Id),
                _ => 0
            };
        }


        private void CarReady(Unit _) {
            _car = _carController.Car;
            _carController.Car.Spins.Skip(1).Subscribe(value => GoalUpdated?.Invoke(RaceGoal.PerformSpin, value));
            _carController.Car.SpinsHistory.ObserveAdd().Subscribe(value => GoalUpdated?.Invoke(RaceGoal.PerformSpinInOneJump, value.Key));
            _carController.Car.AirTime.Skip(1).Subscribe(value => GoalUpdated?.Invoke(RaceGoal.AirTime, value));
            _carController.Car.NitroTime.Skip(1).Subscribe(value => GoalUpdated?.Invoke(RaceGoal.NitroTime, value));
            _carController.Car.PerfectNitros.Skip(1).Subscribe(value => GoalUpdated?.Invoke(RaceGoal.PerfectNitro, value));
            _carController.Car.Shockwaves.Skip(1).Subscribe(value => GoalUpdated?.Invoke(RaceGoal.ShockWaves, value));
            _carController.Car.NitroBottles.Skip(1).Subscribe(value => GoalUpdated?.Invoke(RaceGoal.NitroBottles, value));
            _gameController.CollectedCoinsAmount.Skip(1).Subscribe(value => GoalUpdated?.Invoke(RaceGoal.Coins, value));
        }

        private void OnRaceFinished(Unit _) {
            if (_gameManager.GameplayType == GameplayType.Career) {
                _levelController.Config.AsSeasonLevel.Flags.Where(f => !_careerManager.IsFlagCompleted(_levelController.Config, f)).ForEach(f => {
                    if (CheckFlag(f)) {
                        CompletedGoals.Add(f);
                    }
                });
            }
            if (_gameManager.GameplayType == GameplayType.Event) {
                _gameManager.EventConfig.Missions.SelectMany(m => m.Flags).ForEach(f => {
                    if (CheckFlag(f)) {
                        CompletedGoals.Add(f);
                    }
                });
            }
            if (_gameManager.GameplayType == GameplayType.SpecialEvent) {
                _gameManager.SpecialEventStageConfig.Missions.SelectMany(m => m.Flags).ForEach(f => {
                    if (CheckFlag(f)) {
                        CompletedGoals.Add(f);
                    }
                });
            }
        }

        private bool CheckFlag(Flag flag) {
            return flag.Goal switch {
                RaceGoal.NitroTime => _car.NitroTime.Value >= flag.Value,
                RaceGoal.PerfectNitro => _car.PerfectNitros.Value >= flag.Value,
                RaceGoal.PerformSpin => _car.Spins.Value >= flag.Value,
                RaceGoal.ShockWaves => _car.Shockwaves.Value >= flag.Value,
                RaceGoal.BeatRaceTime => TimeSpan.FromMilliseconds(_car.RacingTime).TotalSeconds <= flag.Value,
                RaceGoal.FinishInPosition => _car.Position <= flag.Value,
                RaceGoal.FinishTheRace => _car.RacingTime > 1 && !_gameController.Started.Value,
                RaceGoal.WinTheRace => _car.Position == 1,
                RaceGoal.WinWithCar => _car.Position == 1 && _car.Config.Id == flag.Car.Id,
                RaceGoal.PerformSpinInOneJump => _car.SpinsHistory.Any(s => s.Key >= flag.Value),
                RaceGoal.NitroBottles => _car.NitroBottles.Value >= flag.Value,
                RaceGoal.Coins => _gameController.CollectedCoinsAmount.Value >= flag.Value,
                RaceGoal.UseCarWithStars => _garageManager.GetStars(_car.Config.Id) >= flag.Value,
                RaceGoal.UseRarityCar => _garageManager.GetRarity(_car.Config.Id) == (Rarity) flag.Value,
                _ => false
            };
        }
    }
}