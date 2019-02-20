﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Fizzyo;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour {

    //Speed to move character at
    public float speed = 0.04f;
    public float flyHeight = 3.0f;
    public string achievement;
    public int score;
    BreathRecogniser br = new BreathRecogniser();
    private int totalScore;

    // Use this for initialization
    void Start () {
        br.BreathStarted += Br_BreathStarted;
        br.BreathComplete += Br_BreathComplete;
    }

    private void Br_BreathStarted(object sender)
    {
        br.MaxBreathLength = FizzyoFramework.Instance.Device.maxBreathCalibrated;
        br.MaxPressure = FizzyoFramework.Instance.Device.maxPressureCalibrated;
    }

    private void Br_BreathComplete(object sender, ExhalationCompleteEventArgs e)
    {
        Debug.LogFormat("Breath Complete.\n Results: Quality [{0}] : Percentage [{1}] : Breath Full [{2}] : Breath Count [{3}] ", e.BreathQuality, e.BreathPercentage, e.IsBreathFull, e.BreathCount);
    }

    // Update is called once per frame
    void Update () {
        //move the player forward
        float x = transform.position.x + speed;
        //set height of the player using the player breath intensity
        float y = FizzyoFramework.Instance.Device.Pressure() * flyHeight;
        //Device.Pressure() can return negative numbers if the player is breathing in. Clamp the player height to be above 0
        y = Mathf.Max(y, 0);

        br.AddSample(Time.deltaTime, FizzyoFramework.Instance.Device.Pressure());

        transform.position = new Vector3(x,y, 0);

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
}