using Fizzyo;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CalibrateScene : MonoBehaviour {

    public PressureGraph pressureGraph;
    public ProgressEllipse progressEllipse;
    public float minExhaleTime = 3.0f;
    public int requiredBreaths = 1;
    public Text countdown;
    float startTime = 0;
    bool exhaling = false;
    int breathCount = 0;
    List<float> pressureVals = new List<float>();
    List<float> breathLengthVals = new List<float>();
    bool isCalibrating = false;


    // Use this for initialization
    void Start () {
        //Hookup the breath recognizer
        FizzyoFramework.Instance.Recogniser.BreathStarted += OnBreathStarted;
        FizzyoFramework.Instance.Recogniser.BreathComplete += OnBreathEnded;
        Debug.Log("should only see this once!");
        progressEllipse.SetProgress(0);
        isCalibrating = true;
    }
	
	// Update is called once per frame
	void Update () {

        if (exhaling)
        {
            float exhaleTime = (Time.realtimeSinceStartup -startTime);
            float progress = exhaleTime / minExhaleTime;

            progressEllipse.SetProgress(Mathf.Min(progress,1.0f));
            countdown.text = "" + Mathf.Max(Mathf.Ceil((minExhaleTime - exhaleTime)), 0) ;
        }

        pressureVals.Add(FizzyoFramework.Instance.Device.Pressure());
    }

    void OnBreathStarted(object sender)
    {
        Debug.Log("also started breathing");
        startTime = Time.realtimeSinceStartup;
        exhaling = true;
    }

    void OnBreathEnded(object sender, ExhalationCompleteEventArgs e)
    {
        float exhaleTime = FizzyoFramework.Instance.Recogniser.BreathLength;
        exhaling = false;

        if(exhaleTime >= minExhaleTime)
        {
            breathCount++;
            breathLengthVals.Add(exhaleTime);
        }

        if (breathCount >= requiredBreaths)
        {
            Calibrate();
            Debug.Log("this shouldnt be called");
            if (isCalibrating)
            {
                isCalibrating = false;
                NextScene();
            }
        }
    }

    void Calibrate()
    {
        float maxPressure = 0;
        float minPressure = 0;
        float averagePressure = 0;
        float totalPressure = 0;
        for(int i = 0; i < pressureVals.Count; i++)
        {
            float v = pressureVals[i];

            totalPressure += v;
            maxPressure = Mathf.Max(v, maxPressure);
            minPressure = Mathf.Min(v, minPressure);

        }
        averagePressure = totalPressure / pressureVals.Count;

        float maxBreath = 0;
        for (int i = 0; i < breathLengthVals.Count; i++)
        {
            if (breathLengthVals[i] > maxBreath)
            {
                maxBreath = breathLengthVals[i];
            }
        }

        FizzyoFramework.Instance.Device.SetCalibrationLimits(maxPressure, maxBreath);
    }

    private void NextScene()
    {
        SceneManager.LoadScene(FizzyoFramework.Instance.CallbackScenePath);
    }
}
