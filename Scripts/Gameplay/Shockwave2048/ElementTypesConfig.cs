using Gameplay.Shockwave2048.Elements;
using UnityEngine;

namespace Gameplay.Shockwave2048
{
    [CreateAssetMenu(menuName = "Configs/ElementTypesConfig", fileName = "ElementTypesConfig")]
    public class ElementTypesConfig : ScriptableObject
    {
        [SerializeField] private ElementTypeInfo[] elementTypeInfos;
        
        public ElementTypeInfo[] ElementTypeInfos => elementTypeInfos;
    }
}