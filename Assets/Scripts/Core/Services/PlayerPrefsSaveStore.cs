using UnityEngine;

namespace Slot3x3
{
    /// <summary>The real save store: Unity's PlayerPrefs.</summary>
    public class PlayerPrefsSaveStore : ISaveStore
    {
        public int GetInt(string key, int fallback) { return PlayerPrefs.GetInt(key, fallback); }
        public void SetInt(string key, int value) { PlayerPrefs.SetInt(key, value); }
        public float GetFloat(string key, float fallback) { return PlayerPrefs.GetFloat(key, fallback); }
        public void SetFloat(string key, float value) { PlayerPrefs.SetFloat(key, value); }
        public string GetString(string key, string fallback) { return PlayerPrefs.GetString(key, fallback); }
        public void SetString(string key, string value) { PlayerPrefs.SetString(key, value); }
        public void Delete(string key) { PlayerPrefs.DeleteKey(key); }
    }
}
