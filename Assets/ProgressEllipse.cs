using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressEllipse : MonoBehaviour {

    public float l = 50.0f;
    public float x = 0;
    public float y = 0;

	// Use this for initialization
	void Start () {

        int points = 360;
        Vector3[] verts  = new Vector3[points+1];
        Vector3[] normals  = new Vector3[points+1];
        Vector2[] uv = new Vector2[points+1];
        int[] tri = new int[(points*3)];

        MeshFilter mf = GetComponent<MeshFilter>();

        verts[0] = new Vector3(0, 0, 0);
        uv[0] = new Vector3(0, 0);
        float step = (float)points / (Mathf.PI * 2);
        for (int i  = 1;  i < points; i ++) { // For each degrees
            x = Mathf.Cos(i/(Mathf.PI*2)) * l; // I calc the position
            y = Mathf.Sin(i / (Mathf.PI * 2)) * l;
            verts[i] = new Vector3(x, y, 0); // and assign vertices, uv, normals, etc..
            uv[i] = new Vector3(x, y);
            tri[(i - 1) * 3] = 0; //     I don't know if triangles should work like this,
            tri[(i - 1) * 3 + 1] = i; // here I think that triangle is the index of a vertice
            if ((i + 1 > 360))
            {
                tri[(i - 1) * 3 + 2] = 1;
            }
            else
            {
                tri[(i - 1) * 3 + 2] = i + 1;
            }
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tri;
        mesh.uv = uv;
        mesh.normals = normals;

        mf.mesh = mesh;



    }
	
	// Update is called once per frame
	void Update () {
		
	}



}

