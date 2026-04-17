#if UNITY_IOS || UNITY_ANDROID
using Firebase.RemoteConfig;
using PT.Backend.Interfaces;
using Zenject;

namespace PT.Backend.FB
{
    public class FirebaseRemoteConfigService : IRemoteConfigService
    {
        [Inject] private IBackendService _backend;
        
        public int GetInt(string key, int defaultValue)
        {
            return FirebaseRemoteConfig.DefaultInstance.AllValues.TryGetValue(key, out var val)
                ? (int)val.LongValue
                : defaultValue;
        }

        public float GetFloat(string key, float defaultValue)
        {
            return FirebaseRemoteConfig.DefaultInstance.AllValues.TryGetValue(key, out var val)
                ? (float)val.DoubleValue
                : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return FirebaseRemoteConfig.DefaultInstance.AllValues.TryGetValue(key, out var val)
                ? val.BooleanValue
                : defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            return FirebaseRemoteConfig.DefaultInstance.AllValues.TryGetValue(key, out var val)
                ? val.StringValue
                : defaultValue;
        }
    }
}
#endif