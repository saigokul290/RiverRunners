using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    bool gamehasended = false;
    float restartdelay = 2f;
    public GameObject completelevelui;
    // Static so it persists across restarts
    public static float currentSpeed = 15f;
    public float speedIncreasePerRestart = 2f;

     void Start()
    {
        // Apply the current speed to player
        playermovement movement = FindObjectOfType<playermovement>();
        if (movement != null)
        {
            movement.speed = currentSpeed;
            Debug.Log($"[GameManager] Player speed set to: {movement.speed}");
        }
    }
    public void completelevel()
    {
        Debug.Log("level won");
        completelevelui.SetActive(true);
         // Increase speed before restarting
        currentSpeed += speedIncreasePerRestart;
        restart();
    }
    public void endgame()
    {
        if (gamehasended==false)
        {
            gamehasended = true;
            Debug.Log("gave over");
            Invoke("restart", restartdelay);
        }
    }
    public void restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
