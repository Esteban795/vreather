using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class OneCallData
{
    public float lat;
    public float lon;
    public string timezone;
    public int timezone_offset;
    public CurrentData current;
}

[Serializable]
public class CurrentData
{
    public long dt;
    public long sunrise;
    public long sunset;
    public float temp;
    public float feels_like;
    public int pressure;
    public int humidity;
    public float dew_point;
    public int clouds;
    public float uvi;
    public int visibility;
    public float wind_speed;
    public float wind_deg;
    public float wind_gust;
    public Rain rain;
    public Snow snow;
    public List<WeatherDescription> weather;
}

[Serializable]
public class WeatherDescription
{
    public int id;
    public string main;
    public string description;
    public string icon;
}

[Serializable]
public class Rain { public float _1h; }

[Serializable]
public class Snow { public float _1h; }
public class WeatherManager : MonoBehaviour
{
    private string apiKey;
    private double latitude = 48.42822;
    private double longitude = -71.0622;
    public WeatherUI weatherUI;
    
    void Start()
    {
        // On charge la clé au démarrage
        apiKey = ConfigLoader.GetApiKey();

        if (!string.IsNullOrEmpty(apiKey))
        {
            StartCoroutine(GetOneCallWeather(latitude, longitude));
        }
    }
    
    IEnumerator GetOneCallWeather(double latitude, double longitude)
    {
        string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&appid={apiKey}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); 
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Raw JSON response: " + webRequest.downloadHandler.text);
                OneCallData data = JsonUtility.FromJson<OneCallData>(webRequest.downloadHandler.text);
                weatherUI.UpdateDisplay(data,"Saguenay");
            }
            else
            {
                Debug.LogError("Erreur API : " + webRequest.error);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
