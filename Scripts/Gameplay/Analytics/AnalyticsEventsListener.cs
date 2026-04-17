using System;
using System.Linq;
using Gameplay.Shockwave2048.Board;
using PT.Backend.Interfaces;
using PT.Backend.Types;
using PT.Logic.Dependency.Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Analytics
{
    public class AnalyticsEventsListener : IInitializable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private IAnalyticsService _analytics;
        [Inject] private RunState _run;
        [Inject] private BoardState _board;

        public void Initialize()
        {
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<GameEndedSignal>(OnGameEnded);
            _signalBus.Subscribe<GameVictorySignal>(OnGameVictory);

            _signalBus.Subscribe<ShowAdSignal>(OnShowAd);

            _signalBus.Subscribe<UndoTurnSignal>(OnUndoUsed);
            _signalBus.Subscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Subscribe<GameTurnSignal>(OnGameTurn);

            _signalBus.Subscribe<MegaMergeReadySignal>(OnMegaMergeReady);
            _signalBus.Subscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);

            _signalBus.Subscribe<MergeChainCompletedSignal>(OnMergeChainCompleted);
            _signalBus.Subscribe<SkillUsedSignal>(OnSkillUsed);
            
            _signalBus.Subscribe<BonusesCreatedSignal>(OnBonusesCreated);
            _signalBus.Subscribe<BonusUsedSignal>(OnBonusUsed);
            
            _signalBus.Subscribe<GameAddCoinsSignal>(OnMoneyEarned);
            _signalBus.Subscribe<GameSpendCoinsSignal>(OnMoneySpent);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Unsubscribe<GameEndedSignal>(OnGameEnded);
            _signalBus.Unsubscribe<GameVictorySignal>(OnGameVictory);
            _signalBus.Unsubscribe<ShowAdSignal>(OnShowAd);
            _signalBus.Unsubscribe<UndoTurnSignal>(OnUndoUsed);
            _signalBus.Unsubscribe<PlayerTurnSignal>(OnPlayerTurn);
            _signalBus.Unsubscribe<GameTurnSignal>(OnGameTurn);
            _signalBus.Unsubscribe<MegaMergeReadySignal>(OnMegaMergeReady);
            _signalBus.Unsubscribe<MegaMergeUsedSignal>(OnMegaMergeUsed);
            _signalBus.Unsubscribe<MergeChainCompletedSignal>(OnMergeChainCompleted);
            _signalBus.Unsubscribe<SkillUsedSignal>(OnSkillUsed);
        }

        private void OnGameStarted()
        {
            _run.Reset();

            _analytics.Log(AnalyticsLogKeys.GameStarted, new()
            {
                { AnalyticsLogKeys.GridSizeX, _board.GridSize.x },
                { AnalyticsLogKeys.GridSizeY, _board.GridSize.y },
                { AnalyticsLogKeys.InitialGridCount, _board.GridCount }
            });
        }

        private void OnPlayerTurn()
        {
            _run.TurnsCount++;
            _run.LastTurnTimestamp = Time.time;
        }

        private void OnGameTurn()
        {
            _run.DecisionTimes.Add(Time.time - _run.LastTurnTimestamp);
        }

        private void OnUndoUsed()
        {
            _run.UndoUsedCount++;
        }

        private void OnMegaMergeReady()
        {
            _run.LastTurnReachedMegaMerge = _run.TurnsCount;
        }

        private void OnMegaMergeUsed(MegaMergeUsedSignal s)
        {
            _run.MegaMergeUsedAmount++;

            _analytics.Log(AnalyticsLogKeys.MegaMergeUsed, new()
            {
                { AnalyticsLogKeys.Turn, _run.TurnsCount },
                { AnalyticsLogKeys.Direction, s.Direction.ToString() },
                { AnalyticsLogKeys.UsedTotal, _run.MegaMergeUsedAmount },
                { AnalyticsLogKeys.TurnsOfKeepingMegaMerge, _run.LastTurnReachedMegaMerge - _run.TurnsCount }
            });
        }

        private void OnMergeChainCompleted(MergeChainCompletedSignal s)
        {
            _run.MaxChainLength = Mathf.Max(_run.MaxChainLength, s.ChainLength);
            _run.TotalChains++;

            _analytics.Log(AnalyticsLogKeys.MergeChainCompleted, new()
            {
                { AnalyticsLogKeys.ChainLength, s.ChainLength },
                { AnalyticsLogKeys.Turn, _run.TurnsCount }
            });
        }

        private void OnSkillUsed(SkillUsedSignal s)
        {
            _run.SkillsUsedCount++;
            _run.SkillUsageByType.TryAdd(s.SkillType, 0);
            _run.SkillUsageByType[s.SkillType]++;

            _analytics.Log(AnalyticsLogKeys.SkillUsed, new()
            {
                { AnalyticsLogKeys.SkillType, s.SkillType.ToString() },
                { AnalyticsLogKeys.Turn, _run.TurnsCount }
            });
        }

        private void OnBonusesCreated(BonusesCreatedSignal s)
        {
            _run.TotalBonusesCreated += s.Amount;
        }

        private void OnBonusUsed(BonusUsedSignal s)
        {
            _run.TotalBonusesUsed++;
        }

        private void OnMoneyEarned(GameAddCoinsSignal s)
        {
            _run.TotalMoneyEarned += s.Amount;
        }

        private void OnMoneySpent(GameSpendCoinsSignal s)
        {
            _run.TotalMoneySpent += s.Amount;
        }

        private void OnShowAd()
        {
            _run.AdsShownCount++;
        }

        private void OnGameVictory()
        {
            _run.IsVictory = true;
        }

        private void OnGameEnded()
        {
            _analytics.Log(AnalyticsLogKeys.RunEnded, new()
            {
                { AnalyticsLogKeys.Turns, _run.TurnsCount },
                { AnalyticsLogKeys.TotalChains, _run.TotalChains },
                { AnalyticsLogKeys.MaxChain, _run.MaxChainLength },
                { AnalyticsLogKeys.UndoUsed, _run.UndoUsedCount },
                { AnalyticsLogKeys.MegaMergeUsedTotal, _run.MegaMergeUsedAmount },
                { AnalyticsLogKeys.SkillsUsed, _run.SkillsUsedCount },
                { AnalyticsLogKeys.AverageDecisionTime, _run.DecisionTimes.Sum() / Math.Max(1, _run.DecisionTimes.Count) },
                { AnalyticsLogKeys.AdsShown, _run.AdsShownCount },
                { AnalyticsLogKeys.Victory, _run.IsVictory },
                { AnalyticsLogKeys.HighestElement, _board.HighestElementType.Value.ToString() },
                { AnalyticsLogKeys.FinalGridSize, $"{_board.GridSize.x}x{_board.GridSize.y}" },

                { AnalyticsLogKeys.BonusesCreated, _run.TotalBonusesCreated },
                { AnalyticsLogKeys.BonusesUsed, _run.TotalBonusesUsed },
                { AnalyticsLogKeys.BonusesUsedOutOfCreated,
                    _run.TotalBonusesCreated == 0
                        ? 0f
                        : (float)_run.TotalBonusesUsed / _run.TotalBonusesCreated },

                { AnalyticsLogKeys.MoneyEarned, _run.TotalMoneyEarned },
                { AnalyticsLogKeys.MoneySpent, _run.TotalMoneySpent },
                { AnalyticsLogKeys.MoneyDelta, _run.TotalMoneyEarned - _run.TotalMoneySpent }
            });
        }
    }
}
