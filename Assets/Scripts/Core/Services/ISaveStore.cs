namespace Slot3x3
{
    /// <summary>
    /// Somewhere to keep small values between sessions. Game code depends on this interface instead of
    /// PlayerPrefs, so tests can use <see cref="MemorySaveStore"/> and a cloud save could be added later.
    /// </summary>
    public interface ISaveStore
    {
        int GetInt(string key, int fallback);
        void SetInt(string key, int value);
        float GetFloat(string key, float fallback);
        void SetFloat(string key, float value);
        string GetString(string key, string fallback);
        void SetString(string key, string value);
        void Delete(string key);
    }
}
