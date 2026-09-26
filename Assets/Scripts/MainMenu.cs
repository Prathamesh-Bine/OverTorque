using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Type the exact name of your main game scene here.")]
    public string gameSceneName = "SampleScene"; 

    [Header("UI Panels")]
    public GameObject infoPanel; // The new How To Play screen

    private void Start()
    {
        // Ensure the info panel is hidden when the game starts
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Shutting down system...");
        Application.Quit();
    }

    // --- NEW: Info Panel Controls ---
    public void OpenInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    public void CloseInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
}