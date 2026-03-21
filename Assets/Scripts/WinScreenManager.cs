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
        int score = (GameManager.Instance != null) ? GameManager.Instance.TotalScore : 0;
        
        if (scoreText != null)
        {
            scoreText.text = "You defeated SATYR with a score of " + score;
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
