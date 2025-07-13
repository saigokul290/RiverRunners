using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    public int laneIndex;
    [HideInInspector] public bool passed = false;
    [HideInInspector] public bool recentlyNear = false;
}
