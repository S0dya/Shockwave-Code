using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Enums;
using Gameplay.Shockwave2048.Grid;
using Gameplay.Shockwave2048.Slot;
using NaughtyAttributes;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using UniRx;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public enum GridOpeningPattern
    {
        Random,
        Spiral,
        CenterSpecific,
        CenterRandom,
    }
    
    public class BoardGridController : IInitializable, IDisposable
    {
        [Inject] private GameConfig _gameConfig;
        [Inject] private BoardManager _boardManager;
        [Inject] private SignalBus _signalBus;
        [Inject] private IGridOpenPattern _gridOpenPattern;
        [Inject] private BoardState _state;

        private int _currentOpenedAmount;
        
        private Stack<Vector2Int[]> _openedSlots = new();
        
        private List<Vector2Int> _currentlyOpeningSlots = new();

        private readonly CompositeDisposable _disposables = new ();
        
        public void Initialize()
        {
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Subscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Subscribe<UndoTurnSignal>(OnUndoTurn);

            _state.HighestElementType
                .Subscribe(OpenSlotsOnNewHighestType)
                .AddTo(_disposables);
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Unsubscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Unsubscribe<UndoTurnSignal>(OnUndoTurn);
            
            _disposables.Dispose();
        }

        public async UniTask Init()
        {
            //set the _elementTypesSlots based on the _boardManager.GridCount
            
            await Rebuild();
        }

        private void OnGameTurn()
        {
            _openedSlots.Push(_currentlyOpeningSlots.ToArray());
            _currentlyOpeningSlots.Clear();
        }
        private void OnPlayerTurn()
        {
            
        }
        private void OnUndoTurn()
        {
            if (_openedSlots.Count == 0) return;
            
            foreach (var pos in _openedSlots.Pop())
            {
                if (_state.CellStates.TryGetValue(pos, out var cell))
                {
                    cell.Slot.ToggleActivation(false, false);
                    _currentOpenedAmount--;
                }
            }
        }

        [Button]
        private async UniTask Rebuild()
        {
            foreach (var kvp in _state.CellStates) kvp.Value.Slot.ToggleActivation(false, false);
            
            _currentOpenedAmount = 0;

            await OpenNewSlots(_gameConfig.InitialActiveSlots);
            _currentlyOpeningSlots.Clear();
        }
        
        private void OpenSlotsOnNewHighestType(ElementType elementType)
        {
            if (_gameConfig.ElementTypesSlots.TryGetValue(elementType, out var amount))
            {
                OpenNewSlots(amount).Forget();
            }
        }

        private async UniTask OpenNewSlots(int amount)
        {
            var gridSlots = new Dictionary<Vector2Int, GridSlot>();
            
            foreach (var kvp in _state.CellStates) gridSlots.Add(kvp.Key, kvp.Value.Slot);

            amount = Math.Min(_state.GridCount - _currentOpenedAmount, amount);
            _currentOpenedAmount += amount;

            if (amount == 0)
            {
                DebugManager.Log(DebugCategory.Gameplay, $"Cant open new slots - reached max", LogType.Warning);
                return;
            } 
                
            DebugManager.Log(DebugCategory.Gameplay, $"Opening {amount} new grid slots");
            
            _currentlyOpeningSlots = await _gridOpenPattern.OpenSlots(gridSlots, amount, _gameConfig.SlotsOpeningDelay);
        }
    }
}