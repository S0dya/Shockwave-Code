using System;
using System.Linq;
using DG.Tweening;
using Gameplay.Shockwave2048.Elements;
using Libraries.RSG;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Helper;
using PT.UI.Effects;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private GameObject pushEffect;
        [SerializeField] private Transform pushEffectsParent;
        [SerializeField] private float pushEffectTargetScale = 5;
        [SerializeField] private float pushEffectDuration = 0.75f;
        [Space]
        [SerializeField] private ParticleSystem maxMergeReachedEffectPrefab;
        [Space(20)]
        [SerializeField] private MergeTextPopUp textPopUpPrefab;
        [SerializeField] private Transform textPopUpParent;
        [Space]
        [SerializeField] private SerializableKeyValue<int, TextPopUp> chainCompletedPopUps;
        
        [Inject] private GameConfig _gameConfig;
        [Inject] private BoardState _state;
        [Inject] private SignalBus _signalBus;
        
        private Vector2 _pushEffectInitialScale;

        private ObjectPool<GameObject> _pushEffectPool;
        private ObjectPool<ParticleSystem> _maxMergeReachedEffectPool;
        private ObjectPool<MergeTextPopUp> _textPopUpPool;

        private void Awake()
        {
            _signalBus.Subscribe<GridElementPushedSignal>(GridElementPushed);
            _signalBus.Subscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Subscribe<MergeChainCompletedSignal>(OnMergeChainCompleted);
            _signalBus.Subscribe<ElementReachedMaxOnMergeSignal>(OnElementReachedMaxOnMerge);
            
            foreach (var popUp in chainCompletedPopUps.Dictionary.Values) popUp.Cancel();
        }
        
        public void Init()
        {
            _pushEffectInitialScale =  pushEffect.transform.localScale;
            _pushEffectPool = new ObjectPool<GameObject>(
                () => Instantiate(pushEffect, pushEffectsParent),
                obj => { obj.SetActive(true); },
            obj =>
                {
                    obj.SetActive(false);
                    obj.transform.localScale = _pushEffectInitialScale;
                },
                defaultCapacity: _state.GridCount );
            
            _maxMergeReachedEffectPool = new ObjectPool<ParticleSystem>(
                () => Instantiate(maxMergeReachedEffectPrefab, pushEffectsParent),
                obj => { obj.SetActive(true); },
            obj => { obj.SetActive(false); },
                defaultCapacity: _state.GridCount );

            var initialTextPopUpScale = textPopUpPrefab.transform.localScale;
            _textPopUpPool = new ObjectPool<MergeTextPopUp>(
                () => Instantiate(textPopUpPrefab, textPopUpParent),
                t => t.gameObject.SetActive(true),
                t =>
                {
                    t.gameObject.SetActive(false);
                    t.transform.localScale = initialTextPopUpScale;
                },
                defaultCapacity: 10);
        }
        
        private void GridElementPushed(GridElementPushedSignal signal)
        {
            PlayPush(signal.Position);
        }

        private void OnMerge(BoardMergeSignal signal)
        {
            var textPopUp = _textPopUpPool.Get();

            textPopUp.SetMergeStep(_state.MergeStep);

            textPopUp.Play($"x{_state.MergeStep}", _state.CellStates[signal.SlotPosition].Slot.GetPosition())
                .Done(() =>
                {
                    if (textPopUp != null) _textPopUpPool.Release(textPopUp);
                });
        }

        private void OnMergeChainCompleted()
        {
            foreach (var popUp in chainCompletedPopUps.Dictionary.Values) popUp.Cancel();

            var leastPopUp = chainCompletedPopUps.Dictionary.FirstOrDefault(kvp => kvp.Key >= _state.MergeStep).Value;
            if (leastPopUp == null) leastPopUp = chainCompletedPopUps.Dictionary.Values.Last();
            leastPopUp.Play();
        }

        private void OnElementReachedMaxOnMerge(ElementReachedMaxOnMergeSignal signal)
        {
            var ps =  _maxMergeReachedEffectPool.Get();
            
            ps.transform.position = signal.Position;
            
            ps.Play();
        }
        
        public IPromise MergeMove(Element elementMoving, Vector2 destination, Element elementTo, float mergeDistance)
        {
            return elementMoving.transform
                .DOLocalMove(destination, GetMoveDuration())
                .SetEase(Ease.OutQuart)
                .OnUpdate(() =>
                {
                    if (Vector2.Distance(elementMoving.transform.position, elementTo.transform.position) < mergeDistance)
                    {
                        elementMoving.PlayMergeEffect();
                        elementTo.PlayMergeEffect();
                    }
                }).ToPromise();
        }
        
        public IPromise Move(Element elementMoving, Vector2 destination)
        {
            return elementMoving.transform
                .DOLocalMove(destination, GetMoveDuration())
                .SetEase(Ease.OutQuart)
                .ToPromise();
        }

        private void PlayPush(Vector2 position)
        {
            var effectObj = _pushEffectPool.Get();

            effectObj.transform.localPosition = position;

            effectObj.transform.DOScale(pushEffectTargetScale, pushEffectDuration )
                .OnComplete(() =>
                {
                    _pushEffectPool.Release(effectObj);
                });
        }

        private float GetMoveDuration() => _gameConfig.ElementSpeedComboProgression[Math.Min(_state.MergeStep - 1, _gameConfig.ElementSpeedComboProgression.Length - 1)];
        
        public void OnDestroy()
        {
            _signalBus.Unsubscribe<GridElementPushedSignal>(GridElementPushed);
            _signalBus.Unsubscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Unsubscribe<MergeChainCompletedSignal>(OnMergeChainCompleted);
        }
    }
}