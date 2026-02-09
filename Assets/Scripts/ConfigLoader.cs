using UnityEngine;
using System.IO;

[System.Serializable]
public class AppConfig
{
    public string openWeatherKey;
}

public class ConfigLoader : MonoBehaviour
{
    public static string GetApiKey()
    {
        // Chemin vers le fichier dans StreamingAssets
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            AppConfig config = JsonUtility.FromJson<AppConfig>(jsonContent);
            return config.openWeatherKey;
        }
        else
        {
            Debug.LogError("Fichier config.json introuvable ! Avez-vous créé le fichier dans StreamingAssets ?");
            return "";
        }
    }
}