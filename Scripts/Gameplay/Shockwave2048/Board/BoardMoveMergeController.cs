using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
    public class BoardMoveMergeController
    {
        [Inject] private BoardState _state;
        [Inject] private BoardView _boardView;
        [Inject] private GameConfig _gameConfig;
        [Inject] private ElementProvider _elementProvider;
        [Inject] private TurnBasedScoreManager _scoreManager;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardGridController _boardGridController;
        [Inject] private BoardElementPoolService _elementPool;
        [Inject] private BoardBonusesController _boardBonusesController;
        
        public void InstantiateElementAt(Vector2Int gridPosition, ElementData data)
        {
            var element = _elementPool.Pool.Get();
            _state.CellStates[gridPosition].Element = element;
            element.transform.localPosition = _state.CellStates[gridPosition].Slot.GetPosition();
            element.SetData(data);
        }
        
        public IPromise MergeMove(Vector2Int pos, Vector2Int target, out Element toMergeElement)
        {
            DebugManager.Log(DebugCategory.Gameplay, $"MergeMove {pos} -> {target}");

            toMergeElement = _state.CellStates[pos].Element;
            var destroyElement = _state.CellStates[target].Element;

            var toMergeElementData = toMergeElement.GetData();
            var mergePos = _state.CellStates[target].Slot.GetPosition();
            
            var promise = _boardView.MergeMove(
                    toMergeElement,
                    _state.CellStates[target].Slot.GetPosition(),
                    destroyElement,
                    _gameConfig.MergeAnimationStartDistance)
                .Then(() =>
                {
                    var destroyElementData = destroyElement.GetData();
                    
                    var newData = _elementProvider.GetNext(toMergeElementData, destroyElementData);
                    DebugManager.Log(DebugCategory.Gameplay, $"Merge result type {newData.ElementTypeInfo.ElementType}");
                    
                    CheckElementDataForBonuses(mergePos, destroyElementData); 
                    CheckElementDataForBonuses(mergePos, toMergeElementData);
                    
                    _state.CellStates[target].Element.SetData(newData);
                    _elementPool.Pool.Release(destroyElement);
                    
                    TrySetElementTypeHighest(newData.ElementTypeInfo.ElementType);
                });

            ResetSlots(pos, target);
            return promise;
        }
        public IPromise Move(Vector2Int pos, Vector2Int target)
        {
            var promise = _boardView.Move(_state.CellStates[pos].Element, _state.CellStates[target].Slot.GetPosition());
            ResetSlots(pos, target);
            return promise;
        }

        public void Merge(Element mergedElement, out bool reachedMax)
        {
            var kvp = _state.CellStates.FirstOrDefault(kvp => kvp.Value.Element == mergedElement);
            var cellState = kvp.Value;
            
            _scoreManager.UpdateScore((int)mergedElement.GetElementType() * _state.MergeStep);
            _state.MergeStep++;
            _signalBus.Fire(new BoardMergeSignal(kvp.Key));
            
            mergedElement.StopMergeEffect();

            reachedMax = CheckLimitTypeReached(cellState);
        }
        
        public UniTask PlaySwap(Element a, Element b, Vector3 aTarget, Vector3 bTarget, float duration = 0.15f)
        {
            var seq = DOTween.Sequence();

            seq.Join(a.transform.DOLocalMove(bTarget, duration).SetEase(Ease.OutQuad));
            seq.Join(b.transform.DOLocalMove(aTarget, duration).SetEase(Ease.OutQuad));

            return seq.ToUniTask();
        }
        
        private void ResetSlots(Vector2Int pos, Vector2Int target)
        {
            _state.CellStates[target].Element = _state.CellStates[pos].Element;
            _state.CellStates[pos].Element = null;
        }

        private void TrySetElementTypeHighest(ElementType newType)
        {
            if (_elementProvider.TryGetNextType(newType, out ElementType nextType) && (int)nextType > (int)_state.HighestElementType.Value)
            {
                DebugManager.Log(DebugCategory.Gameplay, $"New highest type reached: {newType}");

                _state.HighestElementType.Value = nextType;
            }
        }

        private void CheckElementDataForBonuses(Vector2 pos, ElementData elementData)
        {
            if (elementData.HasBonus)
            {
                _boardBonusesController.BonusUsed();
                
                elementData.RemoveBonus();
                
                _signalBus.Fire(new BonusUsedSignal(pos));
            }
        }
        
        private bool CheckLimitTypeReached(CellState cellState)
        {
            if (cellState.Element.GetElementType() == _gameConfig.MaxType)
            {
                _elementPool.Pool.Release(cellState.Element);
                cellState.Element = null;

                return true;
            }

            return false;
        }
    }
}