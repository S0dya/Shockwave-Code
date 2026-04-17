using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PT.Backend.Types;

namespace PT.Backend.Interfaces
{
    public interface IDatabaseService
    {
        UniTask<T> GetData<T>(string path);
        UniTask<bool> SetDataAsync<T>(string path, T data);
        UniTask<IReadOnlyList<T>> Query<T>(DatabaseQuery query);
    }
}