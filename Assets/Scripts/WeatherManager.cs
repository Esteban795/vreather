using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using DigitalRuby.WeatherMaker;

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
    
    [Header("Weather Maker Profiles")]
    public WeatherMakerProfileScript clearProfile;
    public WeatherMakerProfileScript cloudProfile;
    public WeatherMakerProfileScript rainProfile;
    public WeatherMakerProfileScript snowProfile;
    public WeatherMakerProfileScript fogProfile;
    public WeatherMakerProfileScript thunderstormProfile;
    
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
        string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&units=metric&appid={apiKey}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); 
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Raw JSON response: " + webRequest.downloadHandler.text);
                OneCallData data = JsonUtility.FromJson<OneCallData>(webRequest.downloadHandler.text);
                weatherUI.UpdateDisplay(data,"Saguenay");
                ApplyWeatherToEnvironment(data.current);
            }
            else
            {
                Debug.LogError("Erreur API : " + webRequest.error);
            }
        }
    }
    
private void ApplyWeatherToEnvironment(CurrentData current)
    {
        // On s'assure qu'on a bien reçu une description météo
        if (current.weather == null || current.weather.Count == 0) return;

        // On récupère le mot clé et on le met en minuscules
        string condition = current.weather[0].main.ToLower();
        
        WeatherMakerProfileScript targetProfile = clearProfile; // Par défaut

        switch (condition)
        {
            case "clear": targetProfile = clearProfile; break;
            case "clouds": targetProfile = cloudProfile; break;
            case "drizzle":
            case "rain": targetProfile = rainProfile; break;
            case "snow": targetProfile = snowProfile; break;
            case "thunderstorm": targetProfile = thunderstormProfile; break;
            case "mist":
            case "smoke":
            case "haze":
            case "dust":
            case "fog": targetProfile = fogProfile; break;
        }

        if (WeatherMakerScript.Instance != null && targetProfile != null)
        {
            // On récupère le profil actuellement actif pour faire une belle transition
            WeatherMakerProfileScript currentProfile = WeatherMakerScript.Instance.LastLocalProfile;

            // On utilise la méthode officielle de cette version pour déclencher le changement de météo
            float transitionDuration = 3.0f; // La météo mettra 3 secondes à changer
            float holdDuration = 0.0f; // 0 = permanent jusqu'au prochain appel
            bool forceTransition = false;
            
            WeatherMakerScript.Instance.RaiseWeatherProfileChanged(
                currentProfile, 
                targetProfile, 
                transitionDuration, 
                holdDuration, 
                forceTransition, 
                null // connectionIds à null car c'est du solo
            );
            
            Debug.Log($"Météo appliquée dans Unity : {condition} -> Profil: {targetProfile.name}");
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
