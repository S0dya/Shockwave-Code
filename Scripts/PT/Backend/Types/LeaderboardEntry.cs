using UnityEngine;

namespace PT.Backend.Types
{
    public readonly struct LeaderboardEntry
    {
        public string PlayerId { get; }
        public string DisplayName { get; }
        public long Score { get; }
        public int Rank { get; }
        public Sprite Icon { get; }

        public LeaderboardEntry(string playerId, string displayName, long score, int rank, Sprite icon = null)
        {
            PlayerId = playerId;
            DisplayName = displayName;
            Score = score;
            Rank = rank;
            Icon = icon;
        }
    }
}