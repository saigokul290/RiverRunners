using UnityEngine;

public class playercollision : MonoBehaviour
{
    public playermovement movement;
    public Animator anim;

    void OnCollisionEnter(Collision collisionInfo)
{
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

}
