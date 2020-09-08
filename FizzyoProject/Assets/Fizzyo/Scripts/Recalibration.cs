// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Fizzyo;
using UnityEngine;
using UnityEngine.UI;

public class Recalibration : MonoBehaviour {

    // Game Objects used withing the scene - assigned through unity script component
    public GameObject tagChange;
    public GameObject calChange;
    public GameObject backButton;
    public GameObject tagError;
    public GameObject calResults;

    // Slider used within the scene
    private Slider slider;

    // Shows whether the calibration has finished
    bool calComplete = false;

    private Calibration currentCal;

    /// <summary>
    /// Sets up the scene
    /// </summary
    void Start()
    {
        SceneSetup();

        currentCal = new Calibration();
    }

    /// <summary>
    /// Sets up the scene
    /// </summary>
    private void SceneSetup()
    {
        slider = GameObject.Find("Slider").GetComponent<Slider>();
    }

    /// <summary>
    /// Calibration starts if the calibration button is pressed
    /// </summary>
    void Update()
    {
        if (currentCal.calibrating == true) {

            currentCal.Calibrate();
            slider.value = currentCal.pressure;
        }

        if (currentCal.calibrationFinished == true && calComplete == false)
        {
            calComplete = true;
            StartCoroutine("EndCalibration");
        }
    }
}