using System;
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
          startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
 
            }
            else
            {
 
            }
        }
        private void OnApplicationQuit()
        {
            CreateSession();
            PostAnalytics();
        }


        ///<summary>
        ///Sets all the fields of the session before upload. 
        ///</summary>
        
        void CreateSession()
        {
            //All of the stats comes from the Breath Recognizer
            breathCount = FizzyoFramework.Instance.BreathRecognizer.BreathCount.get();
            goodBreaths = FizzyoFramework.Instance.BreathRecognizer.GoodBreaths.get();
            badBreaths = FizzyoFramework.Instance.BreathRecognizer.BadBreaths.get();
            score = 0;
            endTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

       ///<summary>
       ///Once the game shuts down, information from the session is sent to the server. 
       ///
       ///It will send: 
       /// 1. Amount of sets in this session
       /// 2. Amounts of breaths in this session 
       /// 3. Amount of good breaths in this session
       /// 4. Amount of bad breaths in this session
       /// 5. User's game score for this session
       /// 6. Start time of the session
       /// 7. End time of the session. 
       /// Note: Time represented as Unix Epoch time.
       /// </summary>
       /// <param name="score">Player score to post in this session</param>
       /// <param name="setCount">Number of sets during this session</param>
        public FizzyoRequestReturnType PostAnalytics(int score, int setCount)
        {
            ///https://api.fizzyo-ucl.co.uk/api/v1/games/<id>/sessions
            string postAnalytics = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + PlayerPrefs.GetString("gameId") + "/sessions";

            WWWForm form = new WWWForm();
            form.addField("secret", PlayerPrefs.GetString("gameSecret"));
            form.AddField("userId", PlayerPrefs.GetString("userId"));
            form.AddField("setCount", setCount);
            form.AddField("breathCount", breathCount);
            form.AddField("goodBreathCount", goodBreathCount);
            form.AddField("badBreathCount", badBreathCount);
            form.AddField("score", score);
            form.AddField("startTime", startTime);
            form.AddField("endTime", endTime);
            Dictionary<string, string> headers = form.headers;
            headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

            byte[] rawData = form.data;
            
            WWW sendPostAnalytics = new WWW(postAnalytics, rawData, headers);
            
            while (!sendPostUnlock.isDone) { };

            if (sendPostUnlock.error != null)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
                
            }
            return FizzyoRequestReturnType.SUCCESS;
        }

            

            
        }



    }

