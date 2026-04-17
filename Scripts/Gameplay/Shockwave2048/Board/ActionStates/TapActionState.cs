using System;
using PT.Logic.Configs;
using PT.Logic.ProjectContext;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board.ActionStates
{
    public class TapActionState : ActionState
    {
        [Inject] private BoardPlacementController _boardPlacementController;
        [Inject] private AudioManager _adioManager;
        
        public override void PerformAction(Vector2Int pos, CellState cellState, Action onComplete)
        {
            if (_boardPlacementController.CanPlaceElement(pos))
            {
                onComplete?.Invoke();

                _boardPlacementController.PlaceElement(pos);
                
                _adioManager.PlayOneShot(SoundEventEnum.ElementPlaced);
            }
        }
    }
}