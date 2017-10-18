using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProgressEllipse : MonoBehaviour {

    public float l = 50.0f;
    public int points = 50;


    [Range(0, Mathf.PI * 2)]
    public float startRad = 0.0f;

    [Range(0, Mathf.PI * 2)]
    public float endRad = Mathf.PI * 2;

    private int prevPoints;
    private float prevEndRad;
    private float prevStartRad;



    //mesh buffers
    private Vector3[] verts;
    private Vector3[] normals;
    private Vector2[] uv;
    private int[] tri;




    void Start() {
        GenerateBuffers();
        UpdateEllipse();
        prevPoints = points;
        prevEndRad = endRad;
        prevStartRad = startRad;
    }



    // Update is called once per frame
    void Update() {

        if (prevPoints != points)
        {
            prevPoints = points;
            GenerateBuffers();
        }

        if (prevEndRad != endRad || prevStartRad != startRad)
        {
            prevEndRad = endRad;
            prevStartRad = startRad;
            UpdateEllipse();
        }
    }

    void GenerateBuffers()
    {
        verts = new Vector3[points + 1];
        normals = new Vector3[points + 1];
        uv = new Vector2[points + 1];
        tri = new int[((points) * 3)];

        verts[0] = new Vector3(0, 0, 0);
        uv[0] = new Vector3(0, 0);

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.back;
        }

    }

    void UpdateEllipse()
    {

        float x, y = 0;

        for (int i = 0; i < points; i++)
        { // For each degrees
            float delta = (float)i / (points - 1);
            float r = delta * -(endRad - startRad) + startRad + (Mathf.PI/2.0f);
            x = Mathf.Cos(r) * l; // I calc the position
            y = Mathf.Sin(r) * l;
            verts[i+1] = new Vector3(x, y, 0); // and assign vertices, uv, normals, etc..
            uv[i+1] = new Vector2(x, y);
            tri[(i) * 3] = 0; //     
            tri[(i) * 3 + 1] = i; //
            tri[(i) * 3 + 2] = i + 1;
        }


        MeshFilter mf = GetComponent<MeshFilter>();

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        mesh.vertices = verts;
        mesh.triangles = tri;
        mesh.uv = uv;
        mesh.normals = normals;

        mf.mesh = mesh;
    }


    //Set progress from 0-1
    public void SetProgress(float progress)
    {
        
        endRad = progress * (Mathf.PI * 2.0f);
    }


}

