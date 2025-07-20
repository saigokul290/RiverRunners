using UnityEngine;

public class LaneZone : MonoBehaviour
{
    [Tooltip("Right = 0, Center = 1, Left = 2")]
    public bool[] laneAvailable = new bool[3] { true, true, true };  // R, C, L


    private void OnTriggerEnter(Collider other)
    {
        RunnerAgent agent = other.GetComponent<RunnerAgent>();
        if (agent != null)
        {
            agent.SetLaneAvailability(laneAvailable);
            Debug.Log($"[Zone] Lane availability set: R={laneAvailable[0]}, C={laneAvailable[1]}, L={laneAvailable[2]}");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        RunnerAgent agent = other.GetComponent<RunnerAgent>();
        if (agent != null)
        {
            bool[] allLanes = new bool[3] { true, true, true };
            agent.SetLaneAvailability(allLanes);
            Debug.Log("[Zone] All lanes restored after exiting bridge zone.");
        }

    }

}
