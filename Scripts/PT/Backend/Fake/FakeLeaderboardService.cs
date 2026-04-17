using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PT.Backend.Interfaces;
using PT.Backend.Types;
using PT.Logic.Save;
using PT.Tools.Addressables;
using PT.Tools.Debugging;
using PT.Tools.Helper;
using PT.Tools.Leaderboard;
using PT.Tools.Other;
using UnityEngine;
using Zenject;

namespace PT.Backend.Fake
{
    public class FakeLeaderboardService : ILeaderboardService
    {
        [Inject] private LeaderboardConfig _leaderboardConfig;
        [Inject] private IAssetResolver _assetResolver;
        [Inject] private IAuthentificationService _authentificationService;

        private List<string> _names;
        
        private long PlayerScore => GameData.HighestScore;

        public UniTask SetScore(long score)
        {
            if (score > GameData.HighestScore)
                GameData.HighestScore = (int)score;

            return UniTask.CompletedTask;
        }

        public UniTask<LeaderboardSnapshot> GetTop(int count)
        {
            var top = BuildTop();
            var player = BuildPlayer();
            return UniTask.FromResult(new LeaderboardSnapshot(player, top));
        }

        public async UniTask<LeaderboardSnapshot> GetAroundPlayer(int range)
        {
            EnsureLoaded();
            
            var top = BuildTop();
            var player = BuildPlayer();

            if (player.Rank <= top.Count)
            {
                DebugManager.Log(DebugCategory.Leaderboards, $"Player already in TOP ({player.Rank}), skipping dynamic leaderboard");
                
                return await UniTask.FromResult(new LeaderboardSnapshot(player, top));
            }

            var window = BuildAround(player.Rank, range, top.Count);
            
            DebugManager.Log(DebugCategory.Leaderboards, $"Fake around window built | Rank: {player.Rank} | Window size: {window.Count}");
            
            return await UniTask.FromResult(new LeaderboardSnapshot(player, window));
        }

        private List<LeaderboardEntry> BuildTop()
        {
            var list = new List<LeaderboardEntry>();
            int rank = 1;

            foreach (var e in _leaderboardConfig.Entries.Dictionary)
            {
                list.Add(new LeaderboardEntry($"top_{rank}", e.Key, e.Value, rank));
                rank++;
            }

            return list;
        }

        private LeaderboardEntry BuildPlayer()
        {
            var top = BuildTop();
            var betterScoreTopAmount = top.Count(x => x.Score > PlayerScore);
            int rank = -1;

            if (top.Count - 1 > betterScoreTopAmount)
            {
                rank = Mathf.Max(betterScoreTopAmount + 1, 1);
                
                DebugManager.Log(DebugCategory.Leaderboards, $"Player is among TOP players | in front of player : {betterScoreTopAmount}");
            }
            else
            {
                rank = Math.Max(
                    _leaderboardConfig.InitialPlayerRank - (int)(PlayerScore / _leaderboardConfig.RankAdditionDelta),
                    1
                );   
            }
            
            return new LeaderboardEntry(_leaderboardConfig.PlayerId, _authentificationService.DisplayName, PlayerScore, rank);
        }

        private IReadOnlyList<LeaderboardEntry> BuildAround(int playerRank, int range, int topCount)
        {
            var list = new List<LeaderboardEntry>();
            int nameIdx = playerRank;

            long scoreAbove = PlayerScore;
            for (int r = playerRank - 1; r >= playerRank - range; r--)
            {
                if (r <= topCount) break;

                scoreAbove += _leaderboardConfig.ScoreAddition.GetRandomValue();

                list.Add(new LeaderboardEntry($"fake_{r}", GetName(), scoreAbove, r));
            }

            long scoreBelow = PlayerScore;
            for (int r = playerRank + 1; r <= playerRank + range; r++)
            {
                scoreBelow = Math.Max(scoreBelow - _leaderboardConfig.ScoreAddition.GetRandomValue(), 0);

                list.Add(new LeaderboardEntry($"fake_{r}", GetName(), scoreBelow, r));
            }

            list.Sort((a, b) => a.Rank.CompareTo(b.Rank));

            return list;

            string GetName() => _names[nameIdx++ % _names.Count];
        }

        private void EnsureLoaded()
        {
            if (_names != null) return;
            
            var asset = _assetResolver.Get<TextAsset>(AssetKey.LeaderboardFakeNicknames);
            
            _names = asset.text
                .Split('\n')
                .Select(n => n.Trim())
                .Where(n => n.Length > 0)
                .ToList();
            
            DebugManager.Log(DebugCategory.Leaderboards, $"Loaded {_names.Count} fake nicknames");
        }
    }
}