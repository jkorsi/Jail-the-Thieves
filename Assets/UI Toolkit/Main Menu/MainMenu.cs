using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public AudioSource clickSound;

    void OnEnable()
    {
        // Load the UI document
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        // Get the buttons by their IDs
        var playGrassButton = rootVisualElement.Q<Button>("PlayGrassLevel");
        var playDesertButton = rootVisualElement.Q<Button>("PlayDesertLevel");
        var quitButton = rootVisualElement.Q<Button>("QuitGame");

        // Assign click event listeners to the buttons
        playGrassButton.clicked += OnPlayGrassClicked;
        playDesertButton.clicked += OnPlayDesertClicked;
        quitButton.clicked += OnQuitClicked;
    }

    private void OnPlayGrassClicked()
    {
        clickSound.Play();

        StartCoroutine(waitForSoundAndLoadLevel(clickSound, "Grass Level"));
    }

    private void OnPlayDesertClicked()
    {
        clickSound.Play();

        StartCoroutine(waitForSoundAndLoadLevel(clickSound, "Desert Level"));
    }

    private void OnQuitClicked()
    {
        clickSound.Play();

        StartCoroutine(waitForSoundAndExit(clickSound));
    }

    IEnumerator waitForSoundAndExit(AudioSource sound)
    {
        //Wait Until Sound has finished playing
        while (sound.isPlaying)
        {
            yield return null;
        }

        //Auidio has finished playing, disable GameObject
        ExitToEditorOrDesktop();
    }

    private static void ExitToEditorOrDesktop()
    {
        #if UNITY_EDITOR
                // If running in the Unity Editor, stop playing
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                        // If running in a build, quit the application
                        Application.Quit();
        #endif
    }


    IEnumerator waitForSoundAndLoadLevel(AudioSource sound, string levelName)
    {
        //Wait Until Sound has finished playing
        while (sound.isPlaying)
        {
            yield return null;
        }

        SceneManager.LoadScene(levelName);
    }
        
}