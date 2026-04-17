using System.Linq;
using PT.Tools.Helper;

namespace Gameplay.Shockwave2048.Elements
{
    public class ElementData 
    {
        public ElementTypeInfo ElementTypeInfo { get; private set; }
        public DirectionEnum[] PushDirections { get; private set; }
        public bool HasBonus { get; private set; }
        
        public ElementData(ElementTypeInfo elementTypeInfo, DirectionEnum[] pushDirections)
        {
            ElementTypeInfo = elementTypeInfo;
            PushDirections = pushDirections;
        }
        
        public void AddBonus()
        {
            HasBonus = true;
        }
        public void RemoveBonus()
        {
            HasBonus = false;
        }
        
        public ElementData Clone()
        {
            return new ElementData(ElementTypeInfo, PushDirections.ToArray());
        }
    }
}