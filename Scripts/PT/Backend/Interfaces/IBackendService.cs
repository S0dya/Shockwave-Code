using Cysharp.Threading.Tasks;

namespace PT.Backend.Interfaces
{
    public interface IBackendService
    {
        public bool IsReady { get; }
        
        public UniTask Init();
    }
}