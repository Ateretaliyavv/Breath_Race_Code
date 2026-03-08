using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Supported languages in the game.
public enum Lang
{
    EN = 0,
    HE = 1
}

// Manages loading translations and switching the current language.
public class LocalizationManager : MonoBehaviour
{
    // Singleton instance for global access.
    public static LocalizationManager I { get; private set; }

    // The currently active language.
    public Lang CurrentLang { get; private set; } = Lang.EN;

    // Fired whenever the language is changed.
    public event Action OnLanguageChanged;

    // Stores translations for the current language.
    private Dictionary<string, string> dict = new Dictionary<string, string>();

    private const string PrefKey = "lang";
    private const string CsvResourcePath = "Localization/translations"; // without .csv

    // Initializes the singleton and loads the default language dictionary.
    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }

        I = this;
        DontDestroyOnLoad(gameObject);

        // Always start in English on a fresh game launch.
        CurrentLang = Lang.EN;
        LoadDictionary();
    }

    // Changes the current language, reloads translations, and notifies listeners.
    public void SetLanguage(Lang lang)
    {
        if (CurrentLang == lang) return;

        CurrentLang = lang;
        PlayerPrefs.SetInt(PrefKey, (int)lang);
        PlayerPrefs.Save();

        LoadDictionary();
        OnLanguageChanged?.Invoke();
    }

    // Returns the translated value for the given key.
    public string Tr(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (dict.TryGetValue(key, out var v)) return v;

        // If a translation is missing, show the key with a prefix.
        return $"#{key}";
    }

    // Loads the CSV file from Resources and builds the current dictionary.
    private void LoadDictionary()
    {
        TextAsset csv = Resources.Load<TextAsset>(CsvResourcePath);
        if (csv == null)
        {
            Debug.LogError($"Localization CSV not found at Resources/{CsvResourcePath}.csv");
            dict = new Dictionary<string, string>();
            return;
        }

        dict = ParseCsvToDictionary(csv.text, CurrentLang);
    }

    // Parses the CSV text into a dictionary according to the selected language.
    // Expected format: KEY,EN,HE
    private static Dictionary<string, string> ParseCsvToDictionary(string csvText, Lang lang)
    {
        var result = new Dictionary<string, string>();
        var rows = ReadCsvRows(csvText);
        if (rows.Count == 0) return result;

        // Column 0 = KEY, column 1 = EN, column 2 = HE
        int valueCol = (lang == Lang.EN) ? 1 : 2;

        for (int i = 1; i < rows.Count; i++)
        {
            var cols = rows[i];
            if (cols.Count < 3) continue;

            string key = cols[0].Trim();
            if (string.IsNullOrEmpty(key)) continue;

            string value = cols[valueCol];

            // Convert escaped line breaks from the CSV into real line breaks.
            value = value.Replace("\\n", "\n");

            result[key] = value;
        }

        return result;
    }

    // Reads CSV rows safely, including fields with commas or quotes.
    private static List<List<string>> ReadCsvRows(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();

        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"')
            {
                // Handle escaped quotes inside a quoted field.
                if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                row.Add(field.ToString());
                field.Length = 0;
            }
            else if ((c == '\n') && !inQuotes)
            {
                row.Add(field.ToString());
                field.Length = 0;

                // Remove '\r' for Windows line endings.
                for (int k = 0; k < row.Count; k++)
                    row[k] = row[k].Replace("\r", "");

                // Do not add completely empty rows.
                if (!(row.Count == 1 && string.IsNullOrEmpty(row[0])))
                    rows.Add(row);

                row = new List<string>();
            }
            else
            {
                field.Append(c);
            }
        }

        // Add the last row if the file does not end with '\n'.
        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());

            for (int k = 0; k < row.Count; k++)
                row[k] = row[k].Replace("\r", "");

            rows.Add(row);
        }

        return rows;
    }
}
