using Audio;
using Common.Profile;
using Game.States;
using Zenject;

namespace Game.Controllers {
    public class StateController : IStateController {
        [Inject] private IGameController _gameController;
        [Inject] private ILevelController _levelController;
        [Inject] private IAnimationsController _animationsController;
        [Inject] private ITimeController _timeController;
        [Inject] private IProfileManager _profileManager;
        [Inject] private IAudioManager _audioManager;
        
        public void GoToState(IGameState state) {
            GoToState(state, new StateSettings {
                AnimationsController = _animationsController,
                LevelController = _levelController
            });
        }

        public void GoToState(IGameState state, StateSettings settings) {
            settings.ProfileManager ??= _profileManager;
            settings.GameController ??= _gameController;
            settings.LevelController ??= _levelController;
            settings.AudioManager ??= _audioManager;
            settings.TimeController ??= _timeController;
            settings.AnimationsController ??= _animationsController;
            state.Process(settings);
        }
    }
}