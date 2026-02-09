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

    private bool isVisible = false;

    public void Start()
    {
        gameObject.SetActive(false);
    }
    public void UpdateDisplay(OneCallData data, string cityName)
    {
        cityText.text = cityName.ToUpper();
        tempText.text = $"{Mathf.RoundToInt(data.current.temp)}°C";
        descText.text = data.current.weather[0].description;
        humidityText.text = $"Humidité: {data.current.humidity}%";
        FeelsLikeText.text = $"Ressenti : {Mathf.RoundToInt(data.current.feels_like)}°C";
    }

    public void toggleVisibility()
    {
        isVisible = !isVisible;
        gameObject.SetActive(isVisible);
    }
}
