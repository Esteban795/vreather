using UnityEngine;
using TMPro; // Obligatoire pour TextMeshPro
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class WeatherUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI cityText;
    public TextMeshProUGUI tempText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI humidityText;
    public TextMeshProUGUI FeelsLikeText;
    public Image weatherIcon;
    
    public void UpdateDisplay(OneCallData data, string cityName)
    {
        cityText.text = cityName.ToUpper();
        tempText.text = $"{Mathf.RoundToInt(data.current.temp)}°C";
        descText.text = data.current.weather[0].description;
        humidityText.text = $"Humidité: {data.current.humidity}%";
        FeelsLikeText.text = $"Ressenti : {data.current.feels_like}%";

        // Lancer le téléchargement de l'icône
        StartCoroutine(DownloadIcon(data.current.weather[0].icon));
    }

    IEnumerator DownloadIcon(string iconCode)
    {
        string url = $"https://openweathermap.org/img/wn/{iconCode}@2x.png";
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                weatherIcon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
