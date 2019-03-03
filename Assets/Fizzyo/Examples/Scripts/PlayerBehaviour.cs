// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Fizzyo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehaviour : MonoBehaviour {

    //Speed to move character at
    public float speed = 0.04f;
    public float flyHeight = 3.0f;
    public string achievement;
    public int score;
    public Text SetText;
    public Text BreathText;
    public Text EndSetText;
    public Text EndSessionText;
    public Text PausedText;
    private int totalScore;
    private bool waitingForInput = true;
    private bool paused = false;

    // Use this for initialization
    void Start ()
    {
        Debug.Log("Player Starting");
        FizzyoFramework.Instance.Recogniser.BreathComplete += Br_BreathComplete;
        FizzyoFramework.Instance.Session.SetComplete += Session_SetComplete;
        FizzyoFramework.Instance.Session.SessionPaused += Session_SessionPaused;
        FizzyoFramework.Instance.Session.SessionResumed += Session_SessionResumed;
    }

    private void Session_SessionResumed(object sender, SessionEventArgs e)
    {
        paused = false;
        waitingForInput = false;
    }

    private void Session_SessionPaused(object sender, SessionEventArgs e)
    {
        paused = true;
        waitingForInput = true;
    }

    private void Session_SetComplete(object sender, SessionEventArgs e)
    {
        waitingForInput = true;
    }

    private void Br_BreathComplete(object sender, ExhalationCompleteEventArgs e)
    {
        Debug.LogFormat("Breath Complete.\n Results: Quality [{0}] : Percentage [{1}] : Breath Full [{2}] : Breath Count [{3}] ", e.BreathQuality, e.BreathPercentage, e.IsBreathFull, e.BreathCount);
    }

    // Update is called once per frame
    void Update () {
        if (!waitingForInput)
        {
            if (!paused)
            {
                PausedText.gameObject.SetActive(false);
            }

            //move the player forward
            float x = transform.position.x + speed;
            //set height of the player using the player breath intensity
            float y = FizzyoFramework.Instance.Device.Pressure() * flyHeight;
            //Device.Pressure() can return negative numbers if the player is breathing in. Clamp the player height to be above 0
            y = Mathf.Max(y, 0);
            transform.position = new Vector3(x, y, 0);
        }
        else
        {
            if (paused)
            {
                PausedText.gameObject.SetActive(true);
            }
            else if (FizzyoFramework.Instance.Session.IsSessionStarted)
            {
                EndSetText.gameObject.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    EndSetText.gameObject.SetActive(false);
                    waitingForInput = false;
                    FizzyoFramework.Instance.Session.StartSet();
                }
            }
            else
            {
                EndSessionText.gameObject.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    EndSessionText.gameObject.SetActive(false);
                    waitingForInput = false;
                    FizzyoFramework.Instance.Session.StartSession(true);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            FizzyoFramework.Instance.Analytics.Score = totalScore;
            FizzyoFramework.Instance.Achievements.PostScore(totalScore);
            totalScore = 0;
            Debug.Log("Level Restarted");
            SceneManager.LoadScene("ExampleLevel");
        }
        //Test giving achievement
        if (Input.GetKeyDown(KeyCode.A))
        {
            string unlockAchievement = string.Empty;
            if (FizzyoFramework.Instance.Achievements.allAchievements != null && FizzyoFramework.Instance.Achievements.allAchievements.Length > 0)
            {
                for (int i = 0; i < FizzyoFramework.Instance.Achievements.allAchievements.Length; i++)
                {
                    if (FizzyoFramework.Instance.Achievements.allAchievements[i].title == achievement)
                    {
                        unlockAchievement = FizzyoFramework.Instance.Achievements.allAchievements[i].id;
                        totalScore += FizzyoFramework.Instance.Achievements.allAchievements[i].points;
                    }
                }
                if (unlockAchievement != string.Empty)
                {
                    FizzyoFramework.Instance.Achievements.UnlockAchievement(unlockAchievement);
                }
            }
        }

        //Test Adding Score
        if (Input.GetKeyDown(KeyCode.S))
        {
            totalScore += score;
        }
    }

    private void LateUpdate()
    {
        SetText.text = FizzyoFramework.Instance.Session.CurrentSetCount.ToString();
        BreathText.text = FizzyoFramework.Instance.Session.CurrentBreathCount.ToString();
    }

    private void OnDestroy()
    {
        if (FizzyoFramework.Instance != null && FizzyoFramework.Instance.Recogniser != null)
        {
            FizzyoFramework.Instance.Recogniser.BreathComplete -= Br_BreathComplete;
        }
    }
}