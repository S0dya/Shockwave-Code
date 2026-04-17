using System;
using DG.Tweening;
using PT.Tools.Helper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.Shockwave2048.Slot
{
    public class GridSlotView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject[] inactiveObjs;
        [SerializeField] private CanvasGroup inactiveCanvasGroup;
        [Space]
        [SerializeField] private float animTime = 0.16f;
        [SerializeField] private Ease animEase = Ease.OutBack;

        private Tween _tween;
        private Action _onDown;
        private Action _onUp;

        public void Init(Action onPointerDown, Action onPointerUp)
        {
            _onDown = onPointerDown;
            _onUp = onPointerUp;
        }
        public void OnPointerDown(PointerEventData eventData) => _onDown?.Invoke();
        public void OnPointerUp(PointerEventData eventData) => _onUp?.Invoke();

        public void Activate(bool animate)
        {
            KillTween();

            if (!animate)
            {
                SetVisual(false); return;
            }

            SetVisual(true);
            SetState(1f);

            _tween = AnimateTo(0f, () => SetVisual(false));
        }

        public void Deactivate(bool animate)
        {
            KillTween();

            SetVisual(true);

            if (!animate)
            {
                SetState(1f); return;
            }

            SetState(0f);
            _tween = AnimateTo(1f);
        }

        private Tween AnimateTo(float target, Action onComplete = null)
        {
            var seq = DOTween.Sequence().SetTarget(this);

            for (int i = 0; i < inactiveObjs.Length; i++)
            {
                var t = inactiveObjs[i].transform;

                seq.Join(t.DOScale(target, animTime).SetEase(animEase));
                seq.Join(inactiveCanvasGroup.DOFade(target, animTime));
            }

            if (onComplete != null) seq.OnComplete(() => onComplete());

            return seq;
        }

        private void SetState(float value)
        {
            for (int i = 0; i < inactiveObjs.Length; i++)
            {
                inactiveObjs[i].transform.localScale = Vector3.one * value;
                inactiveCanvasGroup.alpha = value;
            }
        }

        private void SetVisual(bool active)
        {
            foreach (var obj in inactiveObjs) if (obj) obj.SetActive(active);
        }

        private void KillTween()
        {
            _tween?.Kill();
            _tween = null;
        }

        private void OnDisable()
        {
            KillTween();
        }
    }
}