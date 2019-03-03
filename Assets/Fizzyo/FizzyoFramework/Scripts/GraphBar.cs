// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

public class GraphBar : MonoBehaviour {

    float destScaleY = 0.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.localScale.y != destScaleY)
        {
            float y = transform.localScale.y + ((destScaleY - transform.localScale.y)*0.05f);
            transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, y/2.0f, transform.localPosition.z);
        }
    }

    public void TweenToScale(float _y)
    {
        destScaleY = _y;
    }
}