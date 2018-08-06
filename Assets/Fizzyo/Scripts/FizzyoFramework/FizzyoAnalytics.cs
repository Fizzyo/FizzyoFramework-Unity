﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Fizzyo
{
    /// <summary>
    /// This class manages the creation of sessions to send over data once the game is about to shut down. 
    /// It will post: 
    /// 1. Amount of sets in this session
    /// 2. Amounts of breaths in this session 
    /// 3. Amount of good breaths in this session
    /// 4. Amount of bad breaths in this session
    /// 5. User's game score for this session
    /// 6. Start time of the session
    /// 7. End time of the session. 
    /// Note: Time represented as Unix Epoch time.       
    /// </summary> 
    public class FizzyoAnalytics
    {
        
        // Various session parameters
        public int breathCount;
        public int goodBreathCount;
        public int badBreathCount;
        public int startTime;
        public int endTime;

        private int _score;
        /// <summary>
        /// Add this to your game to update the score to send in the session. 
        /// </summary>
        public int Score {
            get { return _score; } 
            set {
                if(value >= _score ){
                    _score = value;                  
                }    
            } 
        }

        private int _setCount;
        /// <summary>
        /// Add this to your game to update the breathing setcount to send in the session. 
        /// </summary>
        public int SetCount { get { return _setCount; } set { _setCount = value; } }

        /// <summary>
        /// Constructor for a session
        /// </summary>
        /// <param name="setCount"> 
        /// Integer holding the amount of sets that are to be completed in this session
        /// </param>  
        /// <param name="breathCount"> 
        /// Integer holding the amount of breaths that are to be completed in each set
        /// </param>  

        private void Start()
        {
          //Set start time
          ///
          System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
          startTime = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        }
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                ResetData();
            }
            else
            {
                PostOnQuit();
            }
        }
		
		///<summary>
		///Add this to the logic which manages quitting the application to Create and Post sessions.
		///</summary>
        public void PostOnQuit()
        {
            Debug.Log("[FizzyoAnalytics] About to quit: creating session to upload.");
            CreateSession();
            Debug.Log("[FizzyoAnalytics] Session creation Finished.");
            Debug.Log("[FizzyoAnalytics] Posting Analytics...");
            PostAnalytics();
        }

        ///<summary>
        ///Sets all the fields of the session before upload. 
        ///</summary>
        
        void CreateSession()
        {
            //All of the stats comes from the Breath Recognizer
            goodBreathCount = FizzyoFramework.Instance.Recogniser.GoodBreaths;
            breathCount = FizzyoFramework.Instance.Recogniser.BreathCount;
            badBreathCount = FizzyoFramework.Instance.Recogniser.BadBreaths;
            endTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Debug.Log("Good breath count = " + goodBreathCount);
            Debug.Log("Bearth count = " + breathCount);
            Debug.Log("Bad breath count = " + badBreathCount);
            Debug.Log("Highest score =  " + _score);
            Debug.Log("Time in Unix epoch: " + endTime);

        }
        public void ResetData()
        {
            FizzyoFramework.Instance.Recogniser.UnMinimise();
            this.Score = 0;
            this.SetCount = 0;
            Start();
            Debug.Log("looks like alt tabbing works eh");

        }

        ///<summary>
        ///Once the game shuts down, information from the session is sent to the server. 
        ///
        ///It will send: 
        /// 1. Amount of sets in this session
        /// 2. Amounts of breaths in this session 
        /// 3. Amount of good breaths in this session
        /// 4. Amount of bad breaths in this session
        /// 5. User-s highest score for this session
        /// 6. Start time of the session
        /// 7. End time of the session. 
        /// Note: Time represented as Unix Epoch time.
        /// </summary>

        public FizzyoRequestReturnType PostAnalytics()
        {
            ///https://api.fizzyo-ucl.co.uk/api/v1/games/<id>/sessions
            string postAnalytics = FizzyoFramework.Instance.apiPath + "api/v1/games/" + FizzyoFramework.Instance.gameID + "/sessions";

            WWWForm form = new WWWForm();
            form.AddField("secret", FizzyoFramework.Instance.gameSecret);
            form.AddField("userId", FizzyoFramework.Instance.User.UserID);
            form.AddField("setCount", _setCount);
            form.AddField("breathCount", breathCount);
            form.AddField("goodBreathCount", goodBreathCount);
            form.AddField("badBreathCount", badBreathCount);
            form.AddField("score", _score);
            form.AddField("startTime", startTime);
            form.AddField("endTime", endTime);
            Dictionary<string, string> headers = form.headers;
            headers["Authorization"] = "Bearer " + FizzyoFramework.Instance.User.AccessToken;
#if UNITY_UWP
            headers.Add("User-Agent", " FizzyoClient " + FizzyoFramework.Instance.ClientVersion);
#endif 
            byte[] rawData = form.data;
            
            WWW sendPostAnalytics = new WWW(postAnalytics, rawData, headers);
            
            while (!sendPostAnalytics.isDone) { };

            if (sendPostAnalytics.error != null)
            {
                Debug.Log("[FizzyoAnalytics] Posting analytics failed. ");
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
                
            }
            Debug.Log("[FizzyoAnalytics] Posting analytics successful.");
            return FizzyoRequestReturnType.SUCCESS;
        }

            

            
        }



    }

