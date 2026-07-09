using System.Collections.Generic;

namespace DeepForest.Narrative
{
    public class CharacterStoryFlags
    {
        private readonly Dictionary<string, bool> _boolFlags = new();
        private readonly Dictionary<string, int> _intFlags = new();

        public bool GetBool(string key, bool defaultValue = false)
        {
            return _boolFlags.TryGetValue(key, out var val) ? val : defaultValue;
        }

        public void SetBool(string key, bool value = true)
        {
            _boolFlags[key] = value;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return _intFlags.TryGetValue(key, out var val) ? val : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            _intFlags[key] = value;
        }

        public void Reset()
        {
            _boolFlags.Clear();
            _intFlags.Clear();
        }
    }
}
