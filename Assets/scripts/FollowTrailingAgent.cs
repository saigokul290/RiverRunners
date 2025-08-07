using UnityEngine;

public class FollowTrailingAgent : MonoBehaviour
{
    public Transform RunnerAgent1;
    public Transform RunnerAgent2;

    public Vector3 baseOffset = new Vector3(0, 6, -12); // negative Z for -Z movement
    public float smoothSpeed = 0.1f;
    public float zoomFactor = 0.7f;
    public float minY = 6f;
    public float maxY = 20f;

    void LateUpdate()
    {
        if (RunnerAgent1 == null || RunnerAgent2 == null) return;

        // trailing = the one further behind (less negative Z = closer to start)
        Transform trailing = RunnerAgent1.position.z > RunnerAgent2.position.z ? RunnerAgent1 : RunnerAgent2;
        Transform leader   = (trailing == RunnerAgent1) ? RunnerAgent2 : RunnerAgent1;

        float distance = Vector3.Distance(RunnerAgent1.position, RunnerAgent2.position);

        // adjust zoom height and distance
        float zoomY = Mathf.Clamp(baseOffset.y + distance * zoomFactor, minY, maxY);
        float zoomZ = baseOffset.z - distance * zoomFactor; // push further back on -Z

        // position camera behind trailing agent along -Z
        Vector3 desiredPosition = trailing.position + new Vector3(baseOffset.x, zoomY, zoomZ);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // look at midpoint so both visible
        Vector3 midpoint = (RunnerAgent1.position + RunnerAgent2.position) / 2f;
        transform.LookAt(midpoint);

        Debug.Log($"[Camera] Following trailing: {trailing.name}, Distance={distance:F2}");
    }
}