#if UNITY_IOS || UNITY_ANDROID

using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.RemoteConfig;
using PT.Backend.Interfaces;
using PT.Tools.Debugging;
using UnityEngine;

namespace PT.Backend.FB
{
    public class FirebaseBackendService : IBackendService
    {
        public bool IsReady { get; private set; }
        
        public FirebaseAuth Auth { get; private set; }
        public FirebaseFirestore Firestore { get; private set; }
        
        private const int MaxRetries = 3;
        private const float TimeoutSeconds = 6f;

        public async UniTask Init()
        {
            if (IsReady) return;

            DebugManager.Log(DebugCategory.Backend, "Init started");

            var initTask = TryInitWithRetries();
            var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(TimeoutSeconds));

            var winner = await UniTask.WhenAny(initTask, timeoutTask);

            if (winner == 1)
            {
                DebugManager.Log(DebugCategory.Backend, "Init TIMEOUT → OFFLINE MODE", LogType.Warning);
            }
        }

        private async UniTask TryInitWithRetries()
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    DebugManager.Log(DebugCategory.Backend, $"Attempt {attempt}");

                    var status = await FirebaseApp.CheckAndFixDependenciesAsync();
                    if (status != DependencyStatus.Available) throw new Exception($"Deps failed: {status}");

                    Auth = FirebaseAuth.DefaultInstance;
                    if (Auth.CurrentUser == null) await Auth.SignInAnonymouslyAsync();

                    Firestore = FirebaseFirestore.DefaultInstance;

                    try
                    {
                        await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
                        await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                    }
                    catch (Exception e)
                    {
                        DebugManager.Log(DebugCategory.Backend, $"RC failed: {e.Message}", LogType.Warning);
                    }

                    IsReady = true;
                    DebugManager.Log(DebugCategory.Backend, "READY");
                    return;
                }
                catch (Exception e)
                {
                    DebugManager.Log(DebugCategory.Backend, $"Attempt {attempt} failed: {e.Message}", LogType.Warning);

                    await UniTask.Delay(500);
                }
            }

            DebugManager.Log(DebugCategory.Backend, "All retries failed → OFFLINE MODE", LogType.Warning);
        }
    }
}
#endif