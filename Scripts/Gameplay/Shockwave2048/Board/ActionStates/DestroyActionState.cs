using System;
using Cysharp.Threading.Tasks;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Logic.ProjectContext;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board.ActionStates
{
    public class DestroyActionState : ActionState
    {
        [Inject] private BoardElementPoolService _elementPool;
        [Inject] private BoardShockwaveController _boardShockwaveController;
        [Inject] private SignalBus _signalBus;
        [Inject] private AudioManager _adioManager;

        public override void PerformAction(Vector2Int pos, CellState cellState, Action onComplete)
        {
            if (cellState.Element == null) return;
            
            var element = cellState.Element;
            
            onComplete?.Invoke();
            
            _boardShockwaveController.ProcessTap(pos, element).Forget();
            
            _signalBus.Fire(new ElementDestroyedSignal(cellState.Slot.GetPosition()));
            
            _elementPool.Pool.Release(element);
            _adioManager.PlayOneShot(SoundEventEnum.SkillDestroy);
            cellState.Element = null;
        }
    }
}