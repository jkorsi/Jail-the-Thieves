using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScoreManager : MonoBehaviour
{
    static int score = 0;
    static int highScore = 0;
    static bool highScoreSetToUI = false;
    static string currentScene;

    private void Start()
    {
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        highScore = PlayerPrefs.GetInt(currentScene + "HighScore", 0);
        highScoreSetToUI = UIManager.UpdateHighScoreUI(highScore); 
    }

    public static void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
        {
            SetHighScore(score);
            UIManager.UpdateHighScoreUI(score);
        }

        // Update UI
        UIManager.UpdateScoreUI(score);
    }

    private void OnDestroy()
    {
        ResetScore();
    }

    public static void ResetScore()
    {
        score = 0;
    }

    public static int GetScore()
    {
        return score;
    }

    public static int GetHighScore()
    {
        return highScore;
    }

    public static void SetHighScore(int newHighScore)
    {
        highScore = newHighScore;
        //PlayerPrefs.SetInt("HighScore", highScore);

        // Set highscore per scene
        PlayerPrefs.SetInt(currentScene + "HighScore", highScore);
        PlayerPrefs.Save();
    }

}
