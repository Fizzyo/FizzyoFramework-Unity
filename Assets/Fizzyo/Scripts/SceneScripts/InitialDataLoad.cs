// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitialDataLoad : MonoBehaviour
{
    // Game Objects used withing the scene - assigned through unity script component
    public GameObject loadingData;
    public GameObject tagChange;
    public GameObject offline;

    // Game Object for showing tag upload errors
    private GameObject tagError;

    // Text used within the scene
    private Text tag1;
    private Text tag2;
    private Text tag3;
    private Text loadingDataText;

    /// <summary>
    /// Sets up the scene and begins attempting to load data from the Fizzyo API
    /// </summary>
    void Start()
    {
        SceneSetup();

        StartCoroutine("LoadUserData");
    }

    /// <summary>
    /// Sets up the scene
    /// </summary>
    private void SceneSetup()
    {
        Screen.SetResolution(1280, 800, false);

        loadingData.SetActive(true);
        tagChange.SetActive(true);
        offline.SetActive(true);

        tagError = GameObject.Find("TagError");

        tag1 = GameObject.Find("Tag1").GetComponent<Text>();
        tag2 = GameObject.Find("Tag2").GetComponent<Text>();
        tag3 = GameObject.Find("Tag3").GetComponent<Text>();
        loadingDataText = GameObject.Find("LoadingData").GetComponent<Text>();

        GameObject.Find("Tag1Up").GetComponent<Button>().onClick.AddListener(Tag1UpClick);
        GameObject.Find("Tag1Down").GetComponent<Button>().onClick.AddListener(Tag1DownClick);
        GameObject.Find("Tag2Up").GetComponent<Button>().onClick.AddListener(Tag2UpClick);
        GameObject.Find("Tag2Down").GetComponent<Button>().onClick.AddListener(Tag2DownClick);
        GameObject.Find("Tag3Up").GetComponent<Button>().onClick.AddListener(Tag3UpClick);
        GameObject.Find("Tag3Down").GetComponent<Button>().onClick.AddListener(Tag3DownClick);
        GameObject.Find("Continue").GetComponent<Button>().onClick.AddListener(ContinueClick);
        GameObject.Find("PlayOffline").GetComponent<Button>().onClick.AddListener(OfflineClick);
        GameObject.Find("Retry").GetComponent<Button>().onClick.AddListener(RetryClick);
        GameObject.Find("PlayOfflineTag").GetComponent<Button>().onClick.AddListener(OfflineTagClick);

        tagChange.SetActive(false);
        offline.SetActive(false);
    }

    /// <summary>
    /// Uses Fizzyo Load to attempt to load in user data from the Fizzyo API
    /// If loading fails users are given the chance to retry or play offline
    /// If loading is successful but the user has not yet chosen a tag for the leaderboards, they are given the option to add a tag, or play offline
    /// If loading is successful and the user has already inputted a tag, the user is either sent to the main menu or calibration screen, based on whether thay have already calibrated
    /// </summary>
    private IEnumerator LoadUserData()
    {
        loadingDataText.text = "Loading User Data...";

        offline.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        bool dataLoaded = false;

        if (!dataLoaded)
        {

            loadingDataText.text = "Log In Failed / Playing Offline (Achievements, High Scores, Calibration Data and Session Data Will Not Be Uploaded)";
            offline.SetActive(true);

        }
        else
        {

            loadingDataText.text = "User Data Loaded";
            loadingDataText.color = Color.green;

            if (PlayerPrefs.GetInt("tagDone") == 0)
            {
                loadingData.SetActive(false);
                tagChange.SetActive(true);
            }
            else
            {

                StartCoroutine("EndLoad");
            }
        }
    }

    /// <summary>
    /// Loads calibration or menu screens based on whetherthe user has completed calibration
    /// </summary>
    private IEnumerator EndLoad()
    {

        yield return new WaitForSeconds(2);

        if (PlayerPrefs.GetInt("calDone") == 1)
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            SceneManager.LoadScene("Calibration");
        }

    }

    // Tag methods used to change the tag on screen with arrow buttons
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

    /// <summary>
    /// Forms a tag based on the tag chosen on screen
    /// Attempts to upload tag to the database
    /// If upload fails an error is displayed
    /// If upload is successful the user is taken to the appropriate scene
    /// </summary>
    void ContinueClick()
    {
        string tagUpload = "";// Upload.UserTag(fullTag);

        if (tagUpload != "Tag Upload Complete")
        {
            tagError.GetComponent<Text>().text = tagUpload;
            tagError.SetActive(true);
        }
        else
        {
            tagError.SetActive(false);
            loadingData.SetActive(true);
            tagChange.SetActive(false);

            StartCoroutine("EndLoad");
        }
    }

    /// <summary>
    /// If the user is at the stage of inputting a tag, this method is used to select to play offline
    /// Uses Load.PlayOffline() to set PlayerPrefs correctly for offline play
    /// </summary>
    void OfflineTagClick()
    {
        tagChange.SetActive(false);
        loadingData.SetActive(true);
        //Load.PlayOffline();

        loadingDataText.text = "Log In Failed / Playing Offline (Achievements, High Scores, Calibration Data and Session Data Will Not Be Uploaded)";
        loadingDataText.color = Color.white;

        StartCoroutine("EndLoad");
    }

    /// <summary>
    /// If the user cannot initially load data, this method is used to select to play offline
    /// Uses Load.PlayOffline() to set PlayerPrefs correctly for offline play
    /// </summary>
    void OfflineClick()
    {
        offline.SetActive(false);
        //Load.PlayOffline();
        StartCoroutine("EndLoad");
    }

    /// <summary>
    /// If the user cannot initially load data, this method is used to select to retry the load
    /// </summary>
    void RetryClick()
    {
        offline.SetActive(false);
        StartCoroutine("LoadUserData");
    }
}