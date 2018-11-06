// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

public class MenuCamera : MonoBehaviour
{

    public float camSpeed = 0.1f;

    private void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.Translate(camSpeed, 0, 0);
    }
}