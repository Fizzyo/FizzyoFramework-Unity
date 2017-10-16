using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fizzyo;

public class PressureGraph : MonoBehaviour {


    public float maxTime = 3.0f;
    public int dataPoints = 10;
    public float width = 200.0f;
    public float height = 100.0f;
    public GameObject graphBar = null;
    private GraphBar[] graphBars;
    float currentSecond = 0.0f;
    float startTime = 0;
    private bool exhaling;

    // Use this for initialization
    void Start () {

        //Hoockup the breath recognizer
        FizzyoFramework.Instance.Recogniser.ExhalationStarted += OnBreathStarted;
        FizzyoFramework.Instance.Recogniser.ExhalationComplete += OnBreathEnded;

        //setup default height of graph

        float spacing = width / dataPoints;
        graphBars = new GraphBar[dataPoints];
        int xPos = 0;
        for(int i = 0; i < graphBars.Length; i++)
        {
            GameObject gameObject = Object.Instantiate(graphBar);
            gameObject.transform.SetParent(this.transform);
            graphBars[i] = gameObject.AddComponent(typeof(GraphBar)) as GraphBar;
            graphBars[i].transform.localPosition = new Vector3(graphBar.transform.localPosition.x + (spacing*i), graphBar.transform.localPosition.y, graphBar.transform.localPosition.z);
            graphBars[i].transform.localScale = new Vector3(graphBars[i].transform.localScale.x, 0, graphBars[i].transform.localScale.z);

        }

        //hide template oject
        graphBar.GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update () {
        if (exhaling)
        {
            float realtimeSinceStartup = Time.realtimeSinceStartup;

            if (realtimeSinceStartup - startTime >= maxTime)
                startTime = realtimeSinceStartup;

            float delta = realtimeSinceStartup - startTime;
            int i = (int)((delta / maxTime) * (dataPoints));
            GraphBar g = graphBars[i];
            g.TweenToScale(FizzyoFramework.Instance.Device.Pressure() * 50.0f);
        }
    }



    void OnBreathStarted(object sender)
    {
        ZeroGraph();
        startTime = Time.realtimeSinceStartup;
        exhaling = true;
    }

    void OnBreathEnded(object sender, ExhalationCompleteEventArgs e)
    {
        ZeroGraph();
        exhaling = false;
    }

    void ZeroGraph()
    {
        for (int i = 0; i < graphBars.Length; i++)
        {
            GraphBar g = graphBars[i];
            g.TweenToScale(0);
        }
    }
}
