using System;

namespace PT.Backend.Types
{
    [Serializable]
    public class LeaderboardRecord
    {
        public string PlayerId { get; }
        public string DisplayName { get; }
        public long Score { get; }
        public long UpdatedAt { get; }

        public LeaderboardRecord(string playerId, string displayName, long score, long updatedAt)
        {
            PlayerId = playerId;
            DisplayName = displayName;
            Score = score;
            UpdatedAt = updatedAt;
        }
    }
}