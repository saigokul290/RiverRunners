using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerUp : MonoBehaviour
{
    public AudioClip clip;
    public score scoreincreament;

    public ParticleSystem pickUpEffect;

    public void OnTriggerEnter(Collider obj)
    {
        if (obj.name == "aj") // consider switching to tag-based check (CompareTag) for robustness
        {
            RunnerAgent agent = obj.GetComponent<RunnerAgent>();
            if (agent != null)
            {
                agent.CollectDiamond(); // triggers stacking speed boost
                Debug.Log("[PowerUp] Diamond collected: triggering stacking speed boost.");
            }

            pickUp();
        }
    }

    public void pickUp()
    {
        Instantiate(pickUpEffect, transform.position, transform.rotation);
        if (scoreincreament != null)
        {
            scoreincreament.check = true;
        }
        AudioSource.PlayClipAtPoint(clip, this.gameObject.transform.position);
        Destroy(gameObject);
    }

    void Update()
    {
        // Optional: rotate gem if you want
        // transform.Rotate(0, 1, 0);
    }
}
