using System;
using Cysharp.Threading.Tasks;
using PT.Logic.Configs;
using PT.Logic.ProjectContext;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board.ActionStates
{
    public class SwapActionState : ActionState
    {
        [Inject] private BoardState _state;
        [Inject] private BoardShockwaveController _boardShockwaveController;
        [Inject] private BoardMoveMergeController _boardMoveMergeController;
        [Inject] private AudioManager _adioManager;

        private Vector2Int? _selectedPos;

        public override void PerformAction(Vector2Int pos, CellState cellState, Action onComplete)
        {
            if (cellState.Element == null) return;
            
            if (_selectedPos == null)
            {
                _selectedPos = pos;
                _state.CellStates[_selectedPos.Value].Element.Select();
                return;
            }

            if (_selectedPos.Value == pos)
            {
                ClearSelection();
                return;
            }

            var tempPos = _selectedPos.Value;
            
            onComplete?.Invoke();
            
            Swap(tempPos, pos);
            _adioManager.PlayOneShot(SoundEventEnum.SkillSwap);
            ClearSelection();
        }

        private async UniTaskVoid Swap(Vector2Int aPos, Vector2Int bPos)
        {
            var elemA = _state.CellStates[aPos].Element;
            var elemB = _state.CellStates[bPos].Element;

            if (elemA == null || elemB == null) return;
            
            var aWorldPos = _state.CellStates[aPos].Slot.GetPosition();
            var bWorldPos = _state.CellStates[bPos].Slot.GetPosition();
            
            await _boardMoveMergeController.PlaySwap(elemA, elemB, aWorldPos, bWorldPos);

            _state.CellStates[aPos].Element = elemB;
            _state.CellStates[bPos].Element = elemA;

            elemA.transform.localPosition = bWorldPos;
            elemB.transform.localPosition = aWorldPos;
            
            _boardShockwaveController.ProcessTaps(new []{ aPos, bPos, }, new []{elemA, elemB}).Forget();
            
            ClearSelection();
            elemA.Deselect();
        }

        private void ClearSelection()
        {
            if (_selectedPos.HasValue)
            {
                _state.CellStates[_selectedPos.Value].Element.Deselect();
            }

            _selectedPos = null;
        }

        public override void RemoveState()
        {
            ClearSelection();
        }
    }
}