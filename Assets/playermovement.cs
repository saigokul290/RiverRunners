using System.Collections;
using UnityEngine;

public class playermovement : MonoBehaviour
{
    public float speed = 20f;
    [HideInInspector]
    public float defaultSpeed = 5f; 
    public bool speedBoosted = false;

    public Animator anim;

    
    public int playerPosition = 1;   
    public bool isGrounded = true;
    public bool runn = true;

    
    public float[] laneCenters = new float[3] { -1.4f, 0f, 1.4f };

    private float lastLaneSwitchTime = -999f;
    public float laneSwitchCooldown = 0.2f;

    
    private Coroutine boostCoroutine;
    private int currentStacks = 0;

    [Header("Stacking Speed Boost")]
    public int maxStacks = 3;             
    public float stackMultiplier = 1.2f;  
    public float boostDuration = 10f;     

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        runn = true;
        anim.SetBool("run", runn);

        defaultSpeed = speed; 
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
            Debug.Log("[Collision] Landed on ground — isGrounded = true");
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vector3 agentPos = transform.position;
            Vector3 obsPos = collision.transform.position;
            float distanceZ = agentPos.z - obsPos.z;

            
            if (distanceZ < 0.3f)
            {
                Debug.Log($"[ValidHit] AgentPos: z={agentPos.z:F2} | ObstaclePos: z={obsPos.z:F2} | DistanceZ: {distanceZ:F2}");

                anim.SetTrigger("Fall");
                runn = false;
                Debug.Log("[Movement] Player movement stopped after collision (runn = false)");

                RunnerAgent agent = GetComponent<RunnerAgent>();
                if (agent != null)
                {
                    agent.HitObstacle();  
                    Debug.Log("[Reward] Called agent.HitObstacle() → Ended episode with penalty");
                }
            }
        }
        else
        {
            Debug.Log($"[Collision] Hit non-ground, non-obstacle object: '{collision.gameObject.name}'");
        }
    }

    public void SnapToLaneCenter()
    {
        Vector3 pos = transform.position;
        transform.position = new Vector3(laneCenters[playerPosition], pos.y, pos.z);
    }

    public void ResetPosition()
    {
        playerPosition = 1; 
        transform.position = new Vector3(laneCenters[playerPosition], 0.5f, 0f);
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

    
    public void BoostSpeedStacking(float multiplierPerStack, float duration)
    {
        if (currentStacks == 0)
        {
            
            speed *= multiplierPerStack;
            currentStacks = 1;
        }
        else if (currentStacks < maxStacks)
        {
            speed *= multiplierPerStack;
            currentStacks++;
        }
        else
        {
            Debug.Log("[SpeedBoostStacking] Max stacks reached; refreshing timer only.");
        }

        speedBoosted = currentStacks > 0;

        
        if (boostCoroutine != null)
            StopCoroutine(boostCoroutine);
        boostCoroutine = StartCoroutine(BoostTimer(duration));

        Debug.Log($"[SpeedBoostStacking] Stacks={currentStacks}, Speed={speed:F2}, duration refreshed to {duration}s");
    }

    private IEnumerator BoostTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        speed = defaultSpeed;
        speedBoosted = false;
        currentStacks = 0;
        boostCoroutine = null;
        Debug.Log("[SpeedBoostStacking] All stacks expired; speed reset.");
    }

    public void ResetSpeed()
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            boostCoroutine = null;
        }

        speed = defaultSpeed;
        speedBoosted = false;
        currentStacks = 0;
        Debug.Log("[SpeedReset] Speed reset to default");
    }
}
