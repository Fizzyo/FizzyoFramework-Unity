// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    // Achievement prefab used to hold information on each achievement
    public GameObject leadPre;

    // json achievement data path
    string path;

    // One string containing all data from json achievement data path
    string leadJSONData;

    // Holds leaderboard position
    string position;

    // One string containing all data from json users data path
    string achJSONDataUsers;

    // json users data path
    string pathUsers;

    /// <summary>
    /// Loads the high scores using the Fizzyo API and displays them. If playing offline a message is displayed and the highscores are not
    /// </summary>
    void Start()
    {

        string highscores = "";// Load.GetHighscores();

        if (highscores == "Highscore Load Failed")
        {

            CreateLead("", "Leaderboard Not Loaded While Playing Offline", 0);

        } else
        {

            //AllHighscoreData allDataLead = JsonUtility.FromJson<AllHighscoreData>(highscores);
            /*
            for (int i = 0; i < allDataLead.highscores.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        position = "1st";
                        break;
                    case 1:
                        position = "2nd";
                        break;
                    case 2:
                        position = "3rd";
                        break;
                    default:
                        position = (i + 1) + "th";
                        break;
                }
                
               // CreateLead(position, allDataLead.highscores[i].tag, allDataLead.highscores[i].score);
            }
            */
        }
    }

    /// <summary>
    /// Instantiates a game object for a high score and assigns it the correct values
    /// </summary>
    /// /// <param name="position"> 
    /// String that contains the position for that highscore
    /// </param>  
    /// <param name="name"> 
    /// String that contains the tag associated with that highscore
    /// </param>  
    /// <param name="score"> 
    /// Integer that contains the score associated with  that highscore
    /// </param>  
    public void CreateLead(string position, string name, int score)
    {
        GameObject lead = (GameObject)Instantiate(leadPre);

        SetInfoLead(lead, position, name, score);
    }

    /// <summary>
    /// Sets all relevant information to be shown in a highscore
    /// </summary>
    /// /// <param name="position"> 
    /// String that contains the position for that highscore
    /// </param>  
    /// <param name="name"> 
    /// String that contains the tag associated with that highscore
    /// </param>  
    /// <param name="score"> 
    /// Integer that contains the score associated with that highscore
    /// </param>  
    /// /// <param name="lead"> 
    /// GameObject that contains the instantiated prefab for that highscore
    /// </param>  
    public void SetInfoLead(GameObject lead, string position, string name, int score)
    {
        lead.transform.SetParent(GameObject.Find("LeaderList").transform);
        lead.transform.localScale = new Vector3(1, 1, 1);
        lead.transform.GetChild(0).GetComponent<Text>().text = position;
        lead.transform.GetChild(1).GetComponent<Text>().text = name;
        if (score == 0)
        {
            lead.transform.GetChild(2).GetComponent<Text>().text = "";
        } else
        {
            lead.transform.GetChild(2).GetComponent<Text>().text = score.ToString();
        }        
    }
}