using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RunnerAgent : Agent
{
    private playermovement movement;
    private bool episodeEnded = false;  // Guard flag

    public override void Initialize()
    {
        movement = GetComponent<playermovement>();
        Debug.Log("✅ Agent initialized");
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("🔁 Episode started");
        episodeEnded = false;

        // Reset position and any state variables here
        movement.transform.position = Vector3.zero;

        // Reset your movement script variables (instead of toggling GameObject active)
        movement.runn = true;
        movement.check = true;

        // Enable movement if disabled
        if (!movement.enabled)
            movement.enabled = true;

        // Reset animations if needed
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("fall", false);
            anim.SetBool("drink", false);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 pos = transform.position;
        sensor.AddObservation(pos.x);
        sensor.AddObservation(pos.z);
        // Add more observations as needed
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        Debug.Log("🎮 Action received: " + action);

        switch (action)
        {
            case 1: movement.MoveLeft(); break;
            case 2: movement.MoveRight(); break;
            case 3: movement.Jump(); break;
            default: break;
        }

        AddReward(-0.001f);  // Small step penalty
    }

    // Call this when agent hits obstacle
    public void HitObstacle()
    {
        if (episodeEnded) return;  // Prevent recursion
        episodeEnded = true;

        AddReward(-1f);
        EndEpisode();
    }

    // Call this when agent collects coin
    public void CollectCoin()
    {
        AddReward(0.1f);
        Debug.Log("Reward added: +0.1 for coin");
    }

    // Call this when agent collects diamond
    public void CollectDiamond()
    {
        AddReward(0.2f);
        Debug.Log("Reward added: +0.2 for diamond");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.LeftArrow)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.RightArrow)) discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.Space)) discreteActionsOut[0] = 3;
    }
}
