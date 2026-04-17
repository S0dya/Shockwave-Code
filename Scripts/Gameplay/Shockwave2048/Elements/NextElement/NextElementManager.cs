using System;
using Gameplay.Shockwave2048.Board;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Debugging;
using PT.Tools.Helper;
using UniRx;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Elements.NextElement
{
    public class NextElementManager : MonoBehaviour
    {
        [SerializeField] private ElementView nextElementView;
        
        [Inject] private ElementProvider _elementProvider;
        [Inject] private GameConfig _gameConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardState _boardState;

        private void Awake()
        {
            _signalBus.Subscribe<PlayerTurnSignal>(SetNextElement);

            _boardState.NextElementData.Subscribe(SetNextElement).AddTo(this);
        }
        
        private void SetNextElement(ElementData elementData)
        {
            nextElementView.Set(elementData, true);
        }

        private void SetNextElement()
        {
            var type = _gameConfig.PlayableElementTypes.GetRandomElement();
            _boardState.NextElementData.Value = _elementProvider.GetData(type);

            DebugManager.Log(DebugCategory.Gameplay,
                $"Setting Next Element : {_boardState.NextElementData.Value.ElementTypeInfo.ElementType}, " +
                $"Directions : {String.Join(" ", _boardState.NextElementData.Value.PushDirections)}");
            
            _signalBus.Fire(new NextElementSetSignal());
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<PlayerTurnSignal>(SetNextElement);
        }
    }
}
