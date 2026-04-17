using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Slot;
using PT.Tools.Helper;
using UnityEngine;

namespace Gameplay.Shockwave2048.Grid
{
    public class RandomOpenPattern : IGridOpenPattern
    {
        public async UniTask<List<Vector2Int>> OpenSlots(Dictionary<Vector2Int, GridSlot> gridSlots, int amount, float delay)
        {
            List<Vector2Int> openedPositions = new();
            
            while (amount > 0)
            {
                var ran = Utils.GetRandomElement(gridSlots.ToArray());
                
                if (!ran.Value.GetActive())
                {
                    ran.Value.ToggleActivation(true);
                    openedPositions.Add(ran.Key);
                    amount--;
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
            }

            return openedPositions;
        }
    }
}