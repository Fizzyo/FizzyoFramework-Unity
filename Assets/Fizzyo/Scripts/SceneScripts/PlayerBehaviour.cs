using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fizzyo;

public class PlayerBehaviour : MonoBehaviour {

    //Speed to move character at
    public float speed = 0.04f;
    public float flyHeight = 3.0f;
    Vector3 destPos;
    // Use this for initialization
    void Start () {
        destPos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        //move the player forward
        float x = transform.position.x + speed;
        //set height of the player using the player breath intensity
        float y = FizzyoFramework.Instance.Device.Pressure() * flyHeight;
        //Device.Pressure() can return negative numbers if the player is breathing in. Clamp the player height to be above 0
        y = Mathf.Max(y, 0);

        transform.position = new Vector3(x,y, 0);
	}
}
