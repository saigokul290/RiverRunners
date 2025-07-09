using UnityEngine;

public class playercollision : MonoBehaviour
{
    public playermovement movement;
    public Transform player;
    public Animator anim;

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.collider.tag == "Obstacle")
        {
            Debug.Log("aa");
            movement.enabled = false;
            anim.SetBool("fall", true);

            // Get RunnerAgent component and penalize
            RunnerAgent agent = GetComponent<RunnerAgent>();
            if (agent != null)
            {
                agent.HitObstacle();  // Penalize and end episode
            }

            FindObjectOfType<GameManager>().endgame();
        }
        
        // Your commented coin or bottle logic can be adapted similarly if needed
    }
}

        /*     if(collisionInfo.collider.tag == "coin")
             {
                 Debug.Log("j");
                 movement.enabled = false;
                 anim.SetBool("drink", true);
             }
             /*if (collisionInfo.collider.tag == "bottle")
             {
                 Debug.Log("jj");
                 movement.enabled = false;
                 anim.SetBool("drink",true);

                 //FindObjectOfType<GameManager>().endgame();

             }
 
            void end()
        {
            FindObjectOfType<GameManager>().endgame();
        }*/




