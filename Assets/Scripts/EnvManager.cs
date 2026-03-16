using UnityEngine;

public class EnvManager : MonoBehaviour
{
    [Header("Environments")]
    public GameObject[] environments;
    private int currentIndex = 0;

    void Start()
    {
        if (environments.Length == 0) return;
        for (int i = 0; i < environments.Length; i++)
        {
            environments[i].SetActive(i == currentIndex); 
        }
    }
    
    public void CycleNextEnvironment()
    {
        if (environments.Length == 0) return;
        environments[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % environments.Length;
        environments[currentIndex].SetActive(true);
        Debug.Log("Nouvel environnement activé : " + environments[currentIndex].name);
    }
}