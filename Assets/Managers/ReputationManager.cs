using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ReputationManager : MonoBehaviour
{
    // private UIDocument uiDocument;
    // private static VisualElement[] stars;
    private static int maxReputation; // Max health is now determined at runtime
    private static int currentReputation;

    public static void SetMaxReputation(int newMaxReputation)
    {
        maxReputation = newMaxReputation;
        ResetReputation();
    }

    private void OnDestroy()
    {
        maxReputation = 0;
        currentReputation = 0;
    }

    public static void DecreaseReputation()
    {
        if (currentReputation == 1)
        {
            Debug.Log("Game over! Reputation reached 0.");
            currentReputation = 0;
            UpdateReputationDisplay();
            GameOverManager.ShowGameOverWindow();

            Time.timeScale = 0; // Pause the game
            Debug.Log("Showing game over window...");
        }
        
        if (currentReputation > 1)
        {
            Debug.Log("Decreasing reputation...");
            currentReputation--;
            UpdateReputationDisplay();
        }
    }

    public static void IncreaseReputation()
    {
        if (currentReputation < maxReputation)
        {
            currentReputation++;
            UpdateReputationDisplay();
        }
    }

    public static void ResetReputation()
    {   
        currentReputation = maxReputation;
        UpdateReputationDisplay();
    }

    private static void UpdateReputationDisplay()
    {
        for (int i = 0; i < maxReputation; i++)
        {
            //stars[i].style.opacity = (i < currentReputation) ? 1f : 0.1f;
            if (i < currentReputation)
            {
                UIManager.SetStarOpacityByIndex(i, 1f);
            }
            else
            {
                UIManager.SetStarOpacityByIndex(i, 0.1f);
            }
        }
    }
}