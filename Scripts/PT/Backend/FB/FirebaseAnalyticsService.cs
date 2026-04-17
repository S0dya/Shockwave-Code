#if UNITY_IOS || UNITY_ANDROID
using System.Collections.Generic;
using Firebase.Analytics;
using PT.Backend.Interfaces;
using Zenject;

namespace PT.Backend.FB
{
    public class FirebaseAnalyticsService : IAnalyticsService
    {
        [Inject] private IBackendService _backendService;
        
        public void Log(string eventName)
        {
            if (!_backendService.IsReady) return;
            
            FirebaseAnalytics.LogEvent(eventName);
        }

        public void Log(string eventName, Dictionary<string, object> parameters)
        {
            if (!_backendService.IsReady) return;

            var list = new List<Parameter>();

            foreach (var kv in parameters)
            {
                if (kv.Value is int i) list.Add(new Parameter(kv.Key, i));
                else if (kv.Value is long l) list.Add(new Parameter(kv.Key, l));
                else if (kv.Value is float f) list.Add(new Parameter(kv.Key, f));
                else if (kv.Value is string s) list.Add(new Parameter(kv.Key, s));
            }

            FirebaseAnalytics.LogEvent(eventName, list.ToArray());
        }
    }
}
#endif