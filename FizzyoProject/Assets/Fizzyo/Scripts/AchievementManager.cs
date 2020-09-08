// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Fizzyo;

/// <summary>
/// Class which manages the Achievements scene
/// </summary>
public class AchievementManager : MonoBehaviour
{
    // Achievement prefab used to hold information on each achievement
    public GameObject achPre;

    // Sprite arrays for holding the sprites used for displaying locked and unlocked achievements
    public Sprite[] achPics;
    public Sprite[] achBack;

    // json achievement data path
    string path;

    // One string containing all data from json achievement data path
    string achJSONData;

    // Holds the total point value of all unlocked achievements
    int totalPoints = 0;

    // Active category button
    private AchievementButton active;

    // Changed when a different category is selected
    public ScrollRect scroll;

    // Holds the name of each category
    List<string> catagories = new List<string>();

    // category Button Prefab
    public GameObject catPre;

    // category List Prefab
    public GameObject catList;

    /// <summary>
    /// Pulls the achievements from player preferences, creates each one in the scene and adds the total points for display
    /// </summary>
    void Start()
    {
        FizzyoFramework.Instance.Load();

        if(FizzyoFramework.Instance.Achievements == null || FizzyoFramework.Instance.Achievements.allAchievements == null)
        {
            return;
        }

        totalPoints = 0;

        GameObject text = GameObject.Find("Total");
        Text total = text.GetComponent<Text>();

        

        for (int i = 0; i < FizzyoFramework.Instance.Achievements.allAchievements.Length; i++)
        {
            AchievementData achievment = FizzyoFramework.Instance.Achievements.allAchievements[i];

            if (!(catagories.Contains(achievment.category)))
            {
                Createcategory(achievment.category);
            }

            if (achievment.unlock == 1)
            {
                totalPoints += achievment.points;
            }

            CreateAch(achievment.category, achievment.title, achievment.description, achievment.points, achievment.unlock, achievment.unlockProgress, achievment.unlockRequirement);            

        }

        foreach (GameObject achList in GameObject.FindGameObjectsWithTag("AchList"))
        {
            achList.SetActive(false);
        }

        if(active != null)
        {
            active.Click();
        }

        total.text = "Total Achievement Points: " + totalPoints;
    }

    /// <summary>
    /// Instantiates a game object for an achievement and assigns it the correct values
    /// </summary>
    /// /// <param name="parentCat"> 
    /// String that contains the category for that achievement
    /// </param>  
    /// <param name="title"> 
    /// String that contains the title for that achievement
    /// </param>  
    /// <param name="desc"> 
    /// String that contains the description for that achievement
    /// </param>  
    /// <param name="points"> 
    /// Integer that contains the points for the current achievement
    /// </param>  
    /// <param name="unlock"> 
    /// Integer that contains a 0 or 1 based on whether the achievement id locked or unlocked
    /// </param>  
    /// <param name="unlockProgress"> 
    /// Integer that contains the achievements unlock progress
    /// </param>  
    /// <param name="unlockRequirement"> 
    /// Integer that contains the achievements unlock requirement
    /// </param>  
    public void CreateAch(string parentCat, string title, string desc, int points, int unlock, int unlockProgress, int unlockRequirement)
    {
        GameObject ach = (GameObject)Instantiate(achPre);

        SetInfoAch(parentCat, ach, title, desc, points, unlock, unlockProgress, unlockRequirement);
    }

    /// <summary>
    /// Sets the parent, scale and all relevant information to be shown in an achievement
    /// </summary>
    /// /// <param name="parentCat"> 
    /// String that contains the category for that achievement
    /// </param>  
    /// <param name="title"> 
    /// String that contains the title for that achievement
    /// </param>  
    /// <param name="desc"> 
    /// String that contains the description for that achievement
    /// </param>  
    /// <param name="points"> 
    /// Integer that contains the points for the current achievement
    /// </param>  
    /// <param name="unlock"> 
    /// Integer that contains a 0 or 1 based on whether the achievement id locked or unlocked
    /// </param>  
    /// <param name="unlockProgress"> 
    /// Integer that contains the achievements unlock progress
    /// </param>  
    /// <param name="unlockRequirement"> 
    /// Integer that contains the achievements unlock requirement
    /// </param>  
    /// <param name="ach"> 
    /// GameObject that contains the instantiated prefab for this achievement
    /// </param>  
    public void SetInfoAch(string parentCat, GameObject ach, string title, string desc, int points, int unlock, int unlockProgress, int unlockRequirement)
    {
        ach.transform.SetParent(GameObject.Find(parentCat).transform);
        ach.transform.localScale = new Vector3((float)Screen.width/1280, (float)Screen.width / 1280, 1);
        ach.transform.GetChild(0).GetComponent<Text>().text = title;
        ach.transform.GetChild(1).GetComponent<Text>().text = desc;
        ach.transform.GetChild(2).GetComponent<Text>().text = points.ToString();
        ach.transform.GetChild(3).GetComponent<Image>().sprite = achPics[unlock];

        if (unlock != 0)
            ach.transform.GetChild(4).GetComponent<Text>().text = "";
        else
            ach.transform.GetChild(4).GetComponent<Text>().text = unlockProgress.ToString() + " / " + unlockRequirement.ToString();

        ach.transform.GetComponent<Image>().sprite = achBack[unlock];
    }

    /// <summary>
    /// Instantiates a new category button and category area for achievement display
    /// </summary>
    /// /// <param name="category"> 
    /// String that contains the category to be displayed
    /// </param>
    public void Createcategory(string category)
    {
        GameObject buttonPre = (GameObject)Instantiate(catPre);
        buttonPre.name = "Cat" + category;
        buttonPre.transform.SetParent(GameObject.Find("CatList").transform);
        buttonPre.transform.GetChild(0).GetComponent<Text>().text = category;
        buttonPre.transform.GetComponent<Button>().onClick.AddListener(() => Changecategory(buttonPre));
        buttonPre.transform.localScale = new Vector3(1, 1, 1);

        GameObject listPre = (GameObject)Instantiate(catList);
        listPre.name = category;
        listPre.transform.SetParent(GameObject.Find("MainMask").transform);
        listPre.transform.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        listPre.transform.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

        buttonPre.GetComponent<AchievementButton>().achList = listPre;

        if (catagories.Count == 0)
        {
            active = GameObject.Find("Cat" + category).GetComponent<AchievementButton>();
            scroll.content = active.achList.GetComponent<RectTransform>();
        }

        catagories.Add(category);
    }

    /// <summary>
    /// Instantiates a new category button and category area for achievement display
    /// </summary>
    /// /// <param name="button"> 
    /// GameObject which holds the button which is selected
    /// </param>
    public void Changecategory(GameObject button)
    {
        AchievementButton achButton = button.GetComponent<AchievementButton>();

        // Changes scroll content on the main mask. Allows for scrolling of all categories
        scroll.content = achButton.achList.GetComponent<RectTransform>();

        // Sets the active to false and the new button to true
        achButton.Click();
        active.Click();
        active = achButton;
    }
}