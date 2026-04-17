using System;
using Gameplay.Shockwave2048.Board.ActionStates;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Skills;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardActionController : IInitializable, IDisposable
    {
        [Inject] private ElementProvider _elementProvider;
        [Inject] private GameConfig _gameConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private SkillsManager _skillsManager;

        [Inject] private TapActionState _tap;
        [Inject] private SwapActionState _swap;
        [Inject] private DestroyActionState _destroy;
        
        private ActionState _currentState;
        
        public void Initialize()
        {
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);

            _currentState = _tap;
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
        }

        private void OnPlayerTurn()
        {
            RemoveState();
            _currentState = _tap;
        }

        public void PerformAction(Vector2Int pos)
        {
            DebugManager.Log(DebugCategory.Gameplay, $"Try Place Element at {pos}");
            
            if (!_state.IsPlayerTurn) { DebugManager.Log(DebugCategory.Gameplay, $"Can't place during Game Turn"); return; }
            if (!_state.CellStates[pos].Slot.GetActive()) { DebugManager.Log(DebugCategory.Gameplay, $"Element is Not Active"); return; }
            
            _currentState.PerformAction(pos, _state.CellStates[pos], OnActionComplete);
        }
        private void OnActionComplete()
        {
            _signalBus.Fire(new BoardActionPerformedSignal());
            RemoveState();
        }
        
        public void SetSwap()
        {
            RemoveState();
            _currentState = _swap;
        }
        public void SetDestroy()
        {
            RemoveState();
            _currentState = _destroy;
        }
        public void SetTap()
        {
            RemoveState();
            _currentState = _tap;
        }

        private void RemoveState()
        {
            _currentState.RemoveState();
        }
    }
}