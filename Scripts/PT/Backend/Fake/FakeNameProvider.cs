using System.Collections.Generic;
using UnityEngine;

namespace PT.Backend.Fake
{
    public class FakeNicknameProvider
    {
        private readonly List<string> _names;

        public FakeNicknameProvider(TextAsset namesFile)
        {
            _names = new List<string>();

            var lines = namesFile.text.Split('\n');
            foreach (var l in lines)
            {
                var name = l.Trim();
                if (!string.IsNullOrEmpty(name))
                    _names.Add(name);
            }
        }

        public string Get(int index)
        {
            if (_names.Count == 0) return "Player";
            return _names[index % _names.Count];
        }

        public int Count => _names.Count;
    }
}