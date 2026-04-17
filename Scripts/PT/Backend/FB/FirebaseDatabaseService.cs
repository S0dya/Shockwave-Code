#if UNITY_ANDROID || UNITY_IOS
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using Newtonsoft.Json;
using PT.Backend.Interfaces;
using PT.Backend.Types;
using PT.Tools.Debugging;
using UnityEngine;
using Zenject;

namespace PT.Backend.FB
{
    public class FirebaseDatabaseService : IDatabaseService, IInitializable
    {
        private DatabaseReference _databaseRef;
        
        public void Initialize()
        {
            _databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        
        public async UniTask<T> GetData<T>(string path)
        {
            try
            {
                var snapshot = await _databaseRef.Child(path).GetValueAsync();
        
                if (!snapshot.Exists)
                {
                    DebugManager.Log(DebugCategory.Backend, "Snapshot doesn't exist: " + snapshot, LogType.Error);
                    
                    return default;
                }
                
                var json = snapshot.GetRawJsonValue();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                DebugManager.Log(DebugCategory.Backend, $"Exception when trying to GetData: {e.Message}", LogType.Error);
                
                return default;
            }
        }

        public async UniTask<bool> SetDataAsync<T>(string path, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                await _databaseRef.Child(path).SetRawJsonValueAsync(json).AsUniTask();

                return true;
            }
            catch (Exception e)
            {
                DebugManager.Log(DebugCategory.Backend, $"SetDataAsync Error: {e.Message}", LogType.Error);

                return false;
            }
        }
        
        public async UniTask<IReadOnlyList<T>> Query<T>(DatabaseQuery query)
        {
            try
            {
                Query fbQuery = _databaseRef.Child(query.Path);
                
                if (!string.IsNullOrEmpty(query.OrderBy)) 
                    fbQuery = fbQuery.OrderByChild(query.OrderBy);
                
                if (query.Limit > 0) 
                    fbQuery = fbQuery.LimitToLast(query.Limit);
                
                var snapshot = await fbQuery.GetValueAsync();

                if (!snapshot.Exists) return Array.Empty<T>();
                
                var list = new List<T>();
                
                foreach (var child in snapshot.Children)
                {
                    var json = child.GetRawJsonValue();
                    
                    if (string.IsNullOrEmpty(json)) continue;
                    
                    list.Add(JsonConvert.DeserializeObject<T>(json));
                }
                
                if (query.Descending) list.Reverse();
                
                return list;
            }
            catch (Exception e)
            {
                DebugManager.Log(DebugCategory.Backend, $"Query failed at {query.Path}: {e.Message}", LogType.Error);

                return Array.Empty<T>();
            }
        }
    }
}
#endif