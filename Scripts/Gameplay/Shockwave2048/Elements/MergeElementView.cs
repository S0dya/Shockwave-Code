using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Shockwave2048.Elements
{
    [Serializable]
    public class MergeElementView : ElementView
    {
        [SerializeField] private Image mergeImage;
        [Space]
        [SerializeField] private Image bonusImage;
        [Space]
        [SerializeField] private Image selectedImage;
        [Header("Appear")]
        [SerializeField] private float appearScale = 1.15f;
        [SerializeField] private float appearTime = 0.12f;
        [Header("Merge Pulse")]
        [SerializeField] private float mergePulseScale = 1.12f;
        [SerializeField] private float mergePulseTime = 0.35f;
        [SerializeField] private Ease mergePulseEase = Ease.InOutSine;
        [Header("Bonus Anim")]
        [SerializeField] private float bonusPopupScale = 1.2f;
        [SerializeField] private float bonusLiftY = 12f;
        [SerializeField] private float bonusPopupTime = 0.1f;
        [SerializeField] private float bonusHoldTime = 0.25f;
        [SerializeField] private float bonusReturnTime = 0.12f;
        
        private Vector3 _bonusStartPos;
        private Vector3 _bonusStartScale;

        private bool _isAppearing;
        
        private Tween _appearTween;
        private Sequence _bonusTween;
        private Sequence _mergePulseTween;

        private void Start()
        {
            _bonusStartPos = bonusImage.transform.position;
            _bonusStartScale = bonusImage.transform.localScale;
        }
        
        public override void Set(ElementData elementData, bool ignoreDirections = false)
        {
            base.Set(elementData, ignoreDirections);
         
            mergeImage.enabled = false;
            mainImage.enabled = true;
            Deselect();
            
            KillBonusAnim();
            bonusImage.enabled = false;

            PlayAppear();
        }
        
        public void PlayMerge()
        {
            if (mergeImage.enabled) return;

            mergeImage.enabled = true;
            mainImage.enabled = false;
            PlayMergePulse();
        }

        public void StopPlayingMerge()
        {
            KillMergePulse();

            mergeImage.enabled = false;
            mainImage.enabled = true;
            mergeImage.transform.localScale = Vector3.one;

            if (!_isAppearing)
                PlayAppear();
        }
        
         public void ActivateBonus()
        {
            bonusImage.enabled = true;

            KillBonusAnim();

            var t = bonusImage.rectTransform;
            _bonusStartPos = t.anchoredPosition;
            _bonusStartScale = t.localScale;

            t.localScale = Vector3.zero;

            _bonusTween = DOTween.Sequence();
            _bonusTween.SetTarget(this);

            _bonusTween.Append(
                t.DOScale(bonusPopupScale, bonusPopupTime)
                    .SetEase(Ease.OutBack)
            );

            _bonusTween.Join(
                t.DOAnchorPosY(_bonusStartPos.y + bonusLiftY, bonusPopupTime)
                    .SetEase(Ease.OutQuad)
            );

            _bonusTween.AppendInterval(bonusHoldTime);

            _bonusTween.Append(
                t.DOScale(_bonusStartScale, bonusReturnTime)
                    .SetEase(Ease.OutQuad)
            );

            _bonusTween.Join(
                t.DOAnchorPos(_bonusStartPos, bonusReturnTime)
                    .SetEase(Ease.OutQuad)
            );
        }
         
        private void PlayMergePulse()
        {
            KillMergePulse();

            var t = mergeImage.transform;
            t.localScale = Vector3.one;

            _mergePulseTween = DOTween.Sequence()
                .Append(t.DOScale(mergePulseScale, mergePulseTime).SetEase(mergePulseEase))
                .Append(t.DOScale(1f, mergePulseTime).SetEase(mergePulseEase))
                .SetLoops(-1)
                .SetTarget(this);
        }

        private void KillMergePulse()
        {
            _mergePulseTween?.Kill();
            _mergePulseTween = null;
        }

        public void DeactivateBonus()
        {
            KillBonusAnim();
            bonusImage.enabled = false;
        }

        public void Select()
        {
            selectedImage.enabled = true;
        }

        public void Deselect()
        {
            selectedImage.enabled = false;
        }

        private void PlayAppear()
        {
            _appearTween?.Kill();
            _isAppearing = true;

            var t = transform;
            t.localScale = Vector3.zero;

            _appearTween = t.DOScale(appearScale, appearTime)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    t.DOScale(1f, appearTime * 0.6f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => _isAppearing = false)
                        .SetTarget(this);
                })
                .SetTarget(this);
        }

        private void KillBonusAnim()
        {
            if (_bonusTween != null)
            {
                _bonusTween.Kill();
                _bonusTween = null;
            }

            if (bonusImage != null)
            {
                var t = bonusImage.rectTransform;
                t.localScale = Vector3.one;
            }
        }

        private void OnDisable()
        {
            _appearTween?.Kill();
            KillBonusAnim();
            _isAppearing = false;
        }
    }
}