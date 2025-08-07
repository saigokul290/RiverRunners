using UnityEngine;

public class playermovementMultiple : MonoBehaviour
{
    public float speed = 5f;
    public Animator anim;

    
    public int playerPosition = 1;   
    public bool isGrounded = true;
    public bool runn = true;

    
    public float[] laneCenters = new float[3] { -1.4f, 0f, 1.4f };

    private float lastLaneSwitchTime = -999f;
    public float laneSwitchCooldown = 0.2f;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        runn = true;
        anim.SetBool("run", runn);

        SnapToLaneCenter();
    }

void Update()
{
    if (!runn) return;
    RUN();

    
}



    void RUN()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }

    public void MoveLeft(string source = "agent")
    {
        if (!runn) return;
        if (playerPosition > 0 && Time.time - lastLaneSwitchTime > laneSwitchCooldown)
        {
            playerPosition--;
            SnapToLaneCenter();
            lastLaneSwitchTime = Time.time;
        }
    }

    public void MoveRight(string source = "agent")
    {
        if (!runn) return;
        if (playerPosition < laneCenters.Length - 1 && Time.time - lastLaneSwitchTime > laneSwitchCooldown)
        {
            playerPosition++;
            SnapToLaneCenter();
            lastLaneSwitchTime = Time.time;
        }
    }

    public void Jump(string source = "agent")
    {
        if (!runn || !isGrounded) return;
        anim.SetBool("j", true);
        anim.SetBool("run", false);
        isGrounded = false;
    }

 void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Ground"))
    {
        isGrounded = true;
    }
    else if (collision.gameObject.CompareTag("Obstacle"))
    {
        Vector3 agentPos = transform.position;
        Vector3 obsPos = collision.transform.position;
        float distanceZ = agentPos.z - obsPos.z;

        if (distanceZ < 0.3f)
        {
            anim.SetTrigger("Fall");
            runn = false;

            
            foreach (var other in FindObjectsOfType<playermovementMultiple>())
                other.runn = false;

            
            followplayerMultiple cam = FindObjectOfType<followplayerMultiple>();
            if (cam != null)
                cam.FocusOn(transform);

            
            GameManagerMultiple gm = FindObjectOfType<GameManagerMultiple>();
            if (gm != null)
            {
                gm.endgame();
                // gm.CancelInvoke(); 
                // gm.Invoke(nameof(gm.endgame), 1.5f);
            }
        }
    }

}


    public void SnapToLaneCenter()
    {
        Vector3 pos = transform.position;
        transform.position = new Vector3(laneCenters[playerPosition], pos.y, pos.z);
    }

    public void ResetPosition(float startZ)
    {
        playerPosition = 1; 
       transform.position = new Vector3(laneCenters[playerPosition], 0.5f, startZ);
 
        isGrounded = true;
        runn = true;

        if (anim == null) anim = GetComponent<Animator>();
        anim.SetBool("j", false);
        anim.SetBool("run", true);
        anim.SetBool("dance", false);
    }

    public void endjump()
    {
        anim.SetBool("j", false);
        anim.SetBool("run", true);
        anim.Play("run");
        Debug.Log("this is a change");
    }
}