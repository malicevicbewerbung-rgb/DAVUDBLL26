using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Game is exiting...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
