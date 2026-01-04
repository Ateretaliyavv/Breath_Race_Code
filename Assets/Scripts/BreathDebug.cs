using TMPro;
using UnityEngine;

public class BreathDebug : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI t;

    void Update()
    {
        float p = PressureWebSocketReceiver.Instance ? PressureWebSocketReceiver.Instance.lastPressureKPa : -1f;
        bool usb = WebSerialPressureReceiver.Instance != null;
        t.text = $"USB:{usb}  P:{p:0.000}";
    }
}
