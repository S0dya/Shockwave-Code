using System;
using System.Collections.Generic;
using Gameplay.Shockwave2048.Board;
using PT.GameplayAdditional.Input;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using PT.Tools.Helper;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.MegaMerge
{
    public class MegaMergeController : IInitializable, IDisposable
    {
        [Inject] private MegaMergeModel _model;
        [Inject] private InputManager _input;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private BoardShockwaveController _shockwaveController;
        [Inject] private MegaMergeSwipeBlocker _swipeBlocker;

        private readonly Stack<float> _mergeMeterChargeSteps = new();
        private bool _swipeStartedBlocked;

        public void Initialize()
        {
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Subscribe<UndoTurnSignal>(OnUndo);

            _input.OnClick += OnClick;
            _input.OnSwipe += OnSwipe;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Unsubscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Unsubscribe<UndoTurnSignal>(OnUndo);

            _input.OnClick -= OnClick;
            _input.OnSwipe -= OnSwipe;
        }

        private void OnGameStarted()
        {
            _mergeMeterChargeSteps.Clear();
        }

        private void OnGameTurn()
        {
            _mergeMeterChargeSteps.Push(_model.Charge.Value);
        }

        private void OnUndo()
        {
            if (_mergeMeterChargeSteps.Count == 0) return;
            _model.SetClampedCharge(_mergeMeterChargeSteps.Pop());
        }

        private void OnClick(Vector2 screenPos)
        {
            _swipeStartedBlocked = _swipeBlocker.IsPointBlocked(screenPos);
        }

        private void OnSwipe(DirectionEnum dir)
        {
            if (_swipeStartedBlocked) return;
            if (!_state.IsPlayerTurn) return;
            if (!_model.TryConsume()) return;

            DebugManager.Log(DebugCategory.Shockwave, $"Mega Merge activated: {dir}");
            _signalBus.Fire(new MegaMergeUsedSignal(dir));
        }

        public bool CanMegaMergeInAnyDirection()
        {
            if (!_model.IsFullCharged()) return false;

            foreach (DirectionEnum dir in Enum.GetValues(typeof(DirectionEnum)))
            {
                if (_shockwaveController.HasAnyMergeInDirection(dir))
                    return true;
            }

            return false;
        }
    }
}
