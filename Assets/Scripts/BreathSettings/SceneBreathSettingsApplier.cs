using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBreathSettingsApplier : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplySettingsInCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySettingsInCurrentScene();
    }

    public void ApplySettingsInCurrentScene()
    {
        if (BreathSettingsManager.Instance == null)
            return;

        BlowUpBalloons[] balloons = FindObjectsByType<BlowUpBalloons>(FindObjectsSortMode.None);
        foreach (BlowUpBalloons item in balloons)
        {
            item.SetBreathThreshold(BreathSettingsManager.Instance.GetValue(BreathActionKey.BlowBalloons));
        }

        BridgeBuilder[] bridges = FindObjectsByType<BridgeBuilder>(FindObjectsSortMode.None);
        foreach (BridgeBuilder item in bridges)
        {
            item.SetBreathThreshold(BreathSettingsManager.Instance.GetValue(BreathActionKey.BuildBridge));
        }

        PushBox[] pushBoxes = FindObjectsByType<PushBox>(FindObjectsSortMode.None);
        foreach (PushBox item in pushBoxes)
        {
            item.SetBreathThreshold(BreathSettingsManager.Instance.GetValue(BreathActionKey.PushBox));
        }

        InflatingBalloon[] inflatingBalloons = FindObjectsByType<InflatingBalloon>(FindObjectsSortMode.None);
        foreach (InflatingBalloon item in inflatingBalloons)
        {
            item.SetBreathThreshold(
                BreathSettingsManager.Instance.GetValue(BreathActionKey.Surprise1)
            );
        }

        SimpleBlow[] simpleBlows = FindObjectsByType<SimpleBlow>(FindObjectsSortMode.None);
        foreach (SimpleBlow item in simpleBlows)
        {
            item.SetBreathThreshold(
                BreathSettingsManager.Instance.GetValue(BreathActionKey.Surprise2)
            );
        }
    }
}
