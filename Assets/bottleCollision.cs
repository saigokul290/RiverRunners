using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bottleCollision : MonoBehaviour
{
    public playermovement movement;  // Can be assigned manually or auto-found
    public Animator anim;

    void Start()
    {
        // Auto-assign if not set in Inspector
        if (movement == null)
        {
            GameObject player = GameObject.Find("aj");
            if (player != null)
            {
                movement = player.GetComponent<playermovement>();
                anim = player.GetComponent<Animator>();
            }
            else
            {
                Debug.LogWarning("Player object 'aj' not found in scene.");
            }
        }
    }

    public void OnTriggerEnter(Collider player)
    {
        if (player.name == "aj")
        {
            if (movement != null && anim != null)
            {
                Debug.Log("Bottle hit detected, disabling movement...");
                movement.enabled = false;
                anim.SetBool("drink", true);
                GameObject.Destroy(gameObject);
            }
            else
            {
                Debug.LogError("bottleCollision: Missing references to movement or anim!");
            }
        }
    }
}
