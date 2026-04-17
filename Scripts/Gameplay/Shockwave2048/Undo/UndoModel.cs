using UniRx;

namespace Gameplay.Shockwave2048.Undo
{
    public class UndoModel
    {
        public const int StartAmount = 3;
        public const int AdRewardAmount = 5;

        public ReactiveProperty<int> Amount { get; } = new();

        public void Init()
        {
            Amount.Value = StartAmount;
        }

        public bool TryUse()
        {
            if (Amount.Value <= 0) return false;
            Amount.Value--;
            return true;
        }

        public void AddFromAd()
        {
            Amount.Value += AdRewardAmount;
        }
    }
}