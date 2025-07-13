using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    public int laneIndex;
    [HideInInspector] public bool passed = false;
    [HideInInspector] public bool recentlyNear = false;

    void Start()
    {
        Debug.Log($"[ObstacleDebug] {gameObject.name} - laneIndex={laneIndex}, X={transform.position.x:F2}");
    }
}
