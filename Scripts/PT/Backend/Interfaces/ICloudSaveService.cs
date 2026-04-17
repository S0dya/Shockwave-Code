using Cysharp.Threading.Tasks;
using PT.Logic.Save;

namespace PT.Backend.Interfaces
{
    public interface ICloudSaveService
    {
        UniTask Save(SaveData saveData);
        UniTask<SaveData> Load();
    }
}