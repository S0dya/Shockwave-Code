using System;
using PT.Tools.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Shockwave2048.Skills
{
    public enum SkillViewState
    {
        Use,
        Buy,
        RewardAd
    }
    
    public class SkillView : MonoBehaviour
    {
        [Serializable]
        private class WrappedArray
        {
            [SerializeField] private GameObject[] objects;
            
            public GameObject[] Objects => objects;
        }
        
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private Image blockedImage;
        [Space]
        [SerializeField] private SerializableKeyValue<SkillViewState, WrappedArray> stateObjects;
        [Space]
        [SerializeField] private Button useButton;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button adButton;
        [Space]
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private GameObject selectedFrame;

        private Action _use;
        private Action _buy;
        private Action _ad;

        private void Awake()
        {
            useButton.onClick.AddListener(() => _use?.Invoke());
            buyButton.onClick.AddListener(() => _buy?.Invoke());
            adButton.onClick.AddListener(() => _ad?.Invoke());
        }

        public void Init(Action use, Action buy, Action ad, int price)
        {
            _use = use;
            _buy = buy;
            _ad = ad;
            priceText.text = price.ToString();
        }
        
        public void Unlock()
        {
            blockedImage.SetActive(false);
            cg.interactable = true;
            cg.alpha = 1f;
        }
        public void Lock()
        {
            blockedImage.SetActive(true);
            cg.interactable = false;
            cg.alpha = 0.5f;
        }

        public void ActivateState(SkillViewState skillViewState)
        {
            foreach (var kvp in stateObjects.Dictionary) kvp.Value.Objects.SetActive(skillViewState == kvp.Key);
        }
        public void SetPrice(int price)
        {
            priceText.text = price.ToString();
        }
        public void SetAmount(int amount)
        {
            amountText.text = amount.ToString();
        }
        public void SetSelected(bool selected)
        {
            selectedFrame.SetActive(selected);
        }
    }
}