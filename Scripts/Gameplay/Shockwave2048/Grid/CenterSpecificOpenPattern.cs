using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Slot;
using UnityEngine;

namespace Gameplay.Shockwave2048.Grid
{
    public class CenterSpecificOpenPattern : IGridOpenPattern
    {
        public async UniTask<List<Vector2Int>> OpenSlots(Dictionary<Vector2Int, GridSlot> gridSlots, int amount, float openDelay = 0f)
        {
            int size = (int)Mathf.Sqrt(gridSlots.Count);
            int half = size / 2;
            var center = new Vector2Int(half, half);
            List<Vector2Int> openedPositions = new();

            var ordered = new List<Vector2Int>();

            // 1) center first
            ordered.Add(center);

            // 2) cardinals (up/down/left/right)
            ordered.Add(new Vector2Int(center.x + 1, center.y));
            ordered.Add(new Vector2Int(center.x - 1, center.y));
            ordered.Add(new Vector2Int(center.x, center.y + 1));
            ordered.Add(new Vector2Int(center.x, center.y - 1));

            // 3) outer ring for the grid
            for (int x = 0; x < size; x++)
            {
                ordered.Add(new Vector2Int(x, 0));
                ordered.Add(new Vector2Int(x, size - 1));
            }
            for (int y = 1; y < size - 1; y++)
            {
                ordered.Add(new Vector2Int(0, y));
                ordered.Add(new Vector2Int(size - 1, y));
            }

            // remove duplicates
            var unique = new HashSet<Vector2Int>();
            var sequence = new List<Vector2Int>();
            foreach (var v in ordered)
            {
                if (Inside(v, size) && unique.Add(v))
                    sequence.Add(v);
            }

            foreach (var pos in sequence)
            {
                if (amount <= 0) break;

                var slot = gridSlots[pos];

                if (!slot.GetActive())
                {
                    slot.ToggleActivation(true);
                    openedPositions.Add(pos);
                    amount--;

                    if (openDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(openDelay));
                }
            }

            return openedPositions;
        }

        private bool Inside(Vector2Int v, int size)
        {
            return v.x >= 0 && v.y >= 0 && v.x < size && v.y < size;
        }
    }
}