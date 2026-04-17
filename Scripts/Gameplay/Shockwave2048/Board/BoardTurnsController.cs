using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Enums;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardTurnsController : IInitializable, IDisposable
    {
        class CellSnapshot
        {
            public bool IsActive { get; private set; }
            public ElementData ElementData { get; private set; }

            public CellSnapshot(bool isActive, ElementData elementData)
            {
                IsActive = isActive;
                ElementData = elementData;
            }
        }
        class BoardSnapshot
        {
            public Dictionary<Vector2Int, CellSnapshot> Cells { get; private set; }
            public ElementType HighestElementType { get; private set; }
            public ElementData NextElementData { get; private set; }

            public BoardSnapshot(Dictionary<Vector2Int, CellSnapshot> cells, ElementType highestElementType, ElementData nextElementData)
            {
                Cells = cells;
                HighestElementType = highestElementType;
                NextElementData = nextElementData;
            }
        }
        
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private BoardManager _boardManager;
        [Inject] private BoardMoveMergeController _boardMoveMergeController;
        
        private Stack<BoardSnapshot> _cellsSteps = new();

        public void Initialize()
        {
            _cellsSteps.Clear();
            
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<GameTurnSignal>(RecordCurrentState);
            _signalBus.Subscribe<UndoTurnSignal>(OnUndo);
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Unsubscribe<GameTurnSignal>(RecordCurrentState);
            _signalBus.Unsubscribe<UndoTurnSignal>(OnUndo);
        }
        
        private void OnGameStarted()
        {
            _cellsSteps.Clear();
        }
        private void OnUndo()
        {
            RestorePreviousState();
        }
        
        private void RecordCurrentState()
        {
            var snapshot = new BoardSnapshot(
                new Dictionary<Vector2Int, CellSnapshot>(_state.CellStates.Count),
                _state.HighestElementType.Value, _state.NextElementData.Value.Clone());

            foreach (var kvp in _state.CellStates)
            {
                var cell = kvp.Value;
                
                snapshot.Cells[kvp.Key] = new CellSnapshot(cell.Slot.GetActive(),
                    cell.Element != null ? cell.Element.GetData().Clone() : null);
            }


            DebugManager.Log(DebugCategory.Gameplay, $"Recording current state : {snapshot.Cells.Select(kvp => kvp.Value.IsActive).Count()} active cells, " +
                                                       $"Highest Element : {snapshot.HighestElementType}, " +
                                                       $"Next Element : {snapshot.NextElementData.ElementTypeInfo.ElementType}");
            
            _cellsSteps.Push(snapshot);
        }

        private void RestorePreviousState()
        {
            if (_cellsSteps.Count == 0) return;

            var snapshot = _cellsSteps.Pop();

            _boardManager.ClearGrid();
            
            _state.HighestElementType.Value = snapshot.HighestElementType;
            _state.NextElementData.Value = snapshot.NextElementData;

            foreach (var kvp in snapshot.Cells)
            {
                var pos = kvp.Key;
                var saved = kvp.Value;

                _state.CellStates[pos].Slot.ToggleActivation(saved.IsActive);

                if (saved.ElementData != null)
                {
                    _boardMoveMergeController.InstantiateElementAt(pos, saved.ElementData);
                }
            }
            
            _state.MergeStep = 1;
            
            
            DebugManager.Log(DebugCategory.Gameplay, $"restoring state : {snapshot.Cells.Select(kvp => kvp.Value.IsActive).Count()} active cells, " +
                                                     $"Highest Element : {snapshot.HighestElementType}, " +
                                                     $"Next Element : {snapshot.NextElementData.ElementTypeInfo.ElementType}");
        }
    }
}