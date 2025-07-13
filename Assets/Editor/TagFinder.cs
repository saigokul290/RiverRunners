using UnityEngine;
using UnityEditor;

public class TagFinder : MonoBehaviour
{
    [MenuItem("Tools/List All Obstacles")]
    static void ListAllObstacles()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var obj in obstacles)
        {
            Debug.Log("Obstacle: " + obj.name + " at " + obj.transform.position, obj);
        }
        Debug.Log("Found " + obstacles.Length + " obstacles in scene.");
    }
}
