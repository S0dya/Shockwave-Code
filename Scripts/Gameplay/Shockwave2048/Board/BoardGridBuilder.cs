using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Enums;
using Gameplay.Shockwave2048.Slot;
using PT.Logic.Configs;
using PT.Tools.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardGridBuilder
    {
        private readonly GridSlot _gridSlotPrefab;
        private readonly GridLayoutGroup _gridSlotsParent;
    
        private readonly BoardState _state;
        private readonly BoardPlacementController _boardPlacementController;
        private readonly GameConfig _gameConfig;
        private readonly ElementProvider _elementProvider;
        private readonly BoardMoveMergeController _moveMergeController;
        private readonly BoardActionController _boardActionController;

        public BoardGridBuilder(
            GridSlot slotPrefab, GridLayoutGroup slotsParent,
            BoardState state, 
            BoardPlacementController placement,
            GameConfig gameConfig,
            ElementProvider provider,
            BoardMoveMergeController moveMergeController,
            BoardActionController boardActionController)
        {
            _gridSlotPrefab = slotPrefab;
            _gridSlotsParent = slotsParent;

            _state = state;
            _boardPlacementController = placement;
            _gameConfig = gameConfig;
            _elementProvider = provider;
            _moveMergeController = moveMergeController;
            _boardActionController = boardActionController;
        }
        public async UniTask CreateSlotsAndElements()
        {
            _gridSlotsParent.constraintCount = _state.GridSize.x; 
            
            for (int i = 0; i < _state.GridCount; i++)
            {
                var pos = new Vector2Int(i % _state.GridSize.x, i / _state.GridSize.y);
                var gridSlot = GameObject.Instantiate(_gridSlotPrefab, _gridSlotsParent.transform);
                
                _state.CellStates[pos] = new (gridSlot, null); 
                gridSlot.Init(() => _boardActionController.PerformAction(pos), null);
                
                gridSlot.ToggleActivation(false, false);
            }
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_gridSlotsParent.transform);
            
            _gridSlotsParent.enabled = false;
        }

        public async void SpawnInitialElements()
        {
            int count = _gameConfig.InitialAddedElements.GetRandomValue();
            
            var empty = _state.CellStates
                .Where(kvp => kvp.Value.Slot.GetActive() && kvp.Value.Element == null)
                .Select(kvp => kvp.Key)
                .ToList();

            count = Mathf.Clamp(count, 0, empty.Count);

            for (int i = 0; i < count; i++)
            {
                int rnd = Utils.GetRandomNextInt(empty.Count);
                var pos = empty[rnd];
                empty.RemoveAt(rnd);

                var data = _elementProvider.GetData(ElementType.Two);

                _moveMergeController.InstantiateElementAt(pos, data);

                await UniTask.Yield();
            }
        }
    }
}