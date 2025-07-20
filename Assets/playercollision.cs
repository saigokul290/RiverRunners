using UnityEngine;

public class playercollision : MonoBehaviour
{
    public playermovement movement;
    public Animator anim;

    private float episodeStartTime;
    private float lastCollisionTime = -10f;

    void Start()
    {
        episodeStartTime = Time.time;
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        // Prevent collisions too early after reset (e.g., z=0.0 false hits)
        if (Time.time - episodeStartTime < 0.5f) return;

        // Prevent multiple triggers close together
        if (Time.time - lastCollisionTime < 0.3f) return;
        lastCollisionTime = Time.time;

        Debug.Log($"[Collision] Agent collided with: {collisionInfo.collider.gameObject.name}, tag: {collisionInfo.collider.tag}, position: {collisionInfo.collider.transform.position}");

        if (collisionInfo.collider.CompareTag("Obstacle"))
        {
            movement.enabled = false;
            anim.SetBool("fall", true);

            RunnerAgent agent = GetComponent<RunnerAgent>();
            if (agent != null)
            {
                agent.HitObstacle();
            }

            FindObjectOfType<GameManager>().endgame();
        }
    }

    public void ResetTimer()
    {
        episodeStartTime = Time.time;
        lastCollisionTime = -10f;
    }
}
