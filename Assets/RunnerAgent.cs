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
    Vector3 pos = transform.position;
    sensor.AddObservation(pos.x);
    sensor.AddObservation(pos.z);
    sensor.AddObservation(movement.playerPosition);
    sensor.AddObservation(movement.isGrounded ? 1f : 0f);

    for (int lane = 0; lane < 3; lane++)
    {
        float minDist = 1000f; // Use a big number for "no obstacle"
        foreach (var obs in FindObjectsOfType<ObstacleScript>())
        {
            float dz = obs.transform.position.z - transform.position.z;
            // Now, include obstacles slightly behind the agent (to catch near-collisions)
            if (obs.laneIndex == lane && dz > -1.0f)
            {
                if (dz < minDist) minDist = dz;
            }
        }
        sensor.AddObservation(minDist);

        // Logging: show only for current lane to avoid spam
        if (lane == movement.playerPosition)
        {
            Debug.Log(
                $"[ObsLog] Agent at Z={transform.position.z:F1}, Lane={lane}: Nearest obstacle is {minDist:F1} units ahead"
            );
        }
    }

    // Additional debug for agent's actual lane and X position (optional)
    Debug.Log($"[AgentObs] agentLane={movement.playerPosition}, agentX={transform.position.x:F2}");
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
