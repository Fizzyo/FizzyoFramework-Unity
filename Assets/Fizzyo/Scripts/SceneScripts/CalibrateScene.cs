// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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


    // Use this for initialization
    void Start () {
        //Hookup the breath recognizer
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
            NextScene();
        }
    }

    void Calibrate()
    {
        float maxPressure = 0;
        float minPressure = 0;
        float totalPressure = 0;
        for(int i = 0; i < pressureVals.Count; i++)
        {
            float v = pressureVals[i];

            totalPressure += v;
            maxPressure = Mathf.Max(v, maxPressure);
            minPressure = Mathf.Min(v, minPressure);

        }

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

    void NextScene()
    {
        FizzyoFramework.Instance.Recogniser.BreathStarted -= OnBreathStarted;
        FizzyoFramework.Instance.Recogniser.BreathComplete -= OnBreathEnded;
        SceneManager.LoadScene(FizzyoFramework.Instance.CallbackScenePath);
    }
}