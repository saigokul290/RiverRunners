using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class coinCollision : MonoBehaviour
{
    public AudioClip clip;
    public Text coin;
    public GameObject Cn;

    public void OnTriggerEnter(Collider player)
    { 
        if (player.name == "aj")
        {
            // Get RunnerAgent component from the player
            RunnerAgent agent = player.GetComponent<RunnerAgent>();
            if (agent != null)
            {
                agent.CollectCoin();  // Add reward for ML-Agent
            }

            Counter();

            AudioSource.PlayClipAtPoint(clip, this.gameObject.transform.position);
            GameObject.Destroy(gameObject);
        }
    }

    void Counter()
    {
        coin.text = (int.Parse(coin.text) + 1).ToString();
    }

    private void Update()
    {
        Cn.transform.Rotate(1, 0, 0);
    }
}
