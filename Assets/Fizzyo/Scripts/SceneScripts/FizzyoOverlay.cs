// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Fizzyo;

public class FizzyoOverlay : MonoBehaviour
{
    // Session Data Saved popup that appears on-screen
    public GameObject sessionPrefab;

    // Achievment popup that appears on-screen
    public GameObject achievementPrefab;

    // High Score popup that appears on-screen
    public GameObject scorePrefab;

    // Uploading popup that appears on-screen
    public GameObject uploadingPrefab;

    /// <summary>
    /// Used to show in the scene that a data is being uploaded
    /// </summary>
    public void ShowLoading()
    {
        if (PlayerPrefs.GetInt("online") == 0)
        {
            return;
        }

        GameObject uploadingPopup = Instantiate(uploadingPrefab);
        uploadingPopup.transform.SetParent(GameObject.Find("LeadUnlock").transform);
        uploadingPopup.transform.localScale = new Vector3(1, 1, 1);
        StartCoroutine(FadePopup(uploadingPopup, false));
    }

    /// <summary>
    /// Used to show in the scene that a session has been uploaded
    /// </summary>
    /// <remarks>
    /// Using this session system:
    /// Use requires that the FizzyoOverlay Object be placed in a scene and the popup prefab be added in the inspector
    /// Then, within the game code the following can be used to upload a session:
    /// private FizzyoOverlay FizzyoOverlay; - Used as a variable in the class
    /// FizzyoOverlay = GameObject.Find("FizzyoOverlay").GetComponent<FizzyoOverlay>(); - Used in the Start()/Awake() functions in a class
    /// FizzyoOverlay.ShowSession(currentSession.SessionUpload(TotalGoodBreathCount(), TotalBadBreathCount(), TotalCoins())); - Used in the part of the script that denotes where a session is finished
    /// </remarks>
    /// <param name="status"> 
    /// Integer holding the amount of good breaths completed in this session
    /// </param> 
    public void ShowSession(string status)
    {
        if (PlayerPrefs.GetInt("online") == 0)
        {
            return;
        }

        GameObject sessionPopup = Instantiate(sessionPrefab);
        sessionPopup.transform.SetParent(GameObject.Find("LeadUnlock").transform);
        sessionPopup.transform.localScale = new Vector3(1, 1, 1);
        sessionPopup.transform.GetChild(0).GetComponent<Text>().text = status;
        StartCoroutine(FadePopup(sessionPopup, false));
    }

