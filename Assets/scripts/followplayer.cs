﻿using UnityEngine;

public class followplayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public Vector3 cameraOffset; 
    //float y, z;
    // Update is called once per frame
    void Update()
    {

        //transform.position = player.position + offset;
       cameraOffset = new Vector3(0, player.position.y + 3, player.position.z + 5);
       transform.position = cameraOffset;
    }
}
