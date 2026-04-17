using System;
using UnityEngine;

namespace Gameplay.Shockwave2048.Slot
{
    public class GridSlot : MonoBehaviour
    {
        [SerializeField] private GridSlotView view;
        
        private GridSlotData _data;

        public void Init(Action onPointerClick, Action onPointerUp)
        {
            _data = new();

            if (!view) view = GetComponent<GridSlotView>();
            view.Init(onPointerClick, onPointerUp);
        }

        public void ToggleActivation(bool toggle, bool animate = true)
        {
            _data.IsActive = toggle;

            if (toggle) view.Activate(animate); 
            else view.Deactivate(animate);
        }
        
        public bool GetActive() => _data.IsActive;
        public Vector2 GetPosition() => transform.localPosition;
    }
}