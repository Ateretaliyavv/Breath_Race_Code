using System.Collections.Generic;
using UnityEngine;

public class BreathSettingsManager : MonoBehaviour
{
    public static BreathSettingsManager Instance { get; private set; }

    [Header("Default Threshold Values (kPa)")]
    [SerializeField] private float defaultBlowBalloons = 1.0f;
    [SerializeField] private float defaultBuildBridge = 1.0f;
    [SerializeField] private float defaultPushBox = 1.0f;

    [Header("Surprise Defaults")]
    [SerializeField] private float defaultSurprise1 = 3.0f;
    [SerializeField] private float defaultSurprise2 = 2.0f;

    private readonly Dictionary<BreathActionKey, float> values = new Dictionary<BreathActionKey, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAll();
    }

    public float GetValue(BreathActionKey key)
    {
        if (values.TryGetValue(key, out float value))
            return value;

        float fallback = GetDefaultValue(key);
        values[key] = fallback;
        return fallback;
    }

    public void SetValue(BreathActionKey key, float value)
    {
        values[key] = value;
        PlayerPrefs.SetFloat(GetPlayerPrefsKey(key), value);
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        values[BreathActionKey.BlowBalloons] = defaultBlowBalloons;
        values[BreathActionKey.BuildBridge] = defaultBuildBridge;
        values[BreathActionKey.PushBox] = defaultPushBox;
        values[BreathActionKey.Surprise1] = defaultSurprise1;
        values[BreathActionKey.Surprise2] = defaultSurprise2;

        foreach (BreathActionKey key in values.Keys)
            PlayerPrefs.SetFloat(GetPlayerPrefsKey(key), values[key]);

        PlayerPrefs.Save();
    }

    private void LoadAll()
    {
        values[BreathActionKey.BlowBalloons] = PlayerPrefs.GetFloat(GetPlayerPrefsKey(BreathActionKey.BlowBalloons), defaultBlowBalloons);
        values[BreathActionKey.BuildBridge] = PlayerPrefs.GetFloat(GetPlayerPrefsKey(BreathActionKey.BuildBridge), defaultBuildBridge);
        values[BreathActionKey.PushBox] = PlayerPrefs.GetFloat(GetPlayerPrefsKey(BreathActionKey.PushBox), defaultPushBox);
        values[BreathActionKey.Surprise1] = PlayerPrefs.GetFloat(GetPlayerPrefsKey(BreathActionKey.Surprise1), defaultSurprise1);
        values[BreathActionKey.Surprise2] = PlayerPrefs.GetFloat(GetPlayerPrefsKey(BreathActionKey.Surprise2), defaultSurprise2);
    }

    private float GetDefaultValue(BreathActionKey key)
    {
        switch (key)
        {
            case BreathActionKey.BlowBalloons: return defaultBlowBalloons;
            case BreathActionKey.BuildBridge: return defaultBuildBridge;
            case BreathActionKey.PushBox: return defaultPushBox;
            case BreathActionKey.Surprise1: return defaultSurprise1;
            case BreathActionKey.Surprise2: return defaultSurprise2;
            default: return 1f;
        }
    }

    private string GetPlayerPrefsKey(BreathActionKey key)
    {
        return "BreathThreshold_" + key;
    }
}
