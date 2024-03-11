using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIDocument uiDocument; // Assign in the inspector

    private static VisualElement[] stars;
    private static int starsLength; // Max health is now determined at runtime

    private static Label scorePointsText;
    private static Label highScorePointsText;
    private static Label donutText;

    private static string currentScene;

    void Start()
    {
        //uiDocument = FindObjectOfType<UIDocument>();
        uiDocument = GameObject.FindGameObjectWithTag("GameUI").GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        scorePointsText = rootVisualElement.Q<Label>("Score");
        highScorePointsText = rootVisualElement.Q<Label>("HighScore");
        donutText = rootVisualElement.Q<Label>("DonutLabel");

        var starContainer = rootVisualElement.Q<VisualElement>("ReputationContainer");
        if(starContainer == null)
        {
            Debug.LogError("Star container not found");
            return;
        }

        stars = starContainer.Children().ToArray();
        if(stars == null)
        {
            Debug.LogError("Stars not found");
            return;
        }

        starsLength = stars.Length;
        if(starsLength == 0)
        {
            Debug.LogError("No stars found");
            return;
        }

        ReputationManager.SetMaxReputation(starsLength);

        // Example: Update UI initially or subscribe to events
        UpdateScoreUI(0);

        scorePointsText.text = "0";
    }

    public static void UpdateScoreUI(int score)
    {
        if(uiDocument == null)
        {
            Debug.LogError("On Update Score, UI Document not found");
            return;
        }

        if(scorePointsText == null)
        {
            Debug.LogError("Score label not found");
            return;
        }

        scorePointsText.text = $"{score}";
    }

    public static bool UpdateHighScoreUI(int highScore)
    {
        if (uiDocument == null)
        {
            uiDocument = GameObject.FindGameObjectWithTag("GameUI").GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("UI Document not found, probably incorrect order of operations.");
                return false;
            }

            highScorePointsText = uiDocument.rootVisualElement.Q<Label>("HighScore");

            if (highScorePointsText == null)
            {
                Debug.LogError("High score label could not be set, probably incorrect order of operations.");
                return false;
            }
        }
            
        if(highScorePointsText == null)
        {
            Debug.LogError("High score label not found");
            return false;
        }

        highScorePointsText.text = $"{highScore}";
        return true;
    }

    public static void UpdateDonutText(int donutCount)
    {
        if (uiDocument == null)
        {
            Debug.LogError("UI Document not found");
            return;
        }
        if (donutText == null)
        {
            Debug.LogError("Donut label not found");
            return;
        }
        donutText.text = "x " + $"{donutCount}";
    }

    public static int GetStarsLength()
    {
        return starsLength;
    }

    public static VisualElement[] GetStars()
    {
        return stars;
    }

    public static void SetStarOpacityByIndex(int index, float opacity)
    {
        stars[index].style.opacity = opacity;
    }
}