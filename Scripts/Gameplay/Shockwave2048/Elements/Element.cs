using System.Linq;
using Gameplay.Shockwave2048.Enums;
using PT.Tools.Helper;
using UnityEngine;

namespace Gameplay.Shockwave2048.Elements
{
    public class Element : MonoBehaviour
    {
        [SerializeField] private MergeElementView view;
        
        private ElementData _elementData;
        
        public void SetData(ElementData elementData)
        {
            view.Set(elementData);
            
            _elementData = elementData;

            DeactivateBonus();
        }
        
        public void PlayMergeEffect()
        {
            view.PlayMerge();
        }
        public void StopMergeEffect()
        {
            view.StopPlayingMerge();
        }
        
        public void ActivateBonus()
        {
            _elementData.AddBonus();
            
            view.ActivateBonus();
        }
        public void DeactivateBonus()
        {
            _elementData.RemoveBonus();
            
            view.DeactivateBonus();
        }
        
        public void Select()
        {
            view.Select();
        }
        public void Deselect()
        {
            view.Deselect();
        }
        
        public ElementData GetData() => _elementData;
        public DirectionEnum[] GetActivePushDirections() => _elementData.PushDirections.ToArray();
        public ElementType GetElementType() => _elementData.ElementTypeInfo.ElementType;
        public bool GetBonus() => _elementData.HasBonus;
    }
}