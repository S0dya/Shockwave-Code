using System;
using Gameplay.Shockwave2048.Enums;
using UnityEngine;

namespace Gameplay.Shockwave2048.Elements
{
    [Serializable]
    public class ElementTypeInfo
    {
        [SerializeField] private ElementType elementType;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Sprite showcaseSprite;
        
        public ElementType ElementType => elementType;
        public Sprite Sprite => sprite;
        public Sprite ShowcaseSprite => showcaseSprite;
    }
}