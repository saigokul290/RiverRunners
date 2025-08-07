using UnityEngine;

[ExecuteInEditMode]
public class ObstracleAutoLaneSetter : MonoBehaviour
{
    public float[] laneCenters = new float[3] { -1.4f, 0f, 1.4f };

    void Awake()
    {
        SetLaneIndex();
    }

    #if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying)
            SetLaneIndex();
    }
    #endif

    void SetLaneIndex()
    {
        ObstacleScript obs = GetComponent<ObstacleScript>();
        if (obs == null) return;

        float x = transform.position.x;
        int lane = 0;
        float minDist = Mathf.Abs(x - laneCenters[0]);
        for (int i = 1; i < laneCenters.Length; i++)
        {
            float dist = Mathf.Abs(x - laneCenters[i]);
            if (dist < minDist)
            {
                lane = i;
                minDist = dist;
            }
        }
        obs.laneIndex = lane;
    }
}
