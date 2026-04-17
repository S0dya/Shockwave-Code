using System;
using UnityEngine;

namespace Gameplay.Shockwave2048.Board.ActionStates
{
    public abstract class ActionState
    {
        public abstract void PerformAction(Vector2Int pos, CellState cellState, Action onComplete);
        
        public virtual void RemoveState() {}
    }
}