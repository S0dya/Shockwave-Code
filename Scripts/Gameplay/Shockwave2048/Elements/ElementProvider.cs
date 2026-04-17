using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Shockwave2048.Enums;
using PT.Logic.Configs;
using PT.Tools.Debugging;
using PT.Tools.Helper;
using UnityEngine;

namespace Gameplay.Shockwave2048.Elements
{
    public class ElementProvider 
    {
        private ElementTypeInfo[] _elementTypeInfos;
        private GameConfig _gameConfig;

        private readonly DirectionEnum[] _allDirections = new[] { DirectionEnum.Up, DirectionEnum.Down, DirectionEnum.Left, DirectionEnum.Right };

        public ElementProvider(ElementTypesConfig elementTypesConfig, GameConfig gameConfig)
        {
            _elementTypeInfos = elementTypesConfig.ElementTypeInfos;
            _gameConfig = gameConfig;
        }

        public ElementData GetData(ElementType type)
        {
            return new ElementData(_elementTypeInfos.First(x => x.ElementType == type), GetDirections());
        }

        public ElementData GetNext(ElementType type)
        {
            return TryBuild(type, null, null, true);
        }

        public ElementData GetNext(ElementData d1, ElementData d2)
        {
            return TryBuild(d1.ElementTypeInfo.ElementType, d1.PushDirections, d2.PushDirections, true);
        }

        public ElementData GetPrevious(ElementType type)
        {
            return TryBuild(type, null, null, false);
        }
        
        private ElementData TryBuild(ElementType type, DirectionEnum[] dirs1, DirectionEnum[] dirs2, bool forward)
        {
            if (!TryGetType(type, forward, out ElementType outType))
            {
                DebugManager.Log(DebugCategory.Errors, $"Invalid upgrade/downgrade for {type}", LogType.Error);
                return null;
            }

            var info = _elementTypeInfos.First(x => x.ElementType == outType);
            DirectionEnum[] dirs = dirs1 == null || dirs2 == null 
                ? GetDirections() 
                : GetDirections(dirs1, dirs2);

            return new ElementData(info, dirs);
        }

        private bool TryGetType(ElementType type, bool forward, out ElementType result)
        {
            int val = (int)type;
            int target = forward ? val * 2 : val / 2;

            result = ElementType.Two;

            if (!forward && val % 2 != 0) return false;
            if (!Enum.IsDefined(typeof(ElementType), target)) return false;

            result = (ElementType)target;
            return true;
        }

        public bool TryGetNextType(ElementType elementType, out ElementType nextType)
        {
            nextType = ElementType.Two;
            
            int current = (int)elementType;
            int upgraded = current * 2;

            bool can = Enum.IsDefined(typeof(ElementType), upgraded);
            if (can) nextType = (ElementType)upgraded;

            return can;
        }

        private DirectionEnum[] GetDirections(DirectionEnum[] dirs1, DirectionEnum[] dirs2)
        {
            if (_gameConfig.StrictPushDirections)
                return _allDirections;

            if (dirs1 == null || dirs1.Length == 0 ||
                dirs2 == null || dirs2.Length == 0)
                return GetDirections();

            var intersection = dirs1.Intersect(dirs2).ToArray();
            var result = _allDirections.Except(intersection).ToList();

            EnsureMinDirections(result);

            return result.ToArray();
        }

        private DirectionEnum[] GetDirections()
        {
            if (_gameConfig.StrictPushDirections)
                return _allDirections;

            var picked = _allDirections
                .Where(_ => Utils.GetRandomNext(1) > 0.5f)
                .ToList();

            EnsureMinDirections(picked);

            return picked.ToArray();
        }
        
        private void EnsureMinDirections(List<DirectionEnum> list)
        {
            int min = _gameConfig.MinAmountOfPushDirections;

            if (list.Count >= min) return;

            var missing = _allDirections.Except(list).ToList();

            missing = missing.OrderBy(_ => Utils.GetRandomNext(1)).ToList();

            while (list.Count < min && missing.Count > 0)
            {
                list.Add(missing[0]);
                missing.RemoveAt(0);
            }
        }
    }
}