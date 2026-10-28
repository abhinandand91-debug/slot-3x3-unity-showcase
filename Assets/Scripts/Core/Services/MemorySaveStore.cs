using System.Collections.Generic;

namespace Slot3x3
{
    /// <summary>A save store that only lives in memory. Used by tests and when saving is switched off.</summary>
    public class MemorySaveStore : ISaveStore
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public int GetInt(string key, int fallback) { return Get(key, fallback); }
        public void SetInt(string key, int value) { values[key] = value; }
        public float GetFloat(string key, float fallback) { return Get(key, fallback); }
        public void SetFloat(string key, float value) { values[key] = value; }
        public string GetString(string key, string fallback) { return Get(key, fallback); }
        public void SetString(string key, string value) { values[key] = value; }
        public void Delete(string key) { values.Remove(key); }

        private T Get<T>(string key, T fallback)
        {
            return values.TryGetValue(key, out object value) && value is T typed ? typed : fallback;
        }
    }
}
