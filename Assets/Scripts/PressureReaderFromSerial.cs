using System;
using System.IO.Ports;
using System.Globalization;
using UnityEngine;
using TMPro;

public class PressureReaderFromSerial : MonoBehaviour
{
    [Header("Serial Port Settings")]
    [Tooltip("COM port name, למשל COM3, COM4...")]
    public string portName = "COM3";

    [Tooltip("Must match Serial.begin(...) in Arduino code")]
    public int baudRate = 115200;

    [Header("Debug / UI (optional)")]
    public TextMeshProUGUI debugText;   // אפשר להשאיר ריק אם לא צריך

    [Header("Latest Value")]
    public float lastPressureKPa = 0f;  // כאן נגיש לשאר הסקריפטים

    private SerialPort serialPort;

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 50;     // מילישניות
            serialPort.Open();
            Debug.Log("Serial port opened: " + portName);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    void Update()
    {
        if (serialPort == null || !serialPort.IsOpen)
            return;

        try
        {
            // ReadLine קורא עד '\n' – בדיוק מה ש-Serial.println שולח
            string line = serialPort.ReadLine();
            line = line.Trim();

            if (string.IsNullOrEmpty(line))
                return;

            // parse float עם נקודה עשרונית
            if (float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                lastPressureKPa = value;

                if (debugText != null)
                {
                    debugText.text = "Pressure: " + lastPressureKPa.ToString("0.000") + " kPa";
                }
            }
            else
            {
                Debug.LogWarning("Failed to parse pressure value: " + line);
            }
        }
        catch (TimeoutException)
        {
            // אין כרגע שורה לקרוא – מתעלמים
        }
        catch (Exception e)
        {
            Debug.LogError("Serial read error: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
    }
}
