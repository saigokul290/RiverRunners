// File: Assets/Editor/AddObstacleComponents.cs
using UnityEngine;
using UnityEditor;

public class AddObstacleComponents
{
    [MenuItem("Tools/Add Obstacle Components")]
    public static void AddComponentsToObstacles()
    {
        int count = 0;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            if (obj.GetComponent<ObstacleScript>() == null)
            {
                obj.AddComponent<ObstacleScript>();
            }
            if (obj.GetComponent<ObstracleAutoLaneSetter>() == null)
            {
                obj.AddComponent<ObstracleAutoLaneSetter>();
            }
            count++;
        }
        Debug.Log($"[AddObstacleComponents] Added components to {count} obstacle(s)!");
    }
}
