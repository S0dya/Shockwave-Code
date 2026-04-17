using System;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Enums;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using UniRx;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardProgressionController : IInitializable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _state;
        [Inject] private BoardProgressionView _view;
        [Inject] private ElementProvider _elementProvider;

        private readonly CompositeDisposable _disposables = new ();
        
        public void Initialize()
        {
            _state.HighestElementType
                .Subscribe(SetNewProgression)
                .AddTo(_disposables);
        }

        private void SetNewProgression(ElementType elementType)
        {
                _view.SetProgression(
                    _elementProvider.GetPrevious(elementType), 
                    _elementProvider.GetData(elementType), 
                    _elementProvider.GetNext(elementType));
            
            //handle no prev element?
        }
        
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}