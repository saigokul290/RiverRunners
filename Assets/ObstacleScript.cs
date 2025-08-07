using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    public int laneIndex;
    public bool recentlyNear = false;
    public bool passed = false;

   void Start()
    {
        float[] laneCenters = new float[] { -1.4f, 0f, 1.4f };
        float x = transform.position.x;

        float minDist = float.MaxValue;
        int closestLane = -1;
        for (int i = 0; i < laneCenters.Length; i++)
        {
            float dist = Mathf.Abs(x - laneCenters[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closestLane = i;
            }
        }

        laneIndex = closestLane;
        Debug.Log($"[LaneAssign] Obstacle {name} at X={x:F2} assigned to Lane {laneIndex}");
    }
}

