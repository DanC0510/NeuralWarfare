using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")] 
    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button QuitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGameButton.onClick.AddListener(() => TransferScene("MainGame"));
        QuitButton.onClick.AddListener(() => QuitGame());
    }

    
    void TransferScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
