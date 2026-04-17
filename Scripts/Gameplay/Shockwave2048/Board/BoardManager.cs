using System.Linq;
using Cysharp.Threading.Tasks;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardManager : MonoBehaviour
    {
        [Inject] private GameConfig _gameConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private BoardView _boardView;
        [Inject] private BoardElementPoolService _elementPool;
        [Inject] private BoardGridBuilder _boardGridBuilder;
        [Inject] private BoardShockwaveController _boardShockwaveController;
        
        private void Awake()
        {
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<GameEndedSignal>(OnGameFinished);
            _signalBus.Subscribe<GameReplaySignal>(OnGameReplay);
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);
        }
        public void OnDestroy()
        {
            _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Unsubscribe<GameEndedSignal>(OnGameFinished);
            _signalBus.Unsubscribe<GameReplaySignal>(OnGameReplay);
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
        }

        public async UniTask Init()
        {
            _state.GridSize = new Vector2Int(5, 5);
            
            await _boardGridBuilder.CreateSlotsAndElements();
            _boardView.Init();
        }

        private void OnGameStarted()
        {
            _state.MergeStep = 1;
        }
        private void OnGameFinished()
        {
        }
        private void OnGameReplay()
        {
            ClearLowestElements();
        }
        private void OnPlayerTurn()
        {
            if (_state.MergeStep >= _gameConfig.MergesNeededForChain) _signalBus.Fire(new MergeChainCompletedSignal(_state.MergeStep));
            
            _state.MergeStep = 1;
        }
        
        public bool NoPlayableSteps() => 
            _state.CellStates
                .Where(kvp => kvp.Value.Slot.GetActive())
                .All(kvp => kvp.Value.Element != null && 
                            kvp.Value.Element.GetElementType() != _state.NextElementData.Value.ElementTypeInfo.ElementType);
        
        internal void ClearGrid()
        {
            DebugManager.Log(DebugCategory.Gameplay, "Clearing entire grid");

            foreach (var kvp in _state.CellStates)
            {
                if (kvp.Value.Element != null) _elementPool.Pool.Release(kvp.Value.Element);

                kvp.Value.Element = null;
                kvp.Value.Slot.ToggleActivation(false, false);
            }
        }

        private void ClearLowestElements()
        {
            var types = _state.CellStates
                .Where(kvp => kvp.Value.Element != null)
                .Select(kvp => kvp.Value.Element.GetElementType())
                .Distinct()
                .OrderBy(t => (int)t)
                .ToList();

            foreach (var kvp in _state.CellStates)
            {
                var element = kvp.Value.Element;
                if (element == null) continue;

                if (element.GetElementType() == types[0] || element.GetElementType() == types[1])
                {
                    _elementPool.Pool.Release(element);
                    kvp.Value.Element = null;
                }
            }
            
            _signalBus.Fire(new GameTurnSignal());
        }
    }
}