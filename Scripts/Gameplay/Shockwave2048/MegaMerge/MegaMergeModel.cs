using System;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.MegaMerge
{
    public class MegaMergeModel : IInitializable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private GameConfig _config;

        public readonly ReactiveProperty<float> Charge = new(0f);
        private readonly float _max = 1f;

        public void Initialize()
        {
            _signalBus.Subscribe<GridElementPushedSignal>(OnMergePush);
            
            AddCharge(_config.MegaMergeChargePerMerge);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GridElementPushedSignal>(OnMergePush);
        }

        private void OnMergePush()
        {
            AddCharge(_config.MegaMergeChargePerMerge);
        }

        public void AddCharge(float amount)
        {
            var lastValue = Charge.Value;
            
            Charge.Value = Mathf.Clamp01(Charge.Value + amount);

            if (lastValue < _max && Charge.Value >= _max) _signalBus.Fire(new MegaMergeReadySignal());
        }
        
        internal void SetClampedCharge(float value)
        {
            Charge.Value = value;
        }

        public bool TryConsume()
        {
            if (Charge.Value < _max) return false;

            Charge.Value = 0f;
            return true;
        }
        
        public bool IsFullCharged() => Charge.Value >= _max;
    }
}