using System;
using System.Collections.Generic;
using Extensions;
using Game.Controllers;
using Game.Objects;
using Game.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Racing {
    public class RacingHall : MonoBehaviour {
        [field: SerializeField] public DestinationArrow DestinationArrow { get; private set; }
        [field: SerializeField] public Parking Parking { get; private set; }
        [SerializeField] private ActionContainer _actionContainer;
        [SerializeField] private List<Race> _races;

        [Inject] private IPlayerStateController _playerStateController;
        [Inject] private IGameController _gameController;

        public ReactiveCommand OnPlay { get; } = new();
        public ReactiveCommand<int> OnRacePlayed { get; } = new();

        public Race NextRace {
            get {
                if (_gameController.CurrentRace >= _races.Count) {
                    return null;
                }

                return _races[_gameController.CurrentRace];
            }
        }

        public void PlayNext() {
            var race = _races[_gameController.CurrentRace];
            race.Play();
            race.OnFinish.First().Subscribe(position => {
                if (position == 1) {
                    _gameController.RaceCompleted();
                }
                OnFinish(position);
            }).AddTo(this);
            OnPlay.Execute();
        }

        private void OnFinish(int position) {
            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                var car = _playerStateController.CurrentCar;
                car.SetPosition(Parking.transform.position, new Vector3(0, 180, 0));
                OnRacePlayed.Execute(position);
            }).AddTo(this);
        }

        private void Start() {
            _actionContainer.OnInvoke.Subscribe(_ => PlayNext()).AddTo(this);
        }
    }
}