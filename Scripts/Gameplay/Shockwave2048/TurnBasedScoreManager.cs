using System.Collections.Generic;
using PT.Logic.Dependency.Signals;
using PT.Tools.Score;
using Zenject;

namespace Gameplay.Shockwave2048
{
    public class TurnBasedScoreManager : ScoreManager, IInitializable
    {
        private Stack<int> _prevScores = new();
        
        public override void Initialize()
        {
            base.Initialize();
            
            _signalBus.Subscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Subscribe<UndoTurnSignal>(OnUndoTurn);
        }
        
        private void OnGameTurn()
        {
            _prevScores.Push(CurrentScoreReactive.Value);
        }

        private void OnUndoTurn()
        {
            if (_prevScores.Count == 0) return;
            
            var prev = CurrentScoreReactive.Value;
            var restored = _prevScores.Pop();

            CurrentScoreReactive.Value = restored;
        }

        protected override void ResetScore()
        {
            base.ResetScore();
            
            _prevScores.Clear();
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Unsubscribe<UndoTurnSignal>(OnUndoTurn);
        }
    }
}