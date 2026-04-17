#if UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PT.Backend.Interfaces;
using PT.Backend.Types;
using PT.Tools.Debugging;
using PT.Tools.Leaderboard;
using UnityEngine;
using UnityEngine.Networking;
using YG;
using YG.Utils.LB;
using Zenject;

namespace PT.Backend.YG
{
    public class YandexLeaderboardService : ILeaderboardService
    {
        [Inject] private LeaderboardConfig _leaderboardConfig;

        public UniTask SetScore(long score)
        {
            YG2.SetLeaderboard(_leaderboardConfig.LeaderboardId, (int)score);

            return UniTask.CompletedTask;
        }

        public async UniTask<LeaderboardSnapshot> GetTop(int count)
        {
            var tcs = new UniTaskCompletionSource<LeaderboardSnapshot>();

            void Handler(LBData data)
            {
                if (data.technoName != _leaderboardConfig.LeaderboardId) return;

                YG2.onGetLeaderboard -= Handler;

                var entries = data.players
                    .Take(count)
                    .Select(p => new LeaderboardEntry(
                        p.uniqueID,
                        p.name,
                        p.score,
                        p.rank
                    )).ToList();

                var playerRaw = data.players.FirstOrDefault(p => p.uniqueID == YG2.player.id);

                LeaderboardEntry playerEntry = playerRaw != null
                    ? new LeaderboardEntry(
                        playerRaw.uniqueID,
                        playerRaw.name,
                        playerRaw.score,
                        playerRaw.rank)
                    : new LeaderboardEntry();

                DebugManager.Log(DebugCategory.Leaderboards, $"Yandex LB loaded | Count: {entries.Count}");
                tcs.TrySetResult(new LeaderboardSnapshot(playerEntry, entries));
            }

            YG2.onGetLeaderboard += Handler;

            YG2.GetLeaderboard(_leaderboardConfig.LeaderboardId, count, 1, "small");

            return await tcs.Task;
        }

        public UniTask<LeaderboardSnapshot> GetAroundPlayer(int range)
        {
            return UniTask.FromResult(new LeaderboardSnapshot(new LeaderboardEntry(), new List<LeaderboardEntry>()));
        }
    }
}
#endif
