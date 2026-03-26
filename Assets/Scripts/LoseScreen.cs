using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoseScreen : MonoBehaviour
{
    public static LoseScreen Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public Button restartButton;
    public Button quitButton;
    public TextMeshProUGUI loseText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Hide();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}