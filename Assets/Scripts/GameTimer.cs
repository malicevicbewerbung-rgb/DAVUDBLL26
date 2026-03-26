using UnityEngine;
using TMPro;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Timer Settings")]
    public float startTime = 300f;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    IEnumerator Start()
    {
        // kurze Verzögerung, damit HUD sicher aufgebaut ist
        yield return null;

        if (timerText == null)
        {
            HUD hud = FindObjectOfType<HUD>();
            if (hud != null)
                timerText = hud.timerText;
        }

        currentTime = startTime;
        isRunning = true;
        UpdateUI();

        Debug.Log("GameTimer gestartet");
    }

    void Update()
    {
        if (!isRunning)
            return;

        currentTime -= Time.unscaledDeltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            UpdateUI();
            TriggerGameOver();
            return;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (timerText == null)
        {
            Debug.LogWarning("GameTimer: timerText ist NULL");
            return;
        }

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    void TriggerGameOver()
    {
        Debug.Log("Zeit abgelaufen - Game Over");

        Time.timeScale = 0f;

        if (LoseScreen.Instance != null)
            LoseScreen.Instance.Show();
    }

    public void AddTime(float amount)
    {
        currentTime += amount;
        UpdateUI();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = startTime;
        isRunning = true;
        UpdateUI();
    }
}