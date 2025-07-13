using UnityEngine;

public class playermovement : MonoBehaviour
{
    public float speed = 5f;
    public Animator anim;

    // Lane indices: 0=left, 1=center, 2=right
    public int playerPosition = 1;   // default: center lane
    public bool isGrounded = true;
    public bool runn = true;

    // Use your actual measured lane X centers
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

    // Debug: Print current lane and true X every frame
    Debug.Log($"[PlayerPos] X={transform.position.x:F2}, playerPosition={playerPosition}");
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
    }

    public void SnapToLaneCenter()
    {
        Vector3 pos = transform.position;
        transform.position = new Vector3(laneCenters[playerPosition], pos.y, pos.z);
    }

    public void ResetPosition()
    {
        playerPosition = 1; // center
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
    }
}
