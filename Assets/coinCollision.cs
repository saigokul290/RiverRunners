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
                agent.CollectCoin(); 
            }

           
            if (coinCounter != null)
            {
                int currentCount = int.Parse(coinCounter.text);
                coinCounter.text = (currentCount + 1).ToString();
            }

           
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);

            
            Destroy(gameObject);
        }
    }

    void Update()
    {
        
        if (rotatingVisual != null)
        {
            rotatingVisual.transform.Rotate(1f, 0f, 0f);
        }
    }
}
