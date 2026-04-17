using DG.Tweening;
using PT.Tools.Helper;
using UnityEngine;

namespace Gameplay.Shockwave2048.MegaMerge
{
    public class SwapPointerHint : MonoBehaviour
    {
        [SerializeField] private RectTransform pointer;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private SerializableKeyValue<DirectionEnum, float> directionsDistances;
        [Space]
        [SerializeField] private float moveTime = 0.35f;
        [SerializeField] private float delayBetween = 0.15f;

        private Vector2 _startPos;
        private Sequence _seq;

        private void Awake()
        {
            _startPos = pointer.anchoredPosition;
        }
        
        private void OnEnable()
        {
            ResetState();
            Play();
        }

        private void OnDisable()
        {
            Kill();
        }

        private void ResetState()
        {
            Kill();
            pointer.anchoredPosition = _startPos;
            canvasGroup.alpha = 1f;
        }

        private void Play()
        {
            _seq = DOTween.Sequence();
            _seq.SetTarget(this);

            foreach (var kvp in directionsDistances.Dictionary)
            {
                Vector2 offset = Utils.GetDirection(kvp.Key) * kvp.Value;

                _seq.Append(pointer.DOAnchorPos(_startPos + offset, moveTime).SetEase(Ease.OutCubic));
                _seq.Join(canvasGroup.DOFade(0f, moveTime));
                _seq.AppendInterval(delayBetween);
                _seq.AppendCallback(() =>
                {
                    pointer.anchoredPosition = _startPos;
                    canvasGroup.alpha = 1f;
                });
            }

            _seq.SetLoops(-1);
        }
        
        private void Kill()
        {
            if (_seq != null && _seq.IsActive()) _seq.Kill(true);
        }
    }
}