// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Fizzyo;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CalibrateScene : MonoBehaviour {

    public PressureGraph PressureGraph;
    public Image ProgressEllipse;
    private float minExhaleTime = 2.0f;
    private float absMinExhaleTime = 1.0f;
    private int requiredBreaths = 1;
    public Text Countdown;
    public Text DisplayText;
    private bool retry = false;
    private float startTime = 0;
    private bool exhaling = false;
    private int breathCount = 0;
    private float lastBreathLength = 1000;
    private float lastBreathPressure = 1000;
    private List<float> pressureVals = new List<float>();
    private List<float> breathLengthVals = new List<float>();
    private string countdownValue = "0";
    private float progressAmount = 0;


    // Use this for initialization
    void Start ()
    {
        //Hookup the breath recognizer
        FizzyoFramework.Instance.Recogniser.BreathStarted += OnBreathStarted;
        FizzyoFramework.Instance.Recogniser.BreathComplete += OnBreathEnded;

        ProgressEllipse.fillAmount = 0;

        lastBreathLength = PlayerPrefs.HasKey("CurrentBreathLength") ? PlayerPrefs.GetFloat("CurrentBreathLength") : 1000;
        lastBreathPressure = PlayerPrefs.HasKey("CurrentBreathPressure") ? PlayerPrefs.GetFloat("CurrentBreathPressure") : 1000;
    }
	
	// Update is called once per frame
	void Update () {

        if (exhaling)
        {
            float exhaleTime = (Time.realtimeSinceStartup -startTime);
            float progress = exhaleTime / minExhaleTime;

            progressAmount = Mathf.Min(progress,1.0f);
            countdownValue = "" + Mathf.Max(Mathf.Ceil((minExhaleTime - exhaleTime)), 0) ;
        }

        pressureVals.Add(FizzyoFramework.Instance.Device.Pressure());
    }

    private void LateUpdate()
    {
        ProgressEllipse.fillAmount = progressAmount;
        Countdown.text = countdownValue;
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

        if(exhaleTime >= minExhaleTime || exhaleTime >= lastBreathLength)
        {
            retry = false;
            breathCount++;
            breathLengthVals.Add(exhaleTime);
        }
        else if (exhaleTime > absMinExhaleTime)
        {
            if (!retry)
            {
                retry = true;
                DisplayText.text = "Just one more time please\n Keep going!";
                breathLengthVals.Add(exhaleTime);
            }
            else
            {
                retry = false;
                breathCount++;
                breathLengthVals.Add(exhaleTime);
            }
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

        FizzyoFramework.Instance.SetCalibrationLimits(maxPressure, maxBreath);
        PlayerPrefs.SetFloat("CurrentBreathLength", maxBreath);
        PlayerPrefs.SetFloat("LastBreathLength", lastBreathLength);

        PlayerPrefs.SetFloat("CurrentBreathPressure", maxPressure);
        PlayerPrefs.SetFloat("LastBreathPressure", lastBreathPressure);
    }

    void NextScene()
    {
        FizzyoFramework.Instance.Recogniser.BreathStarted -= OnBreathStarted;
        FizzyoFramework.Instance.Recogniser.BreathComplete -= OnBreathEnded;
        if (string.IsNullOrEmpty(FizzyoFramework.Instance.CallbackScenePath))
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(FizzyoFramework.Instance.CallbackScenePath);
        }
    }
}