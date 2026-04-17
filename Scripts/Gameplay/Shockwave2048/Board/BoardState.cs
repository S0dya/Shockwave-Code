using System.Collections.Generic;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Enums;
using Gameplay.Shockwave2048.Slot;
using UniRx;
using UnityEngine;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardState
    {
        public Dictionary<Vector2Int, CellState> CellStates = new();

        public Vector2Int GridSize;
        public int GridCount => GridSize.x * GridSize.y;

        public readonly ReactiveProperty<ElementData> NextElementData = new();
        public readonly ReactiveProperty<ElementType> HighestElementType = new(ElementType.Eight);

        public bool IsPlayerTurn;
        public int GameStep;
        
        public int MergeStep = 1;
    }
    
    public class CellState
    {
        public GridSlot Slot;
        public Element Element;

        public CellState(GridSlot slot, Element element)
        {
            Slot = slot;
            Element = element;
        }
    }
}