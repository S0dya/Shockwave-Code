using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Enums;
using PT.Tools.Sequences;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardProgressionView : MonoBehaviour
    {
        [SerializeField] private ElementView prevProgressionView;
        [SerializeField] private ElementView currentProgressionView;
        [SerializeField] private ElementView targetProgressionView;
        [Space]
        [SerializeField] private ParticleSystem[] nextProgressionReachedParticles;
        [SerializeField] private Sequencer[] progressionSequencers;
        [SerializeField] private ElementType[] skipEffectsTypeReached;
        
        [Inject] private BoardState _boardState; 
        
        private readonly CompositeDisposable _disposables = new ();
        
        public void SetProgression(ElementData prevValue, ElementData currentValue, ElementData targetValue)
        {
            prevProgressionView.Set(prevValue);
            currentProgressionView.Set(currentValue);
            targetProgressionView.Set(targetValue);
            
            
            _boardState.HighestElementType
                .Subscribe(OnNextHighestElementTypeReached)
                .AddTo(_disposables);
        }

        private void OnNextHighestElementTypeReached(ElementType elementType)
        {
            if (skipEffectsTypeReached.Contains(elementType)) return;

            // foreach (var sequencer in progressionSequencers) sequencer.Skip();
            
            foreach (var particle in nextProgressionReachedParticles) particle.Play();
            foreach (var sequencer in progressionSequencers) sequencer.Play().Forget();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}