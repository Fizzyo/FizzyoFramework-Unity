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
    private Calibration currentCal;

    // Shows whether the calibration has finished
    bool calComplete = false;

    /// <summary>
    /// Sets up the scene
    /// </summary
    void Start()
    {

        SceneSetup();

        currentCal = new Fizzyo.Calibration();


        // Give button correct value 
        // Give status correct value

    }

    /// <summary>
    /// Sets up the scene
    /// </summary>
    private void SceneSetup()
    {

        tagChange.SetActive(true);
        calChange.SetActive(true);
        backButton.SetActive(true);
        calResults.SetActive(true);

        results = calResults.transform.GetChild(0).GetComponent<Text>();

        startCal = GameObject.Find("StartCal").GetComponent<Button>();
        startButton = startCal.GetComponentInChildren<Text>();

        changeTag = GameObject.Find("ChangeTag").GetComponent<Button>();
        begin = GameObject.Find("Begin").GetComponent<Button>();

        slider = GameObject.Find("Slider").GetComponent<Slider>();

        tag1 = GameObject.Find("Tag1").GetComponent<Text>();
        tag2 = GameObject.Find("Tag2").GetComponent<Text>();
        tag3 = GameObject.Find("Tag3").GetComponent<Text>();

        status = GameObject.Find("Status").GetComponent<Text>();


        GameObject.Find("Tag1Up").GetComponent<Button>().onClick.AddListener(Tag1UpClick);
        GameObject.Find("Tag1Down").GetComponent<Button>().onClick.AddListener(Tag1DownClick);
        GameObject.Find("Tag2Up").GetComponent<Button>().onClick.AddListener(Tag2UpClick);
        GameObject.Find("Tag2Down").GetComponent<Button>().onClick.AddListener(Tag2DownClick);
        GameObject.Find("Tag3Up").GetComponent<Button>().onClick.AddListener(Tag3UpClick);
        GameObject.Find("Tag3Down").GetComponent<Button>().onClick.AddListener(Tag3DownClick);

        GameObject.Find("Cancel").GetComponent<Button>().onClick.AddListener(CancelClick);
        GameObject.Find("Menu").GetComponent<Button>().onClick.AddListener(LoadMenuClick);

        begin.onClick.AddListener(BeginClick);
        changeTag.GetComponent<Button>().onClick.AddListener(ChangeTagClick);
        startCal.GetComponent<Button>().onClick.AddListener(StartCalClick);

        // Check if calibration has been done by checking if tag is set for the user
        if (PlayerPrefs.GetInt("calDone") == 0)
        {
            backButton.SetActive(false);
        }

        if (PlayerPrefs.GetInt("online") == 0)
        {
            GameObject.Find("ChangeTag").SetActive(false);
        }

        tagChange.SetActive(false);
        calResults.SetActive(false);
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
		
	// Stops all interaction and waits before changing scene
	private IEnumerator EndCalibration()
	{
        startCal.interactable = false;
		changeTag.interactable = false;

        status.text = currentCal.calibrationStatus;
        status.color = currentCal.calibrationColor;

        yield return new WaitForSeconds(1);

        calResults.SetActive(true);
        calChange.SetActive(false);

        results.text = "Calibrated Pressure (Between 0 and 1) " + Environment.NewLine + PlayerPrefs.GetFloat("calPressure") + Environment.NewLine + Environment.NewLine + "Calibrated Breath Length " + Environment.NewLine + PlayerPrefs.GetFloat("calTime") + "s";
      
	}

    /// <summary>
    /// Loads the menu when the Back to Menu Button is pressed
    /// </summary>
    void LoadMenuClick()
    {
        SceneManager.LoadScene("Menu");
    }

    // Tag methods used to change the tag with arrows
    void Tag1UpClick()
	{
		char myChar = (tag1.text)[0];
		myChar = ++myChar;
		int i = (int)myChar;
		tag1.text = myChar.ToString();

		if (i == 91) 
			tag1.text = "A";
	}
	void Tag1DownClick()
	{
		char myChar = (tag1.text)[0];
		myChar = --myChar;
		int i = (int)myChar;
		tag1.text = myChar.ToString();

		if (i == 64) 
			tag1.text = "Z";
	}
	void Tag2UpClick()
	{
		char myChar = (tag2.text)[0];
		myChar = ++myChar;
		int i = (int)myChar;
		tag2.text = myChar.ToString();

		if (i == 91) 
			tag2.text = "A";
	}
	void Tag2DownClick()
	{
		char myChar = (tag2.text)[0];
		myChar = --myChar;
		int i = (int)myChar;
		tag2.text = myChar.ToString();

		if (i == 64) 
			tag2.text = "Z";
	}
	void Tag3UpClick()
	{
		char myChar = (tag3.text)[0];
		myChar = ++myChar;
		int i = (int)myChar;
		tag3.text = myChar.ToString();

		if (i == 91) 
			tag3.text = "A";
	}
	void Tag3DownClick()
	{
		char myChar = (tag3.text)[0];
		myChar = --myChar;
		int i = (int)myChar;
		tag3.text = myChar.ToString();

		if (i == 64) 
			tag3.text = "Z";
	}

    // Checks that tag is available and moves to calibration
    void BeginClick()
	{
        /*

        string fullTag = tag1.text + tag2.text + tag3.text;

        string tagUpload = Upload.UserTag(fullTag);

        if (tagUpload != "Tag Upload Complete")
        {
            tagError.GetComponent<Text>().text = tagUpload;
            tagError.SetActive(true);
        }
        else {
            tagError.SetActive(false);
            tagChange.SetActive(false);
            calChange.SetActive(true);
        }
        */
        
	}

    // Used to go back to changing tag
    void ChangeTagClick()
	{
        tagChange.SetActive (true);
        calChange.SetActive (false);
	}

    // Used to start calibration
	void StartCalClick()
	{
        currentCal.calibrating = true;
        changeTag.interactable = false;
        startCal.interactable = false;
    }

    void CancelClick()
    {
        tagError.SetActive(false);
        tagChange.SetActive(false);
        calChange.SetActive(true);
    }

}