    /// <summary>
    /// Unlocks / adds progress to an achievement. Data is saved in player preferences and uploaded when a session is uploaded.
    /// When an achievement is unlocked it shows on screen
    /// </summary>
    /// <remarks>
    /// Using this achievement system:
    /// Use requires that the FizzyoOverlay Object be placed in a scene and the pop-up prefab be added in the inspector
    /// Then, within the game code the following can be used to unlock / add progress to an achievement:
    /// private FizzyoOverlay FizzyoOverlay; - Used as a variable in the class
    /// FizzyoOverlay = GameObject.Find("FizzyoOverlay").GetComponent<FizzyoOverlay>(); - Used in the Start()/Awake() functions in a class
    /// FizzyoOverlay.UnlockAchievement("First Good Breath", 1) - Used in the part of the script that denotes where an achievement is gained
    /// </remarks>
    /// <param name="title"> 
    /// String title of achievement as it is set in the StreamingAssets/Achievements.Json File
    /// </param> 
    /// <param name="progress"> 
    /// Integer amount of progress that has been made on this achievement
    /// </param> 
    public void UnlockAchievement(string title, int progress)
    {
        if (PlayerPrefs.GetInt("online") == 0)
        {
            return;
        }

        string achievements = PlayerPrefs.GetString("achievements");
        AllAchievementData allData = JsonUtility.FromJson<AllAchievementData>(achievements);

        string currentTitle = title;
        bool completed = false;
        int currentAchievement;
        bool found;

        // Loops through chained achievements
        while (!completed)
        {
            found = false;

            for (int i = 0; i < allData.achievements.Length; i++)
            {
                if (allData.achievements[i].title == currentTitle)
                {
                    found = true;
                    currentAchievement = i;

                    if (allData.achievements[i].unlock == 0)
                    {
                        // Set that an achievement variables has been changed and that this achievement will have to be uploaded on session end
                        allData.achievements[i].unlockProgress += progress;

                        bool chaining = true;
                        bool dependencyFound;

                        // Loop to unlock children if the progress is high enough to unlock more than one
                        while (chaining)
                        {

                            dependencyFound = false;

                            if (allData.achievements[currentAchievement].unlockProgress >= allData.achievements[currentAchievement].unlockRequirement)
                            {


                                ShowAchievement("AchUnlock", allData.achievements[currentAchievement].title, allData.achievements[currentAchievement].description, allData.achievements[currentAchievement].points, allData.achievements[currentAchievement].unlock);

                                allData.achievements[currentAchievement].unlock = 1;

                                string achievementsToUpload = PlayerPrefs.GetString("achievementsToUpload");

                                // Add achievement into set that need to be uploaded at the end of a session
                                if (!achievementsToUpload.Contains(allData.achievements[i].id))
                                {
                                    achievementsToUpload += allData.achievements[i].id + ",";
                                    PlayerPrefs.SetString("achievementsToUpload", achievementsToUpload);
                                }

                                // Pass on progress to child
                                if (allData.achievements[currentAchievement].dependency != "" && allData.achievements[currentAchievement].dependency != null)
                                {
                                    for (int j = 0; j < allData.achievements.Length; j++)
                                    {
                                        if (allData.achievements[j].title == allData.achievements[currentAchievement].dependency)
                                        {
                                            dependencyFound = true;

                                            allData.achievements[j].unlockProgress = allData.achievements[currentAchievement].unlockProgress;

                                            if (allData.achievements[j].unlockProgress >= allData.achievements[j].unlockRequirement)
                                            {
                                                currentAchievement = j;
                                            }
                                            else
                                            {
                                                chaining = false;
                                            }
                                            break;
                                        }
                                    }

                                    if (!dependencyFound)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                string achievementsToProgress = PlayerPrefs.GetString("achievementsToProgress");

                                if (!achievementsToProgress.Contains(allData.achievements[i].id))
                                {
                                    achievementsToProgress += allData.achievements[i].id + ",";
                                    PlayerPrefs.SetString("achievementsToProgress", achievementsToProgress);
                                }

                                break;
                            }
                        }

                        string newAllData = JsonUtility.ToJson(allData);
                        PlayerPrefs.SetString("achievements", newAllData);
                        completed = true;
                        break;
                    }
                    else
                    {
                        if (allData.achievements[currentAchievement].dependency != "" && allData.achievements[currentAchievement].dependency != null)
                        {
                            currentTitle = allData.achievements[currentAchievement].dependency;
                            break;
                        }
                        else
                        {
                            completed = true;
                            break;
                        }
                    }
                }
            }

            if (!found)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Used to show in the scene that a achievement has been unlocked
    /// </summary>
    /// <param name="parentCat"> 
    /// String holding the achievements category
    /// </param> 
    /// <param name="title"> 
    /// String holding the achievements title
    /// </param> 
    /// <param name="desc"> 
    /// String holding the achievements description
    /// </param> 
    /// <param name="points"> 
    /// Integer holding the achievements points
    /// </param> 
    /// <param name="unlock"> 
    /// Integer holding the achievements unlock value
    /// </param> 
    public void ShowAchievement(string parentCat, string title, string desc, int points, int unlock)
    {
        GameObject ach = Instantiate(achievementPrefab);
        ach.transform.SetParent(GameObject.Find("AchUnlock").transform);
        ach.transform.localScale = new Vector3(1, 1, 1);
        ach.transform.GetChild(0).GetComponent<Text>().text = "Achievement Unlocked!";
        ach.transform.GetChild(1).GetComponent<Text>().text = title;
        ach.transform.GetChild(2).GetComponent<Text>().text = points.ToString();
        StartCoroutine(FadePopup(ach, true));
    }

    /// <summary>
    /// Uploads a score and displays a message in the scene
    /// </summary>
    /// <remarks>
    /// Using this achievement system:
    /// Use requires that the FizzyoOverlay Object be placed in a scene and the pop-up prefab be added in the inspector
    /// Then, within the game code the following can be used to attempt to upload a score:
    /// private FizzyoOverlay FizzyoOverlay; - Used as a variable in the class
    /// FizzyoOverlay = GameObject.Find("FizzyoOverlay").GetComponent<FizzyoOverlay>(); - Used in the Start()/Awake() functions in a class
    /// FizzyoOverlay.LeaderBoard(score); - Used in the part of the script that denotes where a score is uploaded
    /// </remarks>
    /// <param name="score"> 
    /// Integer holding the score that is to be uploaded
    /// </param> 
    public object LeaderBoard(int score)
    {
        Object complete = new Object();

        if (PlayerPrefs.GetInt("online") == 0)
        {
            return complete;
        }

        if (score != 0)
        {

            string status = "";// Upload.Score(score);

            GameObject scorePopup = Instantiate(scorePrefab);
            scorePopup.transform.SetParent(GameObject.Find("LeadUnlock").transform);
            scorePopup.transform.localScale = new Vector3(1, 1, 1);
            scorePopup.transform.GetChild(0).GetComponent<Text>().text = status;
            StartCoroutine(FadePopup(scorePopup, false));

            return complete;

        }

        return complete;
    }

    /// <summary>
    /// Moves the achievement, score and session messages off screen and destroys them
    /// </summary>
    /// <param name="popup"> 
    /// Game Object that is to be moved off screen
    /// </param> 
    /// <param name="direction"> 
    /// Boolean which if false will move the message to the left and it true will move the message to the right
    /// </param> 
    private IEnumerator FadePopup(GameObject popup, bool direction)
    {
        yield return new WaitForSeconds(5);

        Vector3 startPos = popup.transform.position;
        Vector3 endPos;
        if (!direction)
        {
            endPos = popup.transform.position - new Vector3(600, 0, 0);
        }
        else
        {
            endPos = popup.transform.position + new Vector3(600, 0, 0);
        }
        float counter = 0;
        float duration = 3;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            popup.transform.position = Vector3.Lerp(startPos, endPos, counter / duration);
            yield return null;
        }

        Destroy(popup);
    }
}