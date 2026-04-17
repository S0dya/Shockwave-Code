using System;
using System.Linq;
using PT.Tools.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Shockwave2048.Elements
{
    public class ElementView : MonoBehaviour
    {
        [SerializeField] protected Image mainImage;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private SerializableKeyValue<DirectionEnum, Image> pushDirectionImages;
        
        public virtual void Set(ElementData elementData, bool hasDirections = false)
        {
            if (elementData == null) return;
            
            mainImage.sprite = elementData.ElementTypeInfo.Sprite;
            levelText.text = ((int)elementData.ElementTypeInfo.ElementType).ToString();

            if (hasDirections)
            {
                foreach (var pushDirection in (DirectionEnum[])Enum.GetValues(typeof(DirectionEnum)))
                {
                    pushDirectionImages.Dictionary[pushDirection].SetActive(elementData.PushDirections.Contains(pushDirection));
                }   
            }
        } 
    }
}