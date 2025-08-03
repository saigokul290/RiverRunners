using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool gamehasended = false;
    private float restartdelay = 2f;
    public GameObject completelevelui;

    [Header("Speed Settings")]
    public float baseSpeed = 20f;
    public float speedIncreasePerRestart = 2f;

    [Header("Players")]
    public playermovement[] players; // assign RunnerAgent1 + RunnerAgent2 in Inspector

    void Start()
    {
        ResetSpeeds();
    }

    void ResetSpeeds()
    {
        foreach (var player in players)
        {
            if (player != null)
                player.speed = baseSpeed;
        }
    }

    public void completelevel()
    {
        Debug.Log("Level won!");
        if (completelevelui != null)
            completelevelui.SetActive(true);

        // Increase speed only on successful completion
        baseSpeed += speedIncreasePerRestart;

        // Restart after showing UI
        Invoke(nameof(restart), 2f);
    }

    public void endgame()
    {
        if (!gamehasended)
        {
            gamehasended = true;
            Debug.Log("Game over — resetting to initial speed");

            // Reset speed back to initial
            baseSpeed = 15f;

            // Restart scene after delay so fall animation plays
            Invoke(nameof(restart), restartdelay);
        }
    }

    public void restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
