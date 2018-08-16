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
        public double startTime;
        public double endTime;

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

        public void Start()
        {
            //Set start time
            ///
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            this.startTime = (double)t.TotalSeconds * 1000;
            Debug.Log("the time is: " + startTime);
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
            TimeSpan tspan = DateTime.UtcNow - new DateTime(1970, 1, 1);
            this.endTime = ((double)tspan.TotalSeconds) * 1000;

            Debug.Log("Good breath count = " + goodBreathCount);
            Debug.Log("Bearth count = " + breathCount);
            Debug.Log("Bad breath count = " + badBreathCount);
            Debug.Log("Highest score =  " + _score);
            Debug.Log("Time in Unix epoch: " + endTime);
            Debug.Log("above is end, here is start: " + startTime);

        }
        public void ResetData()
        {
            FizzyoFramework.Instance.Recogniser.UnMinimise();
            this.Score = 0;
            this._score = 0;
            this.SetCount = 0;
            Start();
            Debug.Log("inside ResetData()");

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
            Debug.Log("did I get here?");
            ///https://api.fizzyo-ucl.co.uk/api/v1/games/<id>/sessions
            string postAnalytics = FizzyoFramework.Instance.apiPath + "api/v1/games/" + FizzyoFramework.Instance.gameID + "/sessions";
            WWWForm form = new WWWForm();
            if (this.breathCount == 0)
            {
                Debug.Log("breathCount is 0 so don't bother uploading");
                return FizzyoRequestReturnType.FAILED_TO_CONNECT; //no breaths means they didn't really even play the game
            }
            form.AddField("secret", FizzyoFramework.Instance.gameSecret);
            try
            {
                form.AddField("userId", FizzyoFramework.Instance.User.UserID);
            }
            catch (System.ArgumentNullException e)
            {
                Debug.Log("caught no userID --> offlineplay, storesession");
                StoreOffline();
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }
            form.AddField("setCount", _setCount);
            form.AddField("breathCount", breathCount);
            form.AddField("goodBreathCount", goodBreathCount);
            form.AddField("badBreathCount", badBreathCount);
            form.AddField("score", _score);
            form.AddField("startTime", startTime.ToString());
            form.AddField("endTime", endTime.ToString());
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
                StoreOffline();
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;

            }
            Debug.Log("[FizzyoAnalytics] Posting analytics successful.");
            return FizzyoRequestReturnType.SUCCESS;
        }

       public void StoreOffline()
        {
            Debug.Log("in storeoffline");
            OfflineStorageSessions sessionToStore = new OfflineStorageSessions();
            //make a OfflineStorageSession and pass it into SaveToFile
            try
            {
                sessionToStore.breathCount = FizzyoFramework.Instance.Recogniser.BreathCount;
                sessionToStore.badBreathCount = FizzyoFramework.Instance.Recogniser.BadBreaths;
                sessionToStore.goodBreathCount = FizzyoFramework.Instance.Recogniser.GoodBreaths;
                TimeSpan tspan = DateTime.UtcNow - new DateTime(1970, 1, 1);
                sessionToStore.endTime = ((double)tspan.TotalSeconds) * 1000;
                sessionToStore.startTime = this.startTime;
                sessionToStore.score = this._score;
                sessionToStore.setCount = this._setCount;
            } catch
            {
                //error getting variables, dont save
                return;
            }
            SaveToFile(sessionToStore);

        }

        public void SaveToFile(OfflineStorageSessions sessionToStore)
        {
            AllStorageSessions loadedSessions;
            string getSessions = PlayerPrefs.GetString("offlineSessions", "");
            //if null store make new and store
            if (getSessions.Equals(""))
            {
                Debug.Log("get sessions is null (good for first one)");
                loadedSessions = new AllStorageSessions();
                loadedSessions.allSessions.Add(sessionToStore); //now one element in list
                string jsonSessions = JsonUtility.ToJson(loadedSessions); //this MAY need to be redone for AllStorageSessions?
                PlayerPrefs.SetString("offlineSessions", jsonSessions);

            } else {
                //if not null, load into List, add to list, then store
                Debug.Log("get sessions is not null - good for second");
                loadedSessions = JsonUtility.FromJson<AllStorageSessions>(getSessions);
                loadedSessions.allSessions.Add(sessionToStore);
                string jsonSessions = JsonUtility.ToJson(loadedSessions);
                PlayerPrefs.SetString("offlineSessions", jsonSessions);
                Debug.Log("the json: " + jsonSessions);
            }
        }

        [Serializable]
        public class OfflineStorageSessions {
            public int breathCount;
            public int goodBreathCount;
            public int badBreathCount;
            public int setCount;
            public int score;
            public double startTime;
            public double endTime;
        }

        [Serializable]
        public class AllStorageSessions
        {
            public List<OfflineStorageSessions> allSessions = new List<OfflineStorageSessions>();
        }

        public void UploadCache()
        {
            //load into list --> run through list trying to upload
            Debug.Log("in UploadCache");
            AllStorageSessions loadedSessions = new AllStorageSessions();
            string jsonSession = PlayerPrefs.GetString("offlineSessions", "");
            if(jsonSession.Equals(""))
            {
                return; //nothing to upload
            } else
            {
                loadedSessions = JsonUtility.FromJson<AllStorageSessions>(jsonSession);
                //now run through and upload
                Debug.Log("trying to upload");
                for(int i = loadedSessions.allSessions.Count-1; i >= 0; i--)
                {
                    OfflineStorageSessions uploadSession = loadedSessions.allSessions[i];
                    FizzyoRequestReturnType uploadStatus = UploadSession(uploadSession);
                    if (uploadStatus == FizzyoRequestReturnType.SUCCESS)
                    {
                        //remove from queue -- maybe unnecessary
                        loadedSessions.allSessions.RemoveAt(i);
                    } else
                    {
                        return;
                    }
                  
                }
                //if we haven't returned, then everthing has been uploaded by now
                PlayerPrefs.SetString("offlineSessions", ""); //so reset queue
            } Debug.Log("player prefs should now be null " + PlayerPrefs.GetString("offlineSessions", "") + "?");
        }

        public FizzyoRequestReturnType UploadSession(OfflineStorageSessions session)
        {
            ///https://api.fizzyo-ucl.co.uk/api/v1/games/<id>/sessions
            string postAnalytics = FizzyoFramework.Instance.apiPath + "api/v1/games/" + FizzyoFramework.Instance.gameID + "/sessions";
            WWWForm form = new WWWForm();
            form.AddField("secret", FizzyoFramework.Instance.gameSecret);
            form.AddField("userId", FizzyoFramework.Instance.User.UserID);
            form.AddField("setCount", session.setCount);
            form.AddField("breathCount", session.breathCount);
            form.AddField("goodBreathCount", session.goodBreathCount);
            form.AddField("badBreathCount", session.badBreathCount);
            form.AddField("score", session.score);
            form.AddField("startTime", session.startTime.ToString());
            form.AddField("endTime", session.endTime.ToString());
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
                Debug.Log("[FizzyoAnalytics] Posting offline analytics failed. ");
                StoreOffline();
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;

            }
            Debug.Log("[FizzyoAnalytics] Posting offline analytics successful.");
            return FizzyoRequestReturnType.SUCCESS;
        }
    }
}


