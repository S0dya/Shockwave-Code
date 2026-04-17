using System;
using FMODUnity;
using Gameplay.Shockwave2048.Board;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Logic.ProjectContext;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048
{
    public class GameSoundsController : IInitializable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private AudioManager _audioManager;

        public void Initialize()
        {
            _signalBus.Subscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Subscribe<GridElementPushedSignal>(OnElementPushed);
            _signalBus.Subscribe<MegaMergeReadySignal>(OnMegaMergeReady);
            _signalBus.Subscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);
            _signalBus.Subscribe<MergeChainCompletedSignal>(OnMegaMergeChainCompleted);
            _signalBus.Subscribe<BonusUsedSignal>(OnBonusUsed);
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Unsubscribe<GridElementPushedSignal>(OnElementPushed);
            _signalBus.Unsubscribe<MegaMergeReadySignal>(OnMegaMergeReady);
            _signalBus.Unsubscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);
            _signalBus.Unsubscribe<MergeChainCompletedSignal>(OnMegaMergeChainCompleted);
            _signalBus.Unsubscribe<BonusUsedSignal>(OnBonusUsed);
        }

        private void OnMerge(BoardMergeSignal s)
        {
            float pitch = Mathf.Clamp(1f + (_state.MergeStep - 1) * 0.08f, 1f, 1.4f);

            var instance = RuntimeManager.CreateInstance(_audioManager.GetEventReference(SoundEventEnum.Merge));

            instance.setParameterByName("pitch", pitch);
            instance.start();
            instance.release();
        }

        private void OnElementPushed(GridElementPushedSignal s)
        {
            _audioManager.PlayOneShot(SoundEventEnum.ElementPushed, isRelease: true);
        }

        private void OnMegaMergeReady()
        {
            _audioManager.PlayOneShot(SoundEventEnum.MegaMergeReady, isRelease: true);
        }

        private void OnMegaMergeUsed(MegaMergeUsedSignal s)
        {
            _audioManager.PlayOneShot(SoundEventEnum.MegaMergeUsed, isRelease: true);
        }

        private void OnMegaMergeChainCompleted(MergeChainCompletedSignal s)
        {
            _audioManager.PlayOneShot(SoundEventEnum.MergeChainCompleted, isRelease: true);
        }

        private void OnBonusUsed()
        {
            _audioManager.PlayOneShot(SoundEventEnum.BonusUsed, isRelease: true);
        }
    }
}