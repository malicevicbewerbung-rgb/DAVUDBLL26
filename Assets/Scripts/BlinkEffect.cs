using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkEffect : MonoBehaviour
{
    public Image blackScreen;

    [Header("Timing")]
    public float startBlackDuration = 2.5f;
    public float firstOpenDuration = 0.8f;
    public float blinkCloseDuration = 0.25f;
    public float blinkOpenDuration = 0.7f;
    public float pauseBetweenBlinks = 0.4f;
    public float finalFadeInDuration = 2.5f;

    void Start()
    {
        StartCoroutine(BlinkSequence());
    }

    IEnumerator BlinkSequence()
    {
        // Start: komplett schwarz
        SetAlpha(1f);
        yield return new WaitForSeconds(startBlackDuration);

        // Augen öffnen sich langsam ein wenig
        yield return StartCoroutine(FadeTo(0.35f, firstOpenDuration));

        // Wieder etwas schließen
        yield return StartCoroutine(FadeTo(0.85f, blinkCloseDuration));
        yield return new WaitForSeconds(0.15f);

        // Wieder etwas öffnen
        yield return StartCoroutine(FadeTo(0.25f, blinkOpenDuration));
        yield return new WaitForSeconds(pauseBetweenBlinks);

        // Noch ein langsames Blinzeln
        yield return StartCoroutine(FadeTo(0.75f, 0.3f));
        yield return new WaitForSeconds(0.2f);

        // Dann endgültig Szene langsam einblenden
        yield return StartCoroutine(FadeTo(0f, finalFadeInDuration));
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = blackScreen.color.a;
        float time = 0f;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            SetAlpha(alpha);
            time += Time.deltaTime;
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = blackScreen.color;
        c.a = alpha;
        blackScreen.color = c;
    }
}