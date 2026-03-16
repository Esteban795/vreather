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
    private int currentWeatherId = 800;
    public WeatherUI weatherUI;
    
    [Header("Precise Weather Profiles")]
    public WeatherMakerProfileScript clear;
    public WeatherMakerProfileScript clearWindy;
    public WeatherMakerProfileScript lightClouds;
    public WeatherMakerProfileScript mediumClouds;
    public WeatherMakerProfileScript overcastClouds;
    public WeatherMakerProfileScript lightRain;
    public WeatherMakerProfileScript mediumRain;
    public WeatherMakerProfileScript heavyRain;
    public WeatherMakerProfileScript storm;
    public WeatherMakerProfileScript lightSnow;
    public WeatherMakerProfileScript mediumSnow;
    public WeatherMakerProfileScript heavySnow;
    public WeatherMakerProfileScript blizzard;
    public WeatherMakerProfileScript lightSleet;
    public WeatherMakerProfileScript mediumSleet;
    public WeatherMakerProfileScript heavySleet;
    public WeatherMakerProfileScript mediumFog;
    public WeatherMakerProfileScript sandstorm;
    public WeatherMakerProfileScript smoky;
    
    [Header("Materials")]
    public Material staticMaterial;
    public Material treeMaterial;
    
    [Header("Saisons (Textures)")]
    public Texture summerPalette;
    public Texture winterPalette;
    
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
                SynchronizeEnvironment(data);
            }
            else
            {
                Debug.LogError("Erreur API : " + webRequest.error);
            }
        }
    }
    
private void ApplyWeatherToEnvironment(CurrentData current)
    {
        if (current.weather == null || current.weather.Count == 0) return;

        currentWeatherId = current.weather[0].id;
        int weatherId = currentWeatherId;
        // Debug
        //int weatherId = 711;
        //currentWeatherId = weatherId;
        WeatherMakerProfileScript targetProfile = clear; 

        // 200s: Thunderstorms
        if (weatherId >= 200 && weatherId < 300) targetProfile = storm;
        
        // 300s: Drizzle
        else if (weatherId >= 300 && weatherId < 400) targetProfile = lightRain;
        
        // 500s: Rain
        else if (weatherId == 500 || weatherId == 501) targetProfile = mediumRain;
        else if (weatherId == 511 || weatherId == 611 || weatherId == 615) targetProfile = lightSleet;
        else if (weatherId >= 502 && weatherId < 600) targetProfile = heavyRain;
        else if (weatherId == 612 || weatherId == 616) targetProfile = mediumSleet;
        else if (weatherId == 613) targetProfile = heavySleet;
        
        // 600s: Snow
        else if (weatherId == 600 || weatherId == 620) targetProfile = lightSnow;
        else if (weatherId == 601 || weatherId == 621) targetProfile = mediumSnow;
        else if (weatherId == 602 || weatherId == 622) targetProfile = blizzard;
        
        
        // 700s: Atmosphere (Fog, Smoke, Sand)
        else if (weatherId == 711) targetProfile = smoky;
        else if (weatherId == 731 || weatherId == 751 || weatherId == 761) targetProfile = sandstorm;
        else if (weatherId >= 700 && weatherId < 800) targetProfile = mediumFog;
        
        // 800: Clear (Using your wind data!)
        else if (weatherId == 800) 
        {
            // If wind is stronger than 10 meters per second (approx 36 km/h)
            if (current.wind_speed > 10.0f) targetProfile = clearWindy;
            else targetProfile = clear;
        }
        
        // 801-804: Clouds
        else if (weatherId == 801) targetProfile = lightClouds;
        else if (weatherId == 802 || weatherId == 803) targetProfile = mediumClouds;
        else if (weatherId == 804) targetProfile = overcastClouds;

        if (WeatherMakerScript.Instance != null && targetProfile != null)
        {
            WeatherMakerProfileScript currentProfile = WeatherMakerScript.Instance.LastLocalProfile;
            WeatherMakerScript.Instance.LastLocalProfile = targetProfile;
            WeatherMakerScript.Instance.RaiseWeatherProfileChanged(currentProfile, targetProfile, 3.0f, 0.0f, false, null);
            
            Debug.Log($"Weather applied (ID {weatherId}, Wind: {current.wind_speed}m/s) -> Profile: {targetProfile.name}");
        }
        RefreshTextures();
    }

