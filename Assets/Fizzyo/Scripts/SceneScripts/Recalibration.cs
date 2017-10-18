using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;

using Fizzyo;

public class Recalibration : MonoBehaviour {

    // Game Objects used withing the scene - assigned through unity script component
    public GameObject tagChange;
    public GameObject calChange;
    public GameObject backButton;
    public GameObject tagError;
    public GameObject calResults;

    // Game Object for showing tag upload errors
    private Button startCal;
    private Button changeTag;
    private Button begin;

    // Text used within the scene
    private Text tag1;
    private Text tag2;
    private Text tag3;
    private Text status;
    private Text startButton;
    private Text results;

    // Slider used within the scene
    private Slider slider;

    // Used to hold an instance of calibration
    //private Calibration currentCal;

    // Shows whether the calibration has finished
    bool calComplete = false;

    //minimun threshold to be considered a breath
    float minThreshold = 0.01f;
    float holdBreathFor = 2.0f;//breath out for at least 2 seconds
    float breathRepetitions = 1.0f; //how many times to blow out.
    private Calibration currentCal;

    /// <summary>
    /// Sets up the scene
    /// </summary
    void Start()
    {

        SceneSetup();

        currentCal = new Calibration();


        // Give button correct value 
        // Give status correct value

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
            status.text = currentCal.calibrationStatus;
            status.color = currentCal.calibrationColor;

            if (currentCal.calibrating == false)
            {
                startButton.text = "Start Step " + currentCal.calibrationStep;
                changeTag.interactable = true;
                startCal.interactable = true;
            }
        }

        if (currentCal.calibrationFinished == true && calComplete == false)
        {
            calComplete = true;
            StartCoroutine("EndCalibration");
        }
    }
		


}
