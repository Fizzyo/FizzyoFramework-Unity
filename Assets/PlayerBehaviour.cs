using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fizzyo;

public class PlayerBehaviour : MonoBehaviour {

    //Speed to move character at
    public float speed = 0.04f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float x = transform.position.x + speed;
        float y = FizzyoFramework.Instance.Device.Pressure()*5.0f;

        transform.position = new Vector3(x,y, 0);
	}
}
