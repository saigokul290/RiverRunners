using UnityEngine;

public class followplayer : MonoBehaviour
{
    public Transform RunnerAgent1;
    public Transform RunnerAgent2;
    private Transform currentTarget;

    public Vector3 offset = new Vector3(0, 3, 5);

    void Start()
    {
        // Default focus on RunnerAgent1
        currentTarget = RunnerAgent1;
    }

    void Update()
    {
        if (currentTarget == null) return;

        Vector3 newPos = new Vector3(
            0,
            currentTarget.position.y + offset.y,
            currentTarget.position.z + offset.z
        );

        transform.position = newPos;
    }

    // Called by whichever agent crashes
    public void FocusOn(Transform crashedAgent)
    {
        currentTarget = crashedAgent;
        Debug.Log($"[Camera] Focused on {crashedAgent.name}");
    }
}
