using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class MenuManager : MonoBehaviour
{
    // This function will be called by the Start Button
    public void StartGame()
    {
        // Reset Score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScore();
        }

        // Load the main game scene.
        // IMPORTANT: Make sure your game scene is named "SampleScene" 
        // or change this string to match your game scene's name.
        SceneManager.LoadScene("SampleScene"); 
    }

    // This function will be called by the Quit Button
    public void QuitGame()
    {
        // This line only works in a built game (not in the Unity Editor)
        Application.Quit();

        // This line is for testing in the editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}