using Gameplay.Shockwave2048.Board;
using NaughtyAttributes;
using PT.Logic.Dependency.Signals;
using PT.Tools.Helper;
using PT.UI.Effects.ParticleFollow;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Gameplay.Shockwave2048
{
    public class GameEffectsController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private ParticleSystem destroyedElementPrefab;
        [SerializeField] private ParticleSystem mergePrefab;
        [Space]
        [SerializeField] private ParticleSystem megaMergeReady;
        [SerializeField] private ParticleSystem megaMergeUsed;
        [Space]
        [SerializeField] private UIParticlesFollowManager particlesFollowManager;
        [SerializeField][MinMaxSlider(3, 30)] private Vector2Int goldParticlesRange = new (4, 10);

        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;

        private ObjectPool<ParticleSystem> _destroyPool;
        private ObjectPool<ParticleSystem> _mergePool;

        private void Awake()
        {
            _destroyPool = CreatePool(destroyedElementPrefab);
            _mergePool = CreatePool(mergePrefab);
            
            _signalBus.Subscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Subscribe<ElementDestroyedSignal>(OnElementDestroyed);
            _signalBus.Subscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);
            _signalBus.Subscribe<BonusUsedSignal>(OnBonusUsed);
        }
        private void OnDisable()
        {
            _signalBus.Unsubscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Unsubscribe<ElementDestroyedSignal>(OnElementDestroyed);
            _signalBus.Unsubscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);
            _signalBus.Unsubscribe<BonusUsedSignal>(OnBonusUsed);
        }

        private ObjectPool<ParticleSystem> CreatePool(ParticleSystem prefab)
        {
            return new ObjectPool<ParticleSystem>( 
                () => { return Instantiate(prefab, transform); },
                ps =>
                {
                    ps.gameObject.SetActive(true);
                    ps.Play(true);
                },
                ps =>
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                },
                defaultCapacity: 8,
                maxSize: 16
            );
        }

        public void PlayMegaMergeReady()
        {
            megaMergeReady.Play();
        }
        public void StopMegaMergeReady()
        {
            megaMergeReady.Stop();
        }
        
        private void OnMerge(BoardMergeSignal s)
        {
            var ps = _mergePool.Get();
            ps.transform.localPosition = _state.CellStates[s.SlotPosition].Slot.GetPosition();
            BindReturn(ps, _mergePool);
        }

        private void OnElementDestroyed(ElementDestroyedSignal s)
        {
            var ps = _destroyPool.Get();
            ps.transform.localPosition = s.Position;
            BindReturn(ps, _destroyPool);
        }

        private void OnMegaMergeUsed(MegaMergeUsedSignal s)
        {
            StopMegaMergeReady();
            
            megaMergeUsed.transform.rotation = DirectionToRotation(s.Direction);
            megaMergeUsed.Play();
        }
        
        private void OnBonusUsed(BonusUsedSignal s)
        {
            particlesFollowManager.InstantiateParticlesFollowTo(s.Position, false,
                ParticleFollowEnum.Gold, FollowToTargetEnum.GoldGame, 
                null, goldParticlesRange.GetRandomValue());
        }

        private void BindReturn(ParticleSystem ps, ObjectPool<ParticleSystem> pool)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);

            ps.gameObject.AddComponent<ParticleReturn>().Init(ps, pool);
        }

        private Quaternion DirectionToRotation(DirectionEnum dir)
        {
            return dir switch
            {
                DirectionEnum.Up => Quaternion.Euler(0, 0, 0),
                DirectionEnum.Right => Quaternion.Euler(0, 0, -90),
                DirectionEnum.Down => Quaternion.Euler(0, 0, 180),
                DirectionEnum.Left => Quaternion.Euler(0, 0, 90),
                _ => Quaternion.identity
            };
        }
    }

    internal class ParticleReturn : MonoBehaviour
    {
        private ParticleSystem _ps;
        private ObjectPool<ParticleSystem> _pool;

        public void Init(ParticleSystem ps, ObjectPool<ParticleSystem> pool)
        {
            _ps = ps;
            _pool = pool;
        }

        private void OnParticleSystemStopped()
        {
            if (_pool != null && _ps != null) _pool.Release(_ps);

            Destroy(this);
        }
    }
}