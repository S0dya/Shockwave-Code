using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Slot;
using UnityEngine;

namespace Gameplay.Shockwave2048.Grid
{
    public interface IGridOpenPattern
    {
        public UniTask<List<Vector2Int>> OpenSlots(Dictionary<Vector2Int, GridSlot> gridSlots, int amount, float openDelay = 0);
    }
}