namespace PT.Backend.Types
{
    public class AnalyticsLogKeys
    {
        // Events
        public const string GameStarted = "game_started";
        public const string RunEnded = "run_ended";
        public const string MegaMergeUsed = "mega_merge_used";
        public const string MergeChainCompleted = "merge_chain_completed";
        public const string SkillUsed = "skill_used";

        // Params
        public const string GridSizeX = "grid_size_x";
        public const string GridSizeY = "grid_size_y";
        public const string InitialGridCount = "initial_grid_count";

        public const string Turn = "turn";
        public const string Direction = "direction";
        public const string UsedTotal = "used_total";
        public const string TurnsOfKeepingMegaMerge = "turns_of_keeping_mega_merge";

        public const string ChainLength = "chain_length";
        public const string SkillType = "skill_type";

        public const string Turns = "turns";
        public const string TotalChains = "total_chains";
        public const string MaxChain = "max_chain";
        public const string UndoUsed = "undo_used";
        public const string MegaMergeUsedTotal = "mega_merge_used";
        public const string SkillsUsed = "skills_used";
        public const string AverageDecisionTime = "average_decision_time";
        public const string AdsShown = "ads_shown";
        public const string Victory = "victory";
        public const string HighestElement = "highest_element";
        public const string FinalGridSize = "final_grid_size";

        public const string BonusesCreated = "bonuses_created";
        public const string BonusesUsed = "bonuses_used";
        public const string BonusesUsedOutOfCreated = "bonuses_used_out_of_created";

        public const string MoneyEarned = "money_earned";
        public const string MoneySpent = "money_spent";
        public const string MoneyDelta = "money_delta";
        
        public const string NewRankReached = "new_rank_reached";
        public const string Rank = "Rank";
    }
}