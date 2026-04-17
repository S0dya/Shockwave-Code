namespace Gameplay.Shockwave2048.Slot
{
    public class GridSlotData
    {
        public bool IsActive;
        
        public GridSlotData GetCopy()
        {
            return new GridSlotData
            {
                IsActive = IsActive
            };
        }
    }
}