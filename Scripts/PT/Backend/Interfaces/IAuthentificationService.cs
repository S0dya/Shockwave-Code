using Cysharp.Threading.Tasks;

namespace PT.Backend.Interfaces
{
    public interface IAuthentificationService
    {
        bool IsSignedIn { get; }

        string PlayerId { get; }
        string DisplayName { get; }
        
        UniTask SignIn();
        UniTask SetDisplayName(string name);
    }
}