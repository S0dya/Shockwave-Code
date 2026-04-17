using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Slot;
using PT.Tools.Helper;
using UnityEngine;

namespace Gameplay.Shockwave2048.Grid
{
    public class SpiralOpenPattern : IGridOpenPattern
    {
        private readonly DirectionEnum _spiralDir;  // clockwise / counterclockwise

        public SpiralOpenPattern(DirectionEnum spiralDir)
        {
            _spiralDir = spiralDir;
        }

        public async UniTask<List<Vector2Int>> OpenSlots(Dictionary<Vector2Int, GridSlot> gridSlots, int amount, float openDelay = 0f)
        {
            int size = (int)Mathf.Sqrt(gridSlots.Count); // grid is guaranteed square
            int half = size / 2;
            var center = new Vector2Int(half, half);
            List<Vector2Int> openedPositions = new();

            List<Vector2Int> spiral = BuildSpiral(center, size, _spiralDir);

            foreach (var pos in spiral)
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

        private List<Vector2Int> BuildSpiral(Vector2Int center, int size, DirectionEnum dir)
        {
            var result = new List<Vector2Int>();
            int steps = 1;

            Vector2Int[] clockwiseOrder = {
                new Vector2Int(1,0),
                new Vector2Int(0,1),
                new Vector2Int(-1,0),
                new Vector2Int(0,-1)
            };
            Vector2Int[] counterOrder = {
                new Vector2Int(0,1),
                new Vector2Int(1,0),
                new Vector2Int(0,-1),
                new Vector2Int(-1,0)
            };

            var dirs = dir == DirectionEnum.Right ? clockwiseOrder : counterOrder;

            var pos = center;
            result.Add(pos);

            while (result.Count < size * size)
            {
                foreach (var d in dirs)
                {
                    for (int i = 0; i < steps; i++)
                    {
                        pos += d;
                        if (Inside(pos, size))
                            result.Add(pos);
                    }
                    if (d == dirs[1] || d == dirs[3])
                        steps++;
                }
            }

            return result;
        }

        private bool Inside(Vector2Int v, int size)
        {
            return v.x >= 0 && v.y >= 0 && v.x < size && v.y < size;
        }
    }
}