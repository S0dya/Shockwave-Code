#if UNITY_WEBGL
using Cysharp.Threading.Tasks;
using PT.Backend.Interfaces;
using PT.Tools.Debugging;
using YG;
#endif

namespace PT.Backend.YG
{
#if UNITY_WEBGL 
    public class YandexAuthentificationService : IAuthentificationService
    {
        public bool IsSignedIn { get; private set; }
        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }

        public async UniTask SignIn()
        {
            if (IsSignedIn) return;

            await UniTask.WaitUntil(() => YG2.player != null);

            PlayerId = YG2.player.id;
            DisplayName = YG2.player.name;

            IsSignedIn = true;

            DebugManager.Log(DebugCategory.Backend, $"Yandex signed in: {DisplayName} ({PlayerId})");
        }

        public UniTask SetDisplayName(string name)
        {
            return UniTask.CompletedTask;
        }
    }
#endif
}