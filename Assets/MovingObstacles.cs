using UnityEngine;

public class MovingObstacles : MonoBehaviour
{
    public float speed = 2f;
    public float leftBound = -2.1f;
    public float rightBound = 2.0f;

    private int direction = 1;

    void FixedUpdate()
    {
        // Reverse direction at bounds
        if (transform.position.x <= leftBound)
            direction = 1;
        else if (transform.position.x >= rightBound)
            direction = -1;

        // Move obstacle left or right
        transform.Translate(Vector3.right * speed * direction * Time.deltaTime);
    }
}
