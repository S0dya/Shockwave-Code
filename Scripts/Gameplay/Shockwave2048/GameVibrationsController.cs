using System;
using Gameplay.Shockwave2048.Board;
using MoreMountains.NiceVibrations;
using PT.Logic.Dependency.Signals;
using PT.Tools.Vibrations;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048
{
    public class GameVibrationsController : IInitializable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private VibrationManager _vibration;

        public void Initialize()
        {
            _signalBus.Subscribe<BoardActionPerformedSignal>(OnBoardActionPerformed);
            _signalBus.Subscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Subscribe<MergeChainCompletedSignal>(OnMergeChainCompleted);
            _signalBus.Subscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);
            _signalBus.Subscribe<BonusUsedSignal>(OnBonusUsed);
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<BoardActionPerformedSignal>(OnBoardActionPerformed);
            _signalBus.Unsubscribe<BoardMergeSignal>(OnMerge);
            _signalBus.Unsubscribe<MergeChainCompletedSignal>(OnMergeChainCompleted);
            _signalBus.Unsubscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);
            _signalBus.Unsubscribe<BonusUsedSignal>(OnBonusUsed);
        }
        
        private void OnBoardActionPerformed(BoardActionPerformedSignal s)
        {
            _vibration.Vibrate(HapticTypes.MediumImpact);
        }
        
        private void OnMerge(BoardMergeSignal s)
        {
            _vibration.Vibrate(HapticTypes.LightImpact);
            
            // int step = Mathf.Clamp(_state.MergeStep, 1, 8);
            // float t = (step - 1) / 7f; 
            //
            // float intensity = Mathf.Lerp(0.12f, 0.22f, t);
            // float duration  = Mathf.Lerp(0.005f, 0.015f, t);
            // float sharpness = Mathf.Lerp(0.85f, 0.55f, t);
            //
            // _vibration.Vibrate(intensity, duration, sharpness);
        }

        private void OnMergeChainCompleted(MergeChainCompletedSignal s)
        {
            _vibration.Vibrate(HapticTypes.MediumImpact);
        }
        
        private void OnMegaMergeUsed(MegaMergeUsedSignal s)
        {
            _vibration.Vibrate(HapticTypes.Failure);
        }

        private void OnBonusUsed()
        {
            // _vibration.Vibrate(HapticTypes.SoftImpact);
        }
    }
}