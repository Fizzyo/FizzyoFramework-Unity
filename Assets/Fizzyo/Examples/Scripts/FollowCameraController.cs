// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

public class FollowCameraController : MonoBehaviour
{

    public GameObject player;       //Public variable to store a reference to the player game object

    private Vector3 offset;         //Private variable to store the offset distance between the player and camera

    // Use this for initialization
    void Start()
    {
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
        Vector3 newCamPos = player.transform.position;
        newCamPos.y = offset.y; //keep y the same

        offset = transform.position - newCamPos;
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {

        Vector3 newCamPos = player.transform.position;
        newCamPos.y = offset.y; //keep y the same

        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        transform.position = newCamPos + offset;
    }
}