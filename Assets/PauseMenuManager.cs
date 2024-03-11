using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuManager : MonoBehaviour
{
    public static GameObject pauseMenuGameObject;
    public static UIDocument pauseMenuDocument;
    public static AudioSource clickSound;

    private static VisualElement pauseMenu;
    private bool isPaused = false;


    void Start()
    {
        clickSound = GetComponent<AudioSource>();

        pauseMenuGameObject = GameObject.FindGameObjectWithTag("PauseMenuUI");
        pauseMenuDocument = pauseMenuGameObject.GetComponent<UIDocument>();
        var rootVisualElement = pauseMenuDocument.rootVisualElement;

        pauseMenu = rootVisualElement.Q<VisualElement>("PauseMenu");

        var continueButton = rootVisualElement.Q<Button>("Continue");
        var mainMenuButton = rootVisualElement.Q<Button>("MainMenu");

        continueButton.clicked += OnContinueClicked;
        mainMenuButton.clicked += OnMainMenuClicked;

        // Initially hide the pause menu window
        pauseMenu.style.display = DisplayStyle.None;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !GameOverManager.IsGameOver())
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        pauseMenu.style.display = DisplayStyle.None;
        clickSound = null;
        pauseMenu = null;
        
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.style.display = isPaused ? DisplayStyle.Flex : DisplayStyle.None;
        Time.timeScale = isPaused ? 0 : 1; // Pause or resume game time
    }

    private void OnContinueClicked()
    {
        clickSound.Play();
        TogglePause();
    }

    private void OnMainMenuClicked()
    {
        clickSound.Play();
        Time.timeScale = 1; // Ensure game time is resumed
        SceneManager.LoadScene("Main Menu"); // Load your main menu scene here
    }

}
