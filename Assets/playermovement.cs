using UnityEngine;

public class playermovement : MonoBehaviour
{
    public AudioSource audio;
    public AudioSource audiobeforespace;
    public float speed;
    public float rotationSpeed;
    public Animator anim;
    public float MoveDistance = 2f;
    public bool check = true;

    private int playerPosition = 1;
    public bool runn = true;
    public bool isGrounded = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        random_animations();
        runn = true;
        anim.SetBool("run", runn);
    }

    void Update()
    {
        runcheck();

        if (Input.GetKey(KeyCode.Escape))
        {
            // optional escape/pause logic
        }
    }

    void random_animations()
    {
        int m = Random.Range(0, 4);
        anim.Play("d" + (m + 1), -1, 1f);
    }

    void RUN()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        Debug.Log("Moving Forward → Speed: " + speed + ", Position: " + transform.position);
    }

    void runcheck()
    {
        if (runn)
        {
            audiobeforespace?.Stop();
            if (check)
            {
                // audio?.Play();
                check = false;
            }
            RUN();
        }
    }

    void end()
    {
        FindObjectOfType<GameManager>()?.endgame();
    }

    void endjump()
    {
        anim.SetBool("j", false);
        anim.Play("run");
    }

    // ✅ Required by PythonBridge.cs
    public void MoveLeft()
    {
        if (!runn) return;

        if (playerPosition > 0)
        {
            transform.Translate(-MoveDistance, 0, 0);
            playerPosition--;
            Debug.Log("⏪ Move Left (from Python)");
        }
    }

    public void MoveRight()
    {
        if (!runn) return;

        if (playerPosition < 2)
        {
            transform.Translate(MoveDistance, 0, 0);
            playerPosition++;
            Debug.Log("⏩ Move Right (from Python)");
        }
    }

    public void Jump()
    {
        if (!runn || !isGrounded) return;

        anim.SetBool("j", true);
        isGrounded = false;
        Debug.Log("⏫ Jump (from Python)");
        // Optionally trigger animation event to call `endjump()`
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("☠️ Hit obstacle, disabling player!");
            gameObject.SetActive(false);  // Triggers 'done = True'
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
