using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Enums;
using PT.Tools.Debugging;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardPlacementController
    {
        [Inject] private BoardState _state;
        [Inject] private ElementProvider _elementProvider;
        [Inject] private BoardMoveMergeController _moveMergeController;
        [Inject] private BoardTurnsController _boardTurnsController;
        [Inject] private BoardShockwaveController _boardShockwaveController;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardView _boardView;
        [Inject] private TurnBasedScoreManager _scoreManager;

        public bool CanPlaceElement(Vector2Int slotPosition)
        {
            var elementType = GetElementTypeOrEmpty(slotPosition);
            bool areDifferentTypes =
                elementType != ElementType.Empty &&
                elementType != _state.NextElementData.Value.ElementTypeInfo.ElementType;
            
            if (areDifferentTypes)
            {
                DebugManager.Log(DebugCategory.Gameplay, 
                    $"Element {elementType} is Not Placeable on {_state.NextElementData.Value.ElementTypeInfo.ElementType}"); return false;
            }
            
            return !areDifferentTypes;
        }
        
        public void PlaceElement(Vector2Int slotPosition)
        {
            var elementType = GetElementTypeOrEmpty(slotPosition);
            var newElementData = _state.NextElementData.Value;

            if (elementType == ElementType.Empty)
            {
                DebugManager.Log(DebugCategory.Gameplay, $"Spawn new element {newElementData} at {slotPosition}");

                _moveMergeController.InstantiateElementAt(slotPosition, newElementData);
            }
            else
            {
                DebugManager.Log(DebugCategory.Gameplay, $"Merge-place: {newElementData} into existing {elementType} at {slotPosition}");

                _state.CellStates[slotPosition].Element.SetData(_elementProvider.GetNext(elementType));
                
                _scoreManager.UpdateScore((int)newElementData.ElementTypeInfo.ElementType * _state.MergeStep);
            }

            _boardShockwaveController.ProcessTap(slotPosition, _state.CellStates[slotPosition].Element).Forget();
        }

        private ElementType GetElementTypeOrEmpty(Vector2Int slotPosition)
        {
            return _state.CellStates[slotPosition].Element != null ? 
                _state.CellStates[slotPosition].Element.GetElementType() : 
                ElementType.Empty;
        }
    }
}