using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Slot;
using UnityEngine;

namespace Gameplay.Shockwave2048.Grid
{
    public class RandomCenterOpenPattern : IGridOpenPattern
    {
        private static readonly Vector2Int[] Neigh = { new (1,0), new (-1,0), new (0,1), new (0,-1) };

        public async UniTask<List<Vector2Int>> OpenSlots(Dictionary<Vector2Int, GridSlot> gridSlots, int amount, float openDelay = 0f)
        {
            int size = (int)Mathf.Sqrt(gridSlots.Count);
            int half = size / 2;
            var center = new Vector2Int(half, half);
            List<Vector2Int> openedPositions = new();

            while (amount > 0)
            {
                bool anyActive = gridSlots.Any(kvp => kvp.Value.GetActive());

                if (!anyActive)
                {
                    gridSlots[center].ToggleActivation(true);
                    amount--;

                    if (openDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(openDelay));

                    continue; 
                }

                var weightedCandidates = new List<(Vector2Int pos, int weight)>();

                foreach (var kvp in gridSlots)
                {
                    if (kvp.Value.GetActive())
                        continue;

                    int activeNeighbors = CountActiveNeighbors(gridSlots, kvp.Key);
                    if (activeNeighbors > 0)
                        weightedCandidates.Add((kvp.Key, activeNeighbors));
                }

                if (weightedCandidates.Count == 0)
                {
                    var inactive = gridSlots
                        .Where(kvp => !kvp.Value.GetActive())
                        .Select(kvp => kvp.Key)
                        .ToList();

                    if (inactive.Count == 0)
                        break;

                    var fallback = inactive[UnityEngine.Random.Range(0, inactive.Count)];
                    gridSlots[fallback].ToggleActivation(true);
                    amount--;

                    if (openDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(openDelay));

                    continue;
                }

                var chosenPos = WeightedRandom(weightedCandidates);
                var slot = gridSlots[chosenPos];

                slot.ToggleActivation(true);
                openedPositions.Add(chosenPos);
                amount--;

                if (openDelay > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(openDelay));
            }

            return openedPositions;
        }

        private int CountActiveNeighbors(Dictionary<Vector2Int, GridSlot> all, Vector2Int pos)
        {
            int count = 0;
            foreach (var n in Neigh)
            {
                var p = pos + n;
                if (all.TryGetValue(p, out var slot) && slot.GetActive())
                    count++;
            }
            return count;
        }

        private Vector2Int WeightedRandom(List<(Vector2Int pos, int weight)> list)
        {
            int total = list.Sum(x => x.weight);
            int r = UnityEngine.Random.Range(0, total);

            foreach (var x in list)
            {
                if (r < x.weight)
                    return x.pos;
                r -= x.weight;
            }

            return list[list.Count - 1].pos;
        }
    }
}