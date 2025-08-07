using UnityEngine;

public class LaneGizmoDrawer : MonoBehaviour
{
    public float[] laneCenters = new float[] { -1.4f, 0f, 1.4f };
    public float laneWidth = 0.8f;
    public float laneLength = 200f;
    public float yOffset = 0.05f;

    private void OnDrawGizmos()
    {
        for (int i = 0; i < laneCenters.Length; i++)
        {
            float x = laneCenters[i];
            Vector3 center = new Vector3(x, yOffset, 0f);
            Vector3 size = new Vector3(laneWidth, 0.01f, laneLength);

            
            if (i == 0) Gizmos.color = Color.red;       
            else if (i == 1) Gizmos.color = Color.white;
            else Gizmos.color = Color.green;            

            Gizmos.DrawCube(center, size);
        }
    }
}