private void SynchronizeEnvironment(OneCallData data)
    {
        CurrentData current = data.current;

        // Day - night cycle
        long localTimeUnix = current.dt + data.timezone_offset;
        System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(localTimeUnix);

        if (DigitalRuby.WeatherMaker.WeatherMakerDayNightCycleManagerScript.Instance != null)
        {
            DigitalRuby.WeatherMaker.WeatherMakerDayNightCycleManagerScript.Instance.TimeOfDay = (float)dateTime.TimeOfDay.TotalSeconds;
            Debug.Log($"Heure synchronisée avec Saguenay : {dateTime.ToString("HH:mm:ss")}");
        }
        
        if (DigitalRuby.WeatherMaker.WeatherMakerWindScript.Instance != null)
        {
            // L'API renvoie le vent en mètres par seconde (m/s).
            // 0 m/s = calme, 10 m/s = vent fort (36 km/h), 30 m/s = ouragan (108 km/h).
            float windIntensity = Mathf.Clamp01(current.wind_speed / 30.0f);
            DigitalRuby.WeatherMaker.WeatherMakerWindScript.Instance.ExternalIntensityMultiplier = windIntensity;
            DigitalRuby.WeatherMaker.WeatherMakerWindScript.Instance.WindZone.transform.rotation = Quaternion.Euler(0, current.wind_deg, 0);
            if (treeMaterial != null)
            {
                float maxAmplitude = 10.0f;
                float maxFrequency = 2.0f; 
                
                float currentAmplitude = Mathf.Max(0.1f, maxAmplitude * windIntensity);
                float currentFrequency = Mathf.Max(0.1f, maxFrequency * windIntensity);
                treeMaterial.SetFloat("_MBAmplitude", currentAmplitude);
                treeMaterial.SetFloat("_MBFrequency", currentFrequency);
                treeMaterial.SetFloat("_MBWindDir", current.wind_deg);
            
                Debug.Log($"Arbres mis à jour : Force = {currentAmplitude}, Direction = {current.wind_deg}°");
            }

            Debug.Log($"Vent synchronisé : {current.wind_speed} m/s (Multiplicateur : {windIntensity})");
        }
    }

    public void RefreshTextures()
    {
        if (treeMaterial != null && staticMaterial != null && summerPalette != null && winterPalette != null)
        {
            // Ice (511 + 61x)
            if (currentWeatherId == 511 || (currentWeatherId >= 611 && currentWeatherId <= 616))
            {
                treeMaterial.SetTexture("_MainTex", winterPalette);
                staticMaterial.SetTexture("_BaseMap", winterPalette);
                treeMaterial.SetFloat("_Glossiness", 0.8f);
                staticMaterial.SetFloat("_Smoothness", 0.8f);
            }
            // Snow (6XX)
            else if (currentWeatherId >= 600 && currentWeatherId < 700) 
            {
                treeMaterial.SetTexture("_MainTex", winterPalette);
                staticMaterial.SetTexture("_BaseMap", winterPalette);
                treeMaterial.SetFloat("_Glossiness", 0.0f);
                staticMaterial.SetFloat("_Smoothness", 0.0f);
            }
            // Rain (200 to 599)
            else if (currentWeatherId >= 200 && currentWeatherId < 600)
            {
                treeMaterial.SetTexture("_MainTex", summerPalette);
                staticMaterial.SetTexture("_BaseMap", summerPalette);
                treeMaterial.SetFloat("_Glossiness", 0.8f);
                staticMaterial.SetFloat("_Smoothness", 0.8f);
            }
            // Dry
            else
            {
                treeMaterial.SetTexture("_MainTex", summerPalette);
                staticMaterial.SetTexture("_BaseMap", summerPalette);
                treeMaterial.SetFloat("_Glossiness", 0.0f);
                staticMaterial.SetFloat("_Smoothness", 0.0f);
            }
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
