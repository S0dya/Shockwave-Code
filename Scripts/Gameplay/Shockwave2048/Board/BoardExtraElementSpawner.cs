using System;
using System.Linq;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Elements.NextElement;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.Helper;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardExtraElementSpawner : IInitializable, IDisposable
    {
        [Inject] private BoardState _state;
        [Inject] private GameConfig _gameConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardMoveMergeController _moveMergeController;
        [Inject] private ElementProvider _elementProvider;
        
        public void Initialize()
        {
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
        }
        
        private void OnPlayerTurn()
        {
            var spawnablePositions = _state.CellStates
                .Where(kvp => kvp.Value.Slot.GetActive() && kvp.Value.Element == null)
                .Select(kvp => kvp.Key)
                .ToList();

            if (spawnablePositions.Count < _gameConfig.ExtraElementSpawnFreeSlotsNeeded) return;
            
            _moveMergeController.InstantiateElementAt(
                spawnablePositions.GetRandomElement(), 
                _elementProvider.GetData(_gameConfig.PlayableElementTypes.GetRandomElement()));
        }
    }
}