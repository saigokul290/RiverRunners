using UnityEngine;

public class playercollisionMultiple : MonoBehaviour
{
    public playermovementMultiple movement;
    public Animator anim;

    private float episodeStartTime;
    private float lastCollisionTime = -10f;

    void Start()
    {
        episodeStartTime = Time.time;
        if (anim == null) anim = GetComponent<Animator>();
        if (movement == null) movement = GetComponent<playermovementMultiple>();
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (Time.time - episodeStartTime < 0.5f) return;
        if (Time.time - lastCollisionTime < 0.3f) return;
        lastCollisionTime = Time.time;

        Debug.Log($"[{gameObject.name}] Collided with: {collisionInfo.collider.gameObject.name}, Tag: {collisionInfo.collider.tag}");

        if (collisionInfo.collider.CompareTag("Obstacle"))
        {
            // Crash animation for THIS agent
            movement.runn = false;
            movement.isGrounded = false;
            anim.SetTrigger("Fall");

            // Stop and animate all other agents
            foreach (var other in FindObjectsOfType<playermovementMultiple>())
            {
                other.runn = false;
                Animator otherAnim = other.GetComponent<Animator>();
                if (otherAnim != null)
                {
                    if (other.gameObject != gameObject)
                        otherAnim.SetBool("run", false);
                    else
                        otherAnim.SetTrigger("Fall");
                }
            }

            // Camera focuses on the crashed agent
            followplayerMultiple cam = FindObjectOfType<followplayerMultiple>();
            if (cam != null)
                cam.FocusOn(transform);

            // Notify RL agent for reward penalty
            RunnerAgentMultiple agent = GetComponent<RunnerAgentMultiple>();
            if (agent != null)
                agent.HitObstacle();

            // Restart after short delay
            GameManagerMultiple gm = FindObjectOfType<GameManagerMultiple>();
            if (gm != null)
            {
                gm.endgame();
                // gm.CancelInvoke();
                // gm.Invoke(nameof(gm.endgame), 1.5f);
            }
        }
    }

    public void ResetTimer()
    {
        episodeStartTime = Time.time;
        lastCollisionTime = -10f;
    }
    
}
