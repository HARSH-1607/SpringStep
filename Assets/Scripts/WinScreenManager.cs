using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class WinScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI noteText;
    public string mainMenuSceneName = "Menu"; // Make sure your menu scene is named this

    private void Start()
    {
        // Display the final score from the GameManager
        if (GameManager.Instance != null)
        {
            if (scoreText != null)
            {
                scoreText.text = "Final Score: " + GameManager.Instance.TotalScore;
            }
        }
        else
        {
            Debug.LogWarning("GameManager not found! Score might be 0.");
            if (scoreText != null) scoreText.text = "Final Score: 0";
        }

        // Set the note text
        if (noteText != null)
        {
            noteText.text = "Thanks for playing!\nThis is just a demo.\nMore levels coming soon!";
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
