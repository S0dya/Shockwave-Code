using System.Collections.Generic;

namespace PT.Backend.Interfaces
{
    public interface IAnalyticsService
    {
        void Log(string eventName);
        void Log(string eventName, Dictionary<string, object> parameters);
    }
}