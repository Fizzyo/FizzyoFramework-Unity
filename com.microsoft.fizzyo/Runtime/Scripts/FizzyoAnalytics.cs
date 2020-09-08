// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
        public int BreathCount;
        public int GoodBreathCount;
        public int BadBreathCount;
        public double StartTime;
        public double EndTime;
        public System.Guid SessionId;

        private int _score;

        /// <summary>
        /// Add this to your game to update the score to send in the session. 
        /// </summary>
        public int Score
        {
            get { return _score; }
            set
            {
                if (value >= _score)
                {
                    _score = value;
                }
            }
        }

        private int _setCount;

        /// <summary>
        /// Add this to your game to update the breathing set-count to send in the session. 
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
        public FizzyoAnalytics()
        {
            //Set start time
            StartTime = (long)(((TimeSpan)(DateTime.UtcNow - new DateTime(1970, 1, 1))).TotalSeconds * 1000);
            SessionId = System.Guid.NewGuid();

        }

        public void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                StartTime = (long)(((TimeSpan)(DateTime.UtcNow - new DateTime(1970, 1, 1))).TotalSeconds * 1000);
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
            GoodBreathCount = FizzyoFramework.Instance.Recogniser.GoodBreaths;
            BreathCount = FizzyoFramework.Instance.Recogniser.BreathCount;
            BadBreathCount = FizzyoFramework.Instance.Recogniser.BadBreaths;
            EndTime = (long)(((TimeSpan)(DateTime.UtcNow - new DateTime(1970, 1, 1))).TotalSeconds * 1000);
            Debug.Log("Good breath count = " + GoodBreathCount);
            Debug.Log("Breath count = " + BreathCount);
            Debug.Log("Bad breath count = " + BadBreathCount);
            Debug.Log("Highest score =  " + _score);
            Debug.Log("Time in Unix epoch: " + EndTime);
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
            if (FizzyoFramework.Instance != null && FizzyoFramework.Instance.FizzyoConfigurationProfile != null)
            {
                if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
                {
                    return FizzyoRequestReturnType.FAILED_TO_CONNECT;
                }

                ///https://api.fizzyo-ucl.co.uk/api/v1/games/<id>/sessions

                Dictionary<string, string> formData = new Dictionary<string, string>();
                formData.Add("secret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
                formData.Add("userId", FizzyoFramework.Instance.User.UserID);
                formData.Add("sessionId", SessionId.ToString());
                formData.Add("setCount", _setCount.ToString());
                formData.Add("breathCount", BreathCount.ToString());
                formData.Add("goodBreathCount", GoodBreathCount.ToString());
                formData.Add("badBreathCount", BadBreathCount.ToString());
                formData.Add("score", _score.ToString());
                formData.Add("startTime", StartTime.ToString());
                formData.Add("endTime", EndTime.ToString());

                var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/sessions", formData);
                webRequest.SendWebRequest();

                while (!webRequest.isDone) { };

                if (webRequest.error != null)
                {
                    Debug.Log("[FizzyoAnalytics] Posting analytics failed. ");
                    return FizzyoRequestReturnType.FAILED_TO_CONNECT;

                }
                Debug.Log("[FizzyoAnalytics] Posting analytics successful.");
                return FizzyoRequestReturnType.SUCCESS;
            }

            return FizzyoRequestReturnType.FAILED_TO_CONNECT;
        }
    }
}