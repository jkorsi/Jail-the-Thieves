using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameObject gameOverGameObject;
    public static UIDocument gameOverDocument;
    private static VisualElement gameOverUIFrame;
    
    public static AudioSource clickSound;

    private static Label thievesCaught;
    private static Button playAgainButton;
    private static Button exitButton;
    private static bool isGameOver = false;

    void Start()
    {
        clickSound = GetComponent<AudioSource>();

        gameOverGameObject = GameObject.FindGameObjectWithTag("GameOverUI");
        gameOverDocument = gameOverGameObject.GetComponent<UIDocument>();

        var rootVisualElement = gameOverDocument.rootVisualElement;
        gameOverUIFrame = rootVisualElement.Q<VisualElement>("GameOverUIFrame");

        playAgainButton = rootVisualElement.Q<Button>("PlayAgain");
        exitButton = rootVisualElement.Q<Button>("QuitToMain");

        thievesCaught = rootVisualElement.Q<Label>("ThievesCaught");

        gameOverUIFrame.style.display = DisplayStyle.None;

        // Setup button callbacks
        playAgainButton.clicked += OnPlayAgainClicked;
        exitButton.clicked += OnExitClicked;
        isGameOver = false;
    }

    void OnDestroy()
    {
        gameOverUIFrame.style.display = DisplayStyle.None;
        isGameOver = false;
    }

    static void OnPlayAgainClicked()
    {
        clickSound.Play();
        // Restart the game, assuming you have a single scene, or specify the scene name/ID to load
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        Time.timeScale = 1; // Ensure time is not paused
    }

    static void OnExitClicked()
    {
        clickSound.Play();
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        Time.timeScale = 1; // Ensure time is not paused
    }

    public static bool IsGameOver()
    {
        return isGameOver;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isGameOver)
        {
            Time.timeScale = 1; // Ensure time is not paused
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }
    }

    public static void ShowGameOverWindow()
    {

        if (isGameOver)
        {
            Debug.LogWarning("Game over window already showing");
            // Already showing the game over window
            return;
        }
        if (gameOverDocument == null)
        {
            Debug.LogError("Game over document not found");
            return;
        }   
        if (thievesCaught == null)
        {
            Debug.LogError("Thieves caught label not found");
            return;
        }
        if (playAgainButton == null)
        {
            Debug.LogError("Play again button not found");
            return;
        }
        if (exitButton == null)
        {
            Debug.LogError("Exit button not found");
            return;
        }
        if (clickSound == null)
        {
            Debug.LogError("Click sound not found");
            return;
        }

        Debug.Log("Showing game over window (in GameOVerMAnager)...");

        isGameOver = true;
        int score = ScoreManager.GetScore();
        int highScore = ScoreManager.GetHighScore();

        if(score == 0)
        {
            thievesCaught.text = "You didn't catch any thieves...";
        }
        else if (score == 1)
        {
            thievesCaught.text = "You caught " + score + " thief!";
        }
        else if (score == highScore)
        {
            thievesCaught.text = "Nice, you made a record, and managed to catch " + score + " thieves!";
        } else
        {
            thievesCaught.text = "You caught " + score + " thieves!";
        }

        Debug.Log("Thieves caught: " + score);
        Debug.Log("Adjusting game over window visibility to show the screen");
        gameOverUIFrame.style.display = DisplayStyle.Flex;
    }
}
