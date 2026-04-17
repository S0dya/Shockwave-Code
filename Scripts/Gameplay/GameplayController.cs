using Gameplay.Shockwave2048.Board;
using Gameplay.Shockwave2048.MegaMerge;
using PT.GameplayAdditional.Input;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using PT.Tools.Windows;
using PT.UI.Windows;
using UnityEngine;
using Zenject;

namespace Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private RectTransform[] inputZoneUIs;
        [SerializeField] private Canvas canvas;
        
        [Inject] private GameConfig _gameConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardManager _boardManager;
        [Inject] private GameEndRewardWindow _gameEndRewardView;
        [Inject(Id = "Game")] private WindowsManager _windowsManager;
        [Inject] private InputManager _inputManager;
        [Inject] private BoardState _state;
        [Inject] private MegaMergeController _megaMergeController;

        private void Awake()
        {
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Subscribe<NextElementSetSignal>(OnNextElementSet);
            _signalBus.Subscribe<UndoTurnSignal>(OnUndoTurn);
            _signalBus.Subscribe<BoardActionPerformedSignal>(OnGameTurn);
            _signalBus.Subscribe<MegaMergeUsedSignal>(OnGameTurn);
        }

        public void TryUndoTurn()
        {
            if (!_state.IsPlayerTurn || _state.GameStep == 0) return;
            
            _signalBus.Fire(new UndoTurnSignal());
        }

        private void OnGameStarted()
        {
            _state.GameStep = 0;
            _signalBus.Fire(new PlayerTurnSignal());
        }

        private async void OnPlayerTurn()
        {
            _state.IsPlayerTurn = true;

            _state.GameStep++;
        }

        private void OnNextElementSet()
        {
            if (_boardManager.NoPlayableSteps() && !_megaMergeController.CanMegaMergeInAnyDirection())
            {
                GameOver();
            }
        }

        private void OnGameTurn()
        {
            DebugManager.Log(DebugCategory.Gameplay, "Starting Game Turn");
            
            _signalBus.Fire(new GameTurnSignal());
            
            _state.IsPlayerTurn = false;
        }

        private void OnUndoTurn()
        {
            _state.GameStep--;
        }
        
        private async void GameOver()
        {
            DebugManager.Log(DebugCategory.Gameplay, "GameOver");
            
            // _gameEndRewardView.SetCollected(_coinsEarnedThisGame);
            
            _signalBus.Fire(new GameEndedSignal());
            await _windowsManager.Open<GameOverReviveWindow>();
            // await _windowsManager.Open<GameEndRewardWindow>();
        }
        
        private void OnDestroy()
        {
            _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Unsubscribe<NextElementSetSignal>(OnNextElementSet);
            _signalBus.Unsubscribe<UndoTurnSignal>(OnUndoTurn);
            _signalBus.Unsubscribe<BoardActionPerformedSignal>(OnGameTurn);
            _signalBus.Unsubscribe<MegaMergeUsedSignal>(OnGameTurn);
        }
    }
}