using UnityEngine;
using UnityEngine.UI;

public class coinCollision : MonoBehaviour
{
    public AudioClip clip;
    public Text coinCounter;
    public GameObject rotatingVisual;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("aj"))
        {
            RunnerAgent agent = other.GetComponent<RunnerAgent>();
            if (agent != null)
            {
                agent.CollectCoin(); // Reward the agent
            }

            // Update coin UI counter
            if (coinCounter != null)
            {
                int currentCount = int.Parse(coinCounter.text);
                coinCounter.text = (currentCount + 1).ToString();
            }

            // Play sound
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);

            // Destroy the coin object
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Optional rotating visual effect
        if (rotatingVisual != null)
        {
            rotatingVisual.transform.Rotate(1f, 0f, 0f);
        }
    }
}
