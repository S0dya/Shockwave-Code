using System.Collections.Generic;
using Gameplay.Shockwave2048.Board;
using MoreMountains.NiceVibrations;
using PT.Logic.Ads;
using PT.Logic.Configs;
using PT.Logic.Dependency.Signals;
using PT.Logic.Save;
using PT.Tools.CurrencyRelated;
using PT.Tools.Debugging;
using PT.Tools.Helper;
using UniRx;
using UnityEngine;
using Zenject;

namespace Gameplay.Shockwave2048.Skills
{
    public enum SkillTypeEnum
    {
        Swap,
        Destroy,
    }
    
    public class SkillsManager : MonoBehaviour
    {
         [SerializeField] private SerializableKeyValue<SkillTypeEnum, SkillView> views;

        [Inject] private GameConfig _gameConfig;
        [Inject] private CurrencyManager _currencyManager;
        [Inject] private AdsManager _adsManager;
        [Inject] private SignalBus _signalBus;
        [Inject] private BoardActionController _boardActionController;
        
        private readonly Dictionary<SkillTypeEnum, GameDataKey> _savingKeys = new()
        {
            { SkillTypeEnum.Swap, GameDataKey.SkillSwap },
            { SkillTypeEnum.Destroy, GameDataKey.SkillDestroy },
        };
        
        private SkillTypeEnum? _activeSkill;
        
        private readonly CompositeDisposable _disposable = new();
        
        private void Awake()
        {
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Subscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Subscribe<BoardActionPerformedSignal>(OnActionPerformed);
            
            foreach (var kvp in views.Dictionary)
            {
                var type = kvp.Key;
                var view = kvp.Value;
                
                view.Init(() => OnPressSkill(type), () => OnBuySkill(type), () => OnWatchAd(type),
                    _gameConfig.SkillsPrices.Dictionary[type]);

                view.Unlock();
                UpdateView(type);
            }

            _currencyManager.GetReactiveValue(CurrencyType.Gold)
                .Subscribe(val =>
                {
                    foreach (var kvp in views.Dictionary) UpdateView(kvp.Key);
                })
                .AddTo(_disposable);
        }
        
        private void OnPlayerTurn()
        {
            foreach (var view in views.Values) view.Unlock();
        }
        private void OnGameTurn()
        {
            foreach (var view in views.Values) view.Lock();
        }
        private void OnActionPerformed()
        {
            UseActiveSkill();
        }
        
        private void OnPressSkill(SkillTypeEnum type)
        {
            if (_activeSkill == type)
            {
                DebugManager.Log(DebugCategory.Skills, $"Deactivated {type}");
                _activeSkill = null;
                UpdateAllViews();
                
                _boardActionController.SetTap();
                return;
            }

            int amount = (int)GameDataRegistry.Get(_savingKeys[type]);
            if (amount <= 0)
            {
                DebugManager.Log(DebugCategory.Skills, $"No charges for {type}");
                return;
            }

            _activeSkill = type;
            DebugManager.Log(DebugCategory.Skills, $"Activated {type}");

            UpdateAllViews();

            switch (_activeSkill)
            {
                case SkillTypeEnum.Swap: _boardActionController.SetSwap(); break; 
                case SkillTypeEnum.Destroy: _boardActionController.SetDestroy(); break; 
            }
        }

        private void UseActiveSkill()
        {
            if (_activeSkill == null) return;

            var type = _activeSkill.Value;
            int amount = (int)GameDataRegistry.Get(_savingKeys[type]);

            if (amount <= 0)
            {
                _activeSkill = null;
                UpdateAllViews();
                return;
            }

            amount--;
            GameDataRegistry.Set(_savingKeys[type], amount);

            DebugManager.Log(DebugCategory.Skills, $"Used {type}, left {amount}");

            _activeSkill = null;
            UpdateAllViews();
            
            _signalBus.Fire(new SkillUsedSignal(type));
        }

        private void OnBuySkill(SkillTypeEnum type)
        {
            int price = _gameConfig.SkillsPrices.Dictionary[type];

            if (!_currencyManager.TrySpend(CurrencyType.Gold, price)) return;
            _signalBus.Fire(new GameSpendCoinsSignal(price));

            int amount = (int)GameDataRegistry.Get(_savingKeys[type]);
            amount++;

            GameDataRegistry.Set(_savingKeys[type], amount);
            UpdateView(type);
        }
        private void OnWatchAd(SkillTypeEnum type)
        {
            _adsManager.ShowRewardAd(() =>
            {
                var amount = _gameConfig.SkillsAdRewardAmount.Dictionary[type];

                GameDataRegistry.Set(_savingKeys[type], amount);
                UpdateView(type);
            });
        }

        private void UpdateAllViews()
        {
            foreach (var kv in views.Dictionary) UpdateView(kv.Key);
        }

        private void UpdateView(SkillTypeEnum type)
        {
            var view = views.Dictionary[type];
            int amount = (int)GameDataRegistry.Get(_savingKeys[type]);
            int price = _gameConfig.SkillsPrices.Dictionary[type];

            view.SetAmount(amount);

            if (_activeSkill == type)
            {
                view.SetSelected(true);
                return;
            }

            view.SetSelected(false);

            if (amount > 0)
            {
                view.ActivateState(SkillViewState.Use);
            }
            else if (_currencyManager.Get(CurrencyType.Gold) >= price)
            {
                view.ActivateState(SkillViewState.Buy);
            }
            else
            {
                view.ActivateState(SkillViewState.RewardAd);
            }
        }
        
        private void OnDestroy()
        {
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Unsubscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Unsubscribe<BoardActionPerformedSignal>(OnActionPerformed);
            _disposable.Dispose();
        }
    }
}