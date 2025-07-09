using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class powerUp : MonoBehaviour
{
    public AudioClip clip;
    public score scoreincreament;

    public ParticleSystem pickUpEffect;

    public void OnTriggerEnter(Collider obj)
    {
        if (obj.name == "aj")
        {
            // Get the RunnerAgent component from the player
            RunnerAgent agent = obj.GetComponent<RunnerAgent>();
            if (agent != null)
            {
                agent.AddReward(0.2f);  // Give a bigger reward for rare gem
                Debug.Log("Reward added: +0.2 for gem");
            }

            pickUp();
        }
    }

    public void pickUp()
    {
        Instantiate(pickUpEffect, transform.position, transform.rotation);
        scoreincreament.check = true;
        AudioSource.PlayClipAtPoint(clip, this.gameObject.transform.position);
        GameObject.Destroy(gameObject);
    }

    void Update()
    {
        // Optional: rotate gem if you want
        // transform.Rotate(0, 1, 0);
    }
}
