
using UnityEngine;
using UnityEditor;

public class SnapObstaclesToLaneCenters
{
    [MenuItem("Tools/Snap Obstacles to Lane Centers")]
    public static void SnapAllObstacles()
    {
        float[] laneCenters = new float[3] { -1.4f, 0f, 1.4f };
        int snapCount = 0;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            Vector3 pos = obj.transform.position;
            float closest = laneCenters[0];
            float minDist = Mathf.Abs(pos.x - laneCenters[0]);
            for (int i = 1; i < laneCenters.Length; i++)
            {
                float dist = Mathf.Abs(pos.x - laneCenters[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = laneCenters[i];
                }
            }
            obj.transform.position = new Vector3(closest, pos.y, pos.z);
            snapCount++;
        }
        Debug.Log($"[SnapObstaclesToLaneCenters] Snapped {snapCount} obstacles to lane centers!");
    }
}
