using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreenController : MonoBehaviour
{
    public void ResumeGame()
    {
        // Hide the menu UI and resume time
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void LoadGame()
    {
        // Call your SaveManager to load a game or switch scenes accordingly
        // Example: SceneManager.LoadScene("Level01");
    }

    public void OpenSettings()
    {
        // Show settings panel or navigate to settings scene
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
