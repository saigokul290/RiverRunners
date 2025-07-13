using UnityEngine;

public class lane: MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Vector3 pos = transform.position;
            Debug.Log($"{gameObject.name} is at X = {pos.x} (Y = {pos.y}, Z = {pos.z})");
        }
    }
}
