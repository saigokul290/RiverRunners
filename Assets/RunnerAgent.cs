using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

public class RunnerAgent : Agent
{
    private playermovement movement;
    private bool episodeEnded = false;
    private bool episodeJustStarted = false;
    private float lastZPosition;

    [Header("Rewards")]
    public float stepPenalty = -0.001f;
    public float laneSwitchPenalty = -0.02f;
    public float jumpPenalty = -0.02f;
    public float coinReward = 1.0f;
    public float diamondReward = 2.0f;
    public float obstaclePenalty = -1.5f;
    public float forwardRewardPerMeter = 0.005f;
    public float finishBonus = 1.0f;

    public float finishLineZ = 100f;
    private int lastAction = 0;
int GetLaneIndex(float x)
{
    if (x > 0.7f)
        return 0; // Left
    else if (x < -0.7f)
        return 2; // Right
    else
        return 1; // Center
}

    public override void Initialize()
    {
        movement = GetComponent<playermovement>();
        lastZPosition = transform.position.z;
    }
    // Returns the distance to the nearest obstacle in the same lane and ahead of the agent
float GetDistanceToNearestObstacle()
{
    float minDist = 1000f; // Use a large default value
    int agentLane = GetComponent<playermovement>().playerPosition;
    float agentZ = transform.position.z;
    ObstacleScript nearestObs = null;

    foreach (var obs in FindObjectsOfType<ObstacleScript>())
    {
        int lane = obs.laneIndex;
        float x = obs.transform.position.x;
        float z = obs.transform.position.z;

        // Print info for all obstacles
        //Debug.Log($"[AllObstacles] {obs.gameObject.name}, laneIndex={lane}, X={x:F2}, Z={z:F2}");

        // Only consider obstacles in the agent's lane
        if (lane != agentLane) continue;

        float dz = z - agentZ;

        // Obstacle is "ahead" if its Z < agentZ (since Z decreases as you move forward)
        if (dz >= 0) continue;

        dz = Mathf.Abs(dz); // convert to positive distance ahead

        Debug.Log($"[AheadObstacle] {obs.gameObject.name}");

        // Track the closest obstacle ahead
        if (dz < minDist)
        {
            minDist = dz;
            nearestObs = obs;
        }
    }

    // Print which obstacle (if any) was found ahead
    if (nearestObs != null)
    {
        Debug.Log($"[NearestObstacle] {nearestObs.gameObject.name}, laneIndex={nearestObs.laneIndex}");
    }
    else
    {
        Debug.Log($"[NearestObstacle] No obstacle ahead in lane {agentLane}");
    }

    return minDist;
}




    public override void OnEpisodeBegin()
    {
        episodeEnded = false;
        episodeJustStarted = true;

        movement.ResetPosition();
        movement.SnapToLaneCenter();
        transform.position = new Vector3(movement.laneCenters[1], 0.5f, 0f); // center lane
        lastZPosition = transform.position.z;

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("j", false);
            anim.SetBool("run", true);
            anim.SetBool("dance", false);
            anim.Play("run");
        }

        movement.isGrounded = true;
        lastAction = 0;

        foreach (var obstacle in FindObjectsOfType<ObstacleScript>())
        {
            obstacle.passed = false;
            obstacle.recentlyNear = false;
        }
    }

 public override void CollectObservations(VectorSensor sensor)
{
    Vector3 p = transform.position;
    // lane -2/0/+2 normalized to [-1,1]
    sensor.AddObservation(p.x / 2f);
    sensor.AddObservation(p.z / 20f);
    sensor.AddObservation(movement.isGrounded ? 1f : 0f);

    float dObs = GetDistanceToNearestObstacle();
    // Optionally: You can still use Raycast for coins if you want
    float dCoin = 1000f;

    // Debugging:
    //Debug.Log($"[ObsLog] Agent at Z={p.z:F1},Lane={movement.playerPosition}: Nearest obstacle is {dObs:F1} units ahead");

    sensor.AddObservation(Mathf.Clamp01(dObs / 100f)); // Normalize to [0,1] over a reasonable distance (change denominator if needed)
    sensor.AddObservation(Mathf.Clamp01(dCoin / 100f)); // Keep this, or add your coin logic as above
}




    public override void OnActionReceived(ActionBuffers actions)
    {
        if (episodeJustStarted)
        {
            episodeJustStarted = false;
            lastAction = 0;
            return;
        }

        if (episodeEnded)
            return;

        int action = actions.DiscreteActions[0];

        switch (action)
        {
            case 1: movement.MoveLeft("agent"); if (lastAction != 1) AddReward(laneSwitchPenalty); break;
            case 2: movement.MoveRight("agent"); if (lastAction != 2) AddReward(laneSwitchPenalty); break;
            case 3: movement.Jump("agent"); if (lastAction != 3) AddReward(jumpPenalty); break;
            default: break;
        }

        lastAction = action;

        AddReward(stepPenalty);

        float deltaZ = transform.position.z - lastZPosition;
        if (deltaZ > 0) AddReward(deltaZ * forwardRewardPerMeter);
        lastZPosition = transform.position.z;

        foreach (var obstacle in FindObjectsOfType<ObstacleScript>())
        {
            if (!obstacle.passed &&
                movement.playerPosition == obstacle.laneIndex &&
                obstacle.transform.position.z > transform.position.z &&
                (obstacle.transform.position.z - transform.position.z) < 3.0f)
            {
                obstacle.recentlyNear = true;
            }

            if (!obstacle.passed &&
                obstacle.recentlyNear &&
                movement.playerPosition == obstacle.laneIndex &&
                transform.position.z > obstacle.transform.position.z)
            {
                AddReward(0.1f);
                obstacle.passed = true;
                obstacle.recentlyNear = false;
                Debug.Log($"Agent avoided an obstacle in lane {movement.playerPosition}!");
            }
        }

        if (transform.position.z >= finishLineZ)
        {
            AddReward(finishBonus);
            EndEpisode();
        }
    }

    public void HitObstacle()
    {
        if (episodeEnded) return;
        episodeEnded = true;
        AddReward(obstaclePenalty);
        Debug.Log($"Agent crashed into obstacle in lane {movement.playerPosition} (0=left, 1=center, 2=right).");
        EndEpisode();
    }

    public void CollectCoin() { AddReward(coinReward); }
    public void CollectDiamond() { AddReward(diamondReward); }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.RightArrow)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.Space)) discreteActionsOut[0] = 3;
    }
}
