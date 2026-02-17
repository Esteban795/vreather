using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Button : MonoBehaviour
{
    [Header("Configuration")]
    public UnityEvent onClick;

    [Header("Animation")]
    public Vector3 pushOffset = new Vector3(0, -0.05f, 0); 
    public float speed = 10f;

    private Vector3 initialPosition;
    private bool isBusy = false;
    
    [Header("Audio")]
    public AudioClip clickSound;
    
    private AudioSource audioSource;
    void Start()
    {
        initialPosition = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
    }
    
    public void Interact()
    {
        if (isBusy) return; // Si déjà en train de bouger, on ignore
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        StartCoroutine(AnimateButton());
        Debug.Log("Bouton appuyé !");
        onClick.Invoke();
    }
    
    public IEnumerator AnimateButton()
    {
        isBusy = true;
        Vector3 targetPosition = initialPosition + pushOffset;
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);
            yield return null; // Attend la frame suivante
        }
        
        
        yield return new WaitForSeconds(0.1f);
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(targetPosition, initialPosition, t);
            yield return null;
        }
        transform.localPosition = initialPosition;
        isBusy = false;
    }

    public void StartAnimation()
    {
        StartCoroutine(AnimateButton());
    }
}