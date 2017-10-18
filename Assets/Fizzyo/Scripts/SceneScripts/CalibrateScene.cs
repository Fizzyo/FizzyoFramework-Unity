using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fizzyo;

public class CalibrateScene : MonoBehaviour {

    public PressureGraph pressureGraph;
    public ProgressEllipse progressEllipse;
    public float minExhaleTime = 3.0f;
    public int requiredBreaths = 1;
    public bool waitForEndOfBreath = false;
    public Text countdown;
    float startTime = 0;
    bool exhaling = false;
    int breathCount = 0;
    List<float> pressureVals = new List<float>();

    // Use this for initialization
    void Start () {
        //Hoockup the breath recognizer
        FizzyoFramework.Instance.Recogniser.BreathStarted += OnBreathStarted;
        FizzyoFramework.Instance.Recogniser.BreathComplete += OnBreathEnded;

        progressEllipse.SetProgress(0);
    }
	
	// Update is called once per frame
	void Update () {

        if (exhaling)
        {
            float exhaleTime = (Time.realtimeSinceStartup -startTime);
            float progress = exhaleTime / minExhaleTime;

            progressEllipse.SetProgress(Mathf.Min(progress,1.0f));
            countdown.text = "" + Mathf.Max(Mathf.Ceil((minExhaleTime - exhaleTime)), 0) ;


            if (!waitForEndOfBreath && exhaleTime >= minExhaleTime && breathCount+1 >= requiredBreaths)
            {
                Calibrate();
                NextScene();
            }
        }

        pressureVals.Add(FizzyoFramework.Instance.Device.Pressure());


    }


    void OnBreathStarted(object sender)
    {
        startTime = Time.realtimeSinceStartup;
        exhaling = true;
    }

    void OnBreathEnded(object sender, ExhalationCompleteEventArgs e)
    {
        float exhaleTime = (Time.realtimeSinceStartup - startTime);
        exhaling = false;

        if(exhaleTime >= minExhaleTime)
        {
            breathCount++;
        }

        if (waitForEndOfBreath && breathCount  >= requiredBreaths)
        {
            Calibrate();
            NextScene();
        }
    }

    void Calibrate()
    {

        float max = 0;
        float min = 0;
        float average = 0;
        float total = 0;
        for(int i = 0; i < pressureVals.Count; i++)
        {
            float v = pressureVals[i];

            total += v;
            max = Mathf.Max(v, max);
            min = Mathf.Min(v, min);

        }
        average = total / pressureVals.Count;

        FizzyoFramework.Instance.Device.SetCalibrationPressure(max);

    }


    void NextScene()
    {
        SceneManager.LoadScene(FizzyoFramework.Instance.CallbackScenePath);
    }


}
