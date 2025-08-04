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
    public float coinReward = 1.0f;
    public float obstaclePenalty = -2.0f;
    public float forwardRewardPerMeter = 0.02f;
    public float finishBonus = 1.0f;
    private int cooldownSteps = 0;
    private bool[] currentLaneAvailable = new bool[3] { true, true, true };

    [Header("Environment")]
    public float laneWidth = 1.3f;
    public float finishLineZ = -360f;

    private int lastAction = 0;

    public override void Initialize()
    {
        movement = GetComponent<playermovement>();
        lastZPosition = transform.position.z;
        if (movement == null)
        {
            Debug.LogError("[RunnerAgent] playermovement component missing!");
        }
    }

    public override void OnEpisodeBegin()
    {
        episodeEnded = false;
        episodeJustStarted = true;

        if (movement != null)
        {
            movement.ResetSpeed();      // reset any previous boost
            movement.ResetPosition();
            movement.SnapToLaneCenter();
        }

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

        // Normalize agent position and grounded state
        sensor.AddObservation(p.x / 2f);
        sensor.AddObservation(p.z / 20f);
        sensor.AddObservation(movement.isGrounded ? 1f : 0f);

        float[] laneDistances = GetLaneObstacleDistances();
        float[] coinDistances = GetLaneCoinDistances();

        foreach (float d in laneDistances)
            sensor.AddObservation(Mathf.Clamp01(d / 15f));

        for (int i = 0; i < 3; i++)
        {
            float obsDist = laneDistances[i];
            float coinDist = coinDistances[i];

            if (obsDist < 2.5f)
            {
                sensor.AddObservation(0f);
            }
            else
            {
                sensor.AddObservation(Mathf.Clamp01(coinDist / 15f));
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (episodeJustStarted)
        {
            episodeJustStarted = false;
            lastAction = 0;
            return;
        }

        if (episodeEnded) return;

        int action = actions.DiscreteActions[0];
        float[] laneDistances = GetLaneObstacleDistances();
        int safestLane = System.Array.IndexOf(laneDistances, laneDistances.Max());
        float[] coinDistances = GetLaneCoinDistances();

        Debug.Log($"[Action] Step: {StepCount}, Action={action}, Lane={movement.playerPosition}, Safest={safestLane}, Distances=({laneDistances[0]:F1}, {laneDistances[1]:F1}, {laneDistances[2]:F1}), Z={transform.position.z:F1}");

        if (cooldownSteps > 0)
        {
            cooldownSteps--;
            AddReward(-0.005f);
            return;
        }

        if (movement.playerPosition == safestLane)
            AddReward(0.1f);

        float distInLane = laneDistances[movement.playerPosition];
        if (distInLane < 1.5f)
        {
            AddReward(-1.0f);
            Debug.Log($"[Penalty] Too close to obstacle! DistanceZ={distInLane:F2}");
        }
        else if (distInLane < 3f)
        {
            AddReward(-0.1f);
            Debug.Log($"[Warning] Moderate proximity to obstacle in lane {movement.playerPosition}");
        }
        else if (distInLane < 4f && movement.playerPosition != safestLane)
            AddReward(0.05f);
        else if (distInLane < 3f)
            AddReward(-0.05f);

        if (action == lastAction && action != 0)
        {
            AddReward(-0.02f);
            Debug.Log($"[Penalty] Repeated action {action}");
        }

        switch (action)
        {
            case 1: // Left
                if (movement.playerPosition > 0 && currentLaneAvailable[movement.playerPosition - 1])
                {
                    movement.MoveLeft("agent");
                    if (lastAction != 1) AddReward(laneSwitchPenalty);
                }
                else
                {
                    AddReward(-0.5f);
                    Debug.Log("[Blocked] Left lane unavailable");
                }
                break;

            case 2: // Right
                if (movement.playerPosition < 2 && currentLaneAvailable[movement.playerPosition + 1])
                {
                    movement.MoveRight("agent");
                    if (lastAction != 2) AddReward(laneSwitchPenalty);
                }
                else
                {
                    AddReward(-0.5f);
                    Debug.Log("[Blocked] Right lane unavailable");
                }
                break;
        }

        lastAction = action;

        AddReward(stepPenalty);

        float deltaZ = transform.position.z - lastZPosition;
        if (deltaZ < 0)
            AddReward(deltaZ * forwardRewardPerMeter);
        lastZPosition = transform.position.z;

        foreach (var obstacle in FindObjectsOfType<ObstacleScript>())
        {
            float obsX = obstacle.transform.position.x;
            float obsZ = obstacle.transform.position.z;

            float xDiff = Mathf.Abs(movement.laneCenters[movement.playerPosition] - obsX);
            float zDiff = obsZ - transform.position.z;

            if (!obstacle.passed && zDiff < 0f && Mathf.Abs(zDiff) < 5f)
            {
                AddReward(0.1f);
                obstacle.passed = true;
                Debug.Log($"[Reward] Avoided obstacle in lane {movement.playerPosition} (+0.1)");
            }
        }

        if (transform.position.z >= finishLineZ)
        {
            AddReward(finishBonus);
            EndEpisode();
        }
    }

    float[] GetLaneObstacleDistances()
    {
        float[] distances = new float[3] { 1000f, 1000f, 1000f };
        float agentZ = transform.position.z;

        foreach (var obs in FindObjectsOfType<ObstacleScript>())
        {
            int lane = obs.laneIndex;
            float obsZ = obs.transform.position.z;
            float obsX = obs.transform.position.x;
            float dz = obsZ - agentZ; // ahead: dz < 0

            float xCenter = movement.laneCenters[lane];
            float xDiff = Mathf.Abs(obsX - xCenter);

            if (dz < 0f && Mathf.Abs(dz) < 15f && xDiff <= 1.0f)
            {
                if (Mathf.Abs(dz) < distances[lane])
                {
                    distances[lane] = Mathf.Abs(dz);
                    Debug.Log($"[Detected] Lane={lane}, dz={dz:F2}, xDiff={xDiff:F2}, Obstacle={obs.name}");
                }
            }
        }

        return distances;
    }

    public void HitObstacle()
    {
        if (episodeEnded) return;
        episodeEnded = true;

        ObstacleScript nearest = null;
        float minDist = float.MaxValue;

        float agentX = movement.laneCenters[movement.playerPosition];
        float agentZ = transform.position.z;

        foreach (var obs in FindObjectsOfType<ObstacleScript>())
        {
            float obsX = obs.transform.position.x;
            float obsZ = obs.transform.position.z;

            float xDiff = Mathf.Abs(agentX - obsX);
            float zDiff = obsZ - agentZ;

            bool xAligned = xDiff <= 0.6f;
            bool zAligned = Mathf.Abs(zDiff) <= 2.0f;

            if (xAligned && zAligned && Mathf.Abs(zDiff) < minDist)
            {
                nearest = obs;
                minDist = Mathf.Abs(zDiff);
            }
        }

        if (nearest != null)
        {
            Debug.Log($"[Action] Agent hit obstacle! AgentPos: x={transform.position.x:F2}, z={transform.position.z:F2} | " +
                      $"ObstaclePos: x={nearest.transform.position.x:F2}, z={nearest.transform.position.z:F2} | " +
                      $"DistanceZ: {minDist:F2}");
        }
        else
        {
            Debug.LogWarning($"[Action] Agent hit an obstacle, but no nearby obstacle was found!");
        }

        AddReward(obstaclePenalty);
        Debug.Log($"[SUMMARY] Final Z={transform.position.z:F2}, Step={StepCount}");
        EndEpisode();
    }

    public void CollectCoin()
    {
        AddReward(coinReward);
        Debug.Log("[Collect] Coin collected (+reward)");
    }

    public void CollectDiamond()
    {
        if (movement != null)
        {
            movement.BoostSpeedStacking(movement.stackMultiplier, movement.boostDuration);
        }
        else
        {
            Debug.LogWarning("[CollectDiamond] movement reference null; cannot boost speed.");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.RightArrow)) discreteActionsOut[0] = 1;
    }

    public void SetLaneAvailability(bool[] available)
    {
        for (int i = 0; i < 3; i++)
            currentLaneAvailable[i] = available[i];

        Debug.Log($"[Lane Update] Available lanes set: L={available[0]}, C={available[1]}, R={available[2]}");
    }

    float[] GetLaneCoinDistances()
    {
        float[] distances = new float[3] { 1000f, 1000f, 1000f };
        float agentZ = transform.position.z;

        foreach (var coin in GameObject.FindGameObjectsWithTag("coin"))
        {
            float coinX = coin.transform.position.x;
            float coinZ = coin.transform.position.z;
            float dz = coinZ - agentZ;
            if (dz < 0 || dz > 15f) continue;

            for (int lane = 0; lane < 3; lane++)
            {
                float xCenter = movement.laneCenters[lane];
                if (Mathf.Abs(coinX - xCenter) <= 0.6f && dz < distances[lane])
                {
                    distances[lane] = dz;
                }
            }
        }

        return distances;
    }
}
