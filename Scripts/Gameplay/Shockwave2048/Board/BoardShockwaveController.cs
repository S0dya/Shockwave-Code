using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Enums;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using PT.Tools.Helper;
using UnityEngine;
using Zenject;
using IPromise = Libraries.RSG.IPromise;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardShockwaveController : IInitializable, IDisposable
    {
        [Inject] private GameConfig _gameConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private BoardMoveMergeController _boardMoveMergeController;

        private Queue<Element> _pendingShockwaves = new();
        
        private CancellationTokenSource _cts;
        
        public void Initialize()
        {
            _signalBus.Subscribe<GameEndedSignal>(OnGameFinished);
            _signalBus.Subscribe<MegaMergeUsedSignal>(ProcessMegaMerge);
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<GameEndedSignal>(OnGameFinished);
            _signalBus.Unsubscribe<MegaMergeUsedSignal>(ProcessMegaMerge);
        }

        public async UniTask ProcessTap(Vector2Int origin, Element origElement)
        {
            await RunTurn(async token => 
            {
                DebugManager.Log(DebugCategory.Shockwave, "Performing initial shockwave burst...");
                
                await PerformShockwave(origin, origElement.GetActivePushDirections(), _cts.Token);
            });
        }
        public async UniTask ProcessTaps(Vector2Int[] origins, Element[] origElements)
        {
            if (origins.Length != origElements.Length) throw new ArgumentException("Origins and elements count mismatch");

            await RunTurn(async token =>
            {
                DebugManager.Log(DebugCategory.Shockwave, "Performing MULTI shockwave burst...");

                for (int i = 0; i < origins.Length; i++) await PerformShockwave(origins[i], origElements[i].GetActivePushDirections(), token);
            });
        }
        
        public bool HasAnyMergeInDirection(DirectionEnum dir)
        {
            var d = Utils.GetDirectionInt(dir);

            foreach (var kvp in _state.CellStates)
            {
                var fromPos = kvp.Key;
                var fromElem = kvp.Value.Element;
                if (fromElem == null) continue;

                var fromType = fromElem.GetElementType();
                var next = fromPos + d;

                while (_state.CellStates.ContainsKey(next) && _state.CellStates[next].Slot.GetActive())
                {
                    var nextElem = _state.CellStates[next].Element;
                    if (nextElem == null)
                    {
                        next += d;
                        continue;
                    }

                    if (nextElem.GetElementType() == fromType) return true;

                    break;
                }
            }

            return false;
        }
        
        private async UniTask ProcessMegaMerge(DirectionEnum dir)
        {
            await RunTurn(async token =>
            {
                var tasks = new List<UniTask>();

                foreach (var entry in GetEntryCells(dir)) tasks.Add(PerformShockwave(entry, new[]{dir}, token));
                
                await UniTask.WhenAll(tasks);
            });
        }

        private async UniTask RunTurn(Func<CancellationToken, UniTask> func)
        {
            _cts?.Cancel();
            _cts = new();

            try
            {
                await func.Invoke(_cts.Token);
                await ProcessTurn();
            }
            catch (OperationCanceledException)
            {
                DebugManager.Log(DebugCategory.Shockwave, "processing CANCELED",  LogType.Error);
            }
        }
        private async UniTask ProcessTurn()
        {
            while (_pendingShockwaves.Count > 0)
            {
                var elem = _pendingShockwaves.Dequeue();
                if (elem == null || !elem.gameObject.activeSelf) continue;

                DebugManager.Log(DebugCategory.Shockwave, $"Pending merge shockwave continued for element at pos {GetPositionByElement(elem)}");

                var lemPosition = elem.transform.position;
                _boardMoveMergeController.Merge(elem, out bool reachedMax);
                if (reachedMax)
                {
                    PlayMaxReached(lemPosition);
                    continue;
                }

                var pos = GetPositionByElement(elem);
                await PerformShockwave(pos, elem.GetActivePushDirections(), _cts.Token);
            }
            
            _signalBus.Fire(new PlayerTurnSignal());
            
            DebugManager.Log(DebugCategory.Shockwave, "ProcessTurn finished");
        }
        
        private void ProcessMegaMerge(MegaMergeUsedSignal signal)
        {
            ProcessMegaMerge(signal.Direction).Forget();
        }
        
        private void OnGameFinished()
        {
            _cts?.Cancel();
        }
        private async UniTask PerformShockwave(Vector2Int origin, DirectionEnum[] directions, CancellationToken token)
        {
            if (_state.CellStates.ContainsKey(origin)) _signalBus.Fire(new GridElementPushedSignal(_state.CellStates[origin].Slot.GetPosition()));
            
            DebugManager.Log(DebugCategory.Shockwave, $"PerformShockwave from {origin}");

            var tasks = new List<UniTask>();

            foreach (var dir in directions)
            {
                DebugManager.Log(DebugCategory.Shockwave, $"Queue push/merge in direction {dir}");
                tasks.Add(PushAndMerge(origin, dir, token));
            }

            await UniTask.WhenAll(tasks);

            DebugManager.Log(DebugCategory.Shockwave, $"Shockwave from {origin} complete");
        }

        private async UniTask PushAndMerge(Vector2Int origin, DirectionEnum dir, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            DebugManager.Log(DebugCategory.Shockwave, $"PushAndMerge start. Origin {origin}, Dir {dir}");

            var allMovements = new List<UniTask>();
            var alreadyMerged = new HashSet<Vector2Int>();

            foreach (var pos in GetLinePositions(origin, dir))
            {
                token.ThrowIfCancellationRequested();

                var element = _state.CellStates[pos].Element;
                if (element == null) continue;

                var direction = Utils.GetDirectionInt(dir);
                var targetPos = FindFarthestValidPosition(pos, direction, alreadyMerged);
                if (targetPos == pos) continue;

                DebugManager.Log(DebugCategory.Shockwave, $"{pos} -> {targetPos} ({element.GetElementType()})");

                IPromise movePromise;

                var targetElement = _state.CellStates[targetPos].Element;
                
                if (targetElement != null && element.GetElementType() == targetElement.GetElementType())
                {
                    alreadyMerged.Add(targetPos);

                    DebugManager.Log(DebugCategory.Shockwave, $"MERGE triggered: {pos} -> {targetPos}");
                    movePromise = _boardMoveMergeController.MergeMove(pos, targetPos, out Element mergedElement);

                    _pendingShockwaves.Enqueue(mergedElement);
                }
                else
                {
                    movePromise = _boardMoveMergeController.Move(pos, targetPos);
                }

                allMovements.Add(movePromise.ToUniTask());
            }

            await UniTask.WhenAll(allMovements);

            DebugManager.Log(DebugCategory.Shockwave, $"PushAndMerge finished for {dir}");
        }

        private void PlayMaxReached(Vector2 pos)
        {
            _signalBus.Fire(new ElementReachedMaxOnMergeSignal(pos));
        }
        
        private IEnumerable<Vector2Int> GetLinePositions(Vector2Int origin, DirectionEnum dir)
        {
            var d = Utils.GetDirectionInt(dir);

            var end = origin;
            while (IsInside(end + d))
                end += d;

            var p = end;
            while (IsInside(p) && p != origin)
            {
                yield return p;
                p -= d;
            }
        }

        private Vector2Int FindFarthestValidPosition(Vector2Int from, Vector2Int dir, HashSet<Vector2Int> alreadyMerged)
        {
            var farthest = from;
            var next = from + dir;
            var fromType = _state.CellStates[from].Element.GetElementType();

            while (IsInsideAndActive(next))
            {
                var element = _state.CellStates[next].Element;
                var targetType = element != null ? element.GetElementType() : ElementType.Empty;

                if (targetType == ElementType.Empty)
                {
                    farthest = next;
                }
                else if (targetType == fromType && !alreadyMerged.Contains(next))
                {
                    farthest = next;
                    break;
                }
                else break;

                next += dir;
            }

            return farthest;
        }
        
        private bool IsInsideAndActive(Vector2Int pos) => IsInside(pos) && _state.CellStates[pos].Slot.GetActive();
        private bool IsInside(Vector2Int pos) => _state.CellStates.ContainsKey(pos);
        
        private Vector2Int GetPositionByElement(Element element) => _state.CellStates.FirstOrDefault(kvp => kvp.Value.Element == element).Key;
        
        
        private IEnumerable<Vector2Int> GetEntryCells(DirectionEnum dir)
        {
            var keys = _state.CellStates.Keys;
            int minX = keys.Min(k => k.x), maxX = keys.Max(k => k.x);
            int minY = keys.Min(k => k.y), maxY = keys.Max(k => k.y);

            switch (dir)
            {
                case DirectionEnum.Right: for (int y = minY; y <= maxY; y++) yield return new Vector2Int(minX - 1, y); break;
                case DirectionEnum.Left:  for (int y = minY; y <= maxY; y++) yield return new Vector2Int(maxX + 1, y); break;
                case DirectionEnum.Down:  for (int x = minX; x <= maxX; x++) yield return new Vector2Int(x, maxY + 1); break;
                case DirectionEnum.Up:    for (int x = minX; x <= maxX; x++) yield return new Vector2Int(x, minY - 1); break;
            }
        }
    }
}