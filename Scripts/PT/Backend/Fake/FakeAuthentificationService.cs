using Cysharp.Threading.Tasks;
using PT.Backend.Interfaces;
using PT.Logic.Save;
using PT.Tools.Leaderboard;
using Zenject;

namespace PT.Backend.Fake
{
    public class FakeAuthentificationService : IAuthentificationService
    {
        [Inject] private LeaderboardConfig _leaderboardConfig;

        public bool IsSignedIn => !string.IsNullOrEmpty(DisplayName); 
        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }
        
        public UniTask SignIn()
        {
            if (string.IsNullOrEmpty((string)GameDataRegistry.Get(GameDataKey.LeaderboardPlayerName)))
            {
                GameDataRegistry.Set(GameDataKey.LeaderboardPlayerName, _leaderboardConfig.InitialPlayerName);
                GameDataRegistry.Set(GameDataKey.LeaderboardPlayerRank, _leaderboardConfig.InitialPlayerRank);
            }

            DisplayName = (string)GameDataRegistry.Get(GameDataKey.LeaderboardPlayerName);
            PlayerId = (string)GameDataRegistry.Get(GameDataKey.LeaderboardPlayerName);
            PlayerId = _leaderboardConfig.PlayerId;

            return UniTask.CompletedTask;
        }

        public UniTask SetDisplayName(string name)
        {
            GameDataRegistry.Set(GameDataKey.LeaderboardPlayerName, name);
            
            return UniTask.CompletedTask;
        }
    }
}