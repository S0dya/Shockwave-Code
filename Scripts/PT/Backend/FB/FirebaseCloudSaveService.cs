#if UNITY_IOS || UNITY_ANDROID

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using PT.Backend.Interfaces;
using PT.Logic.Save;
using PT.Tools.Debugging;
using UnityEngine;
using Zenject;

namespace PT.Backend.FB
{
    public class FirebaseCloudSaveService : ICloudSaveService
    {
        private const string UsersCollection = "users";
        private const string SaveDocument = "save";

        [Inject] private FirebaseBackendService _backendService;
        
        public async UniTask<SaveData> Load()
        {
            if (!_backendService.IsReady) return null;

            try
            {
                var snapshot = await GetSaveDoc().GetSnapshotAsync();

                if (!snapshot.Exists) return null;

                DebugManager.Log(DebugCategory.Backend, "Cloud save loaded");
                return Deserialize(snapshot);
            }
            catch (System.Exception e)
            {
                DebugManager.Log(DebugCategory.Backend, $"Load failed: {e.Message}", LogType.Warning);
                return null;
            }
        }

        public async UniTask Save(SaveData data)
        {
            if (!_backendService.IsReady || data == null) return;

            try
            {
                var payload = Serialize(data);
                payload["updatedAt"] = FieldValue.ServerTimestamp;

                await GetSaveDoc().SetAsync(payload);

                DebugManager.Log(DebugCategory.Backend, "Cloud save uploaded");
            }
            catch (System.Exception e)
            {
                DebugManager.Log(DebugCategory.Backend, $"Save failed: {e.Message}", LogType.Warning);
            }
        }

        private DocumentReference GetSaveDoc()
        {
            return _backendService.Firestore
                .Collection(UsersCollection)
                .Document(_backendService.Auth.CurrentUser.UserId)
                .Collection("data")
                .Document(SaveDocument);
        }

        private Dictionary<string, object> Serialize(SaveData data)
        {
            return new Dictionary<string, object>
            {
                { "ints", data.IntDict },
                { "floats", data.FloatDict },
                { "bools", data.BoolDict },
                { "strings", data.StringDict }
            };
        }

        private SaveData Deserialize(DocumentSnapshot snapshot)
        {
            var ints = snapshot.ContainsField("ints")
                ? snapshot.GetValue<Dictionary<string, int>>("ints")
                : new Dictionary<string, int>();

            var floats = snapshot.ContainsField("floats")
                ? snapshot.GetValue<Dictionary<string, float>>("floats")
                : new Dictionary<string, float>();

            var bools = snapshot.ContainsField("bools")
                ? snapshot.GetValue<Dictionary<string, bool>>("bools")
                : new Dictionary<string, bool>();

            var strings = snapshot.ContainsField("strings")
                ? snapshot.GetValue<Dictionary<string, string>>("strings")
                : new Dictionary<string, string>();
            
            var updatedAt = 0d;

            if (snapshot.ContainsField("updatedAt"))
            {
                var raw = snapshot.GetValue<object>("updatedAt");

                if (raw is Timestamp ts)
                {
                    updatedAt = ts.ToDateTime().ToUniversalTime()
                        .Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc))
                        .TotalSeconds;
                }
            }

            return new SaveData(ints, floats, bools, strings, (long)updatedAt);
        }
    }
}
#endif