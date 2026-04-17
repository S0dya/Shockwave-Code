using UnityEngine;

namespace Gameplay.Shockwave2048.MegaMerge
{
    public class MegaMergeSwipeBlocker : MonoBehaviour
    {
        [SerializeField] private RectTransform forbiddenArea;
        [SerializeField] private Canvas canvas;

        public bool IsPointBlocked(Vector2 screenPos)
        {
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            return RectTransformUtility.RectangleContainsScreenPoint(forbiddenArea, screenPos, cam);
        }
    }
}