using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerMultiple : MonoBehaviour
{
    private bool gamehasended = false;
    private float restartdelay = 2f;
    public GameObject completelevelui;

    [Header("Speed Settings")]
    public float baseSpeed = 20f;
    public float speedIncreasePerRestart = 2f;

    [Header("Players")]
    public playermovementMultiple[] players; // assign RunnerAgent1 + RunnerAgent2 in Inspector

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

    // Restart AFTER waiting longer (e.g., 3 seconds)
    Invoke(nameof(restart), 3f);
    }


    public void endgame()
    {
        if (!gamehasended)
        {
            gamehasended = true;
            Debug.Log("Game over — resetting players");

            // Reset speed back to initial
            baseSpeed = 15f;

            // Reset all players' positions before restart
            foreach (var player in players)
            {
                if (player != null)
                {
                    float startZ = 0f;

                    if (player.gameObject.name.Contains("RunnerAgent2"))
                        startZ = -5f;
                    else if (player.gameObject.name.Contains("RunnerAgent3"))
                        startZ = -10f;

                    player.ResetPosition(startZ); // use the overload with Z offset
                }
            }

            // Restart scene after delay so fall animation plays
           restart();
        }
    }

    public void restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}