// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour {

    public Button play;
    public Button cal;
    public Button ach;
    public Button lead;
    public Button exit;

    void Start()
    {
        Button playBtn = play.GetComponent<Button>();
        playBtn.onClick.AddListener(PlayClick);

        Button calBtn = cal.GetComponent<Button>();
        calBtn.onClick.AddListener(CalClick);

        Button achBtn = ach.GetComponent<Button>();
        achBtn.onClick.AddListener(AchClick);

        Button leadBtn = lead.GetComponent<Button>();
        leadBtn.onClick.AddListener(LeadClick);

        Button exitBtn = exit.GetComponent<Button>();
        exitBtn.onClick.AddListener(ExitClick);
    }

    void PlayClick()
    {
        SceneManager.LoadScene("ExampleLevel");
    }

    void CalClick()
    {
        SceneManager.LoadScene("Calibration");
    }

    void AchClick()
    {
        SceneManager.LoadScene("Achievements");
    }

    void LeadClick()
    {
        SceneManager.LoadScene("Leaderboard");
    }

    void ExitClick()
    {
        Application.Quit();
    }
}