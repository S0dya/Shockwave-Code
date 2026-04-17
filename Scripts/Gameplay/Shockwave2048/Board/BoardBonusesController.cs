using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Shockwave2048.Elements;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Tools.CurrencyRelated;
using PT.Tools.Debugging;
using PT.Tools.Helper;
using Zenject;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardBonusesController : IInitializable, IDisposable
    {
        [Inject] private BoardState _state;
        [Inject] private GameConfig _gameConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private CurrencyManager _currencyManager;

        private int _currentBonusesTurnStep;
        private int _currentBonusesTurnTarget;
        
        private readonly List<Element> _currentElementsWithBonuses = new();
        
        public void Initialize()
        {
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);

            _currentBonusesTurnTarget = _gameConfig.BonusesAddedTurnsRange.GetRandomValue();
        }
        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
        }

        private void OnPlayerTurn()
        {
            ClearBonuses();
            
            if (_currentBonusesTurnStep >= _currentBonusesTurnTarget)
            {
                _currentBonusesTurnStep = 0;
                _currentBonusesTurnTarget = _gameConfig.BonusesAddedTurnsRange.GetRandomValue();

                var activeElements = _state.CellStates.Values
                    .Where(c => c.Element != null)
                    .Select(c => c.Element)
                    .ToList();

                int bonusAmount = GetBonusAmountForActiveElements(activeElements.Count);

                DebugManager.Log(DebugCategory.Gameplay, $"Bonuses to assign: {bonusAmount}");

                for (int i = 0; i < bonusAmount; i++)
                {
                    var element = activeElements.GetRandomElement();

                    element.ActivateBonus();
                    _currentElementsWithBonuses.Add(element);
                }
                
                _signalBus.Fire(new BonusesCreatedSignal(bonusAmount));
            }
            else
            {
                _currentBonusesTurnStep++;
            }
        }

        public void BonusUsed()
        {
            int gold = _gameConfig.GoldBonusAmountRange.GetRandomValue();
            
            DebugManager.Log(DebugCategory.Gameplay, $"BonusUsed → +{gold} gold");

            _currencyManager.Add(CurrencyType.Gold, gold);
            
            _signalBus.Fire(new GameAddCoinsSignal(gold));
        }

        private void ClearBonuses()
        {
            foreach (var element in _currentElementsWithBonuses)
            {
                if (element == null || !element.gameObject.activeSelf) continue;

                element.DeactivateBonus();
            }

            _currentElementsWithBonuses.Clear();
        }
        
        private int GetBonusAmountForActiveElements(int activeCount)
        {
            int result = 0;

            foreach (var kvp in _gameConfig.BonusThresholds.Dictionary)
            {
                if (activeCount >= kvp.Key && kvp.Value > result)
                    result = kvp.Value;
            }

            return result;
        }
    }
}
