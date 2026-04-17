using System.Collections.Generic;
using Gameplay.Shockwave2048.Enums;
using Gameplay.Shockwave2048.Skills;

namespace Gameplay.Analytics
{
    public class RunState
    {
        public int TurnsCount;
        public int MaxChainLength;
        public int TotalChains;

        public int UndoUsedCount;
        public int MegaMergeUsedAmount;

        public float LastTurnTimestamp;
        public int LastTurnReachedMegaMerge;

        public int AdsShownCount;
        public int SkillsUsedCount;
        public bool IsVictory;

        public Dictionary<SkillTypeEnum, int> SkillUsageByType = new();

        public readonly List<float> DecisionTimes = new();
        
        public int TotalBonusesCreated;
        public int TotalBonusesUsed;
        
        public int TotalMoneyEarned;
        public int TotalMoneySpent;

        public void Reset()
        {
            TurnsCount = 0;
            MaxChainLength = 0;
            TotalChains = 0;
            UndoUsedCount = 0;
            MegaMergeUsedAmount = 0;
            AdsShownCount = 0;
            SkillsUsedCount = 0;
            IsVictory = false;
            SkillUsageByType.Clear();
        }
    }
}