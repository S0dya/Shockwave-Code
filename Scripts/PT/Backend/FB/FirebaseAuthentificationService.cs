#if UNITY_ANDROID || UNITY_IOS
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using PT.Backend.Interfaces;
using PT.Tools.Debugging;
using UnityEngine;
#endif

namespace PT.Backend.FB
{
#if UNITY_ANDROID || UNITY_IOS
    public class FirebaseAuthentificationService : IAuthentificationService
    {
        public bool IsSignedIn => _user != null;
        public string PlayerId => _user?.UserId;
        public string DisplayName => _user?.DisplayName;
        
        private readonly FirebaseAuth _auth = FirebaseAuth.DefaultInstance;
        private FirebaseUser _user;

        public async UniTask SignIn()
        {
            if (_auth.CurrentUser != null)
            {
                _user = _auth.CurrentUser;
                DebugManager.Log(DebugCategory.Backend, $"Already signed in anonymously: {_user.UserId}");
                return;
            }

            var result = await _auth.SignInAnonymouslyAsync();
            _user = result.User;
            
            await EnsureDisplayName();

            DebugManager.Log(DebugCategory.Backend, $"Anonymous sign-in success: {_user.UserId}");
        }
        
        public async UniTask SetDisplayName(string newName)
        {
            if (_user == null)
            {
                DebugManager.Log(DebugCategory.Backend, "SetDisplayName called before sign-in", LogType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(newName)) return;

            var profile = new UserProfile
            {
                DisplayName = newName
            };

            await _user.UpdateUserProfileAsync(profile);

            DebugManager.Log(DebugCategory.Backend, $"Display name updated: {newName}");
        }

        private async UniTask EnsureDisplayName()
        {
            if (!string.IsNullOrEmpty(_user.DisplayName)) return;

            await SetDisplayName(GenerateDefaultName(_user.UserId));
        }

        private static string GenerateDefaultName(string userId) //change later
        {
            return $"Player_{userId.Substring(0, 4)}";
        }
    }
#endif
}