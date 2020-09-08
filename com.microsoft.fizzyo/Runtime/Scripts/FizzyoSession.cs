// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzyo
{
    /// <summary>
    /// The FizzyoFramework Session manager provides an easy way to manage Sets and Breaths required for a session
    /// It will automatically save the previous settings, should you enable the user to change these values (provided you also update the session manager)
    /// 
    /// Additionally, the framework provides several events surrounding Session / Set starting and stopping, as well as inactive breathing during a session (pausing)
    /// </summary>
    public class FizzyoSession
    {
        #region Properties

        // Save keys
        private string sessionSetCountKey = "SetsCount";
        private string sessionBreathCountKey = "BreathsCount";
        private float lastBreathLength = 0;
        private float lastMaxPressure = 0;
        private int lastQuality = 0;
        private float timeBreathpaused = 0;
        private const float breathingPausedThreshold = 5f;

        private bool isSessionStarted = false;

        /// <summary>
        /// Has a session begun
        /// </summary>
        public bool IsSessionStarted
        {
            get { return isSessionStarted; }
            set { isSessionStarted = value; }
        }

        private bool isSetStarted = false;

        /// <summary>
        /// Is a set currently running
        /// </summary>
        public bool IsSetStarted
        {
            get { return isSetStarted; }
            set { isSetStarted = value; }
        }

        private bool isSessionPaused = false;

        /// <summary>
        /// Has the player had to pause their breathing for any reason (aka, pause game till they can play again)
        /// </summary>
        public bool IsSessionPaused
        {
            get { return isSessionPaused; }
            set { isSessionPaused = value; }
        }

        private int currentSetCount = 0;

        /// <summary>
        /// Which set is currently running
        /// </summary>
        public int CurrentSetCount
        {
            get { return currentSetCount; }
        }

        private int currentBreathCount = 0;
        /// <summary>
        /// What is the current set breathing count (different to Breathing recognizer breath count which is for the lifespan of the game)
        /// </summary>
        public int CurrentBreathCount
        {
            get { return currentBreathCount; }
        }

        /// <summary>
        /// Public value for setting / getting the current Session set count (cannot be changed while a session is running)
        /// </summary>
        public int SessionSetCount = 3;

        /// <summary>
        /// Public value for setting / getting the current Session breath count (cannot be changed while a session is running)
        /// </summary>
        public int SessionBreathCount = 8;

        #endregion Properties

        #region Events

        public event SessionStartedEventHandler SessionStarted;
        public event SessionEventHandler SetStarted;
        public event SessionEventHandler SetComplete;
        public event SessionEventHandler SessionComplete;
        public event SessionEventHandler SessionPaused;
        public event SessionEventHandler SessionResumed;

        #endregion Events

        #region Constructors

        public FizzyoSession()
        {
            LoadPlayerPrefs();
        }

        public FizzyoSession(int SetCount, int BreathCount) : base()
        {
            SessionSetCount = SetCount;
            SessionBreathCount = BreathCount;
        }

        #endregion Constructors

        #region Functions

        /// <summary>
        /// Update is used in the session manager to track if the player has paused breathing for any reason and alert the game if this happens
        /// </summary>
        public void Update()
        {
            if (isSessionStarted && isSetStarted && !isSessionPaused)
            {
                // Calculating stopping breathing
                // ------------------------------
                // Check last pressure to current pressure
                // if pressure is zero, increase non-breathing time if in a set is in progress
                // if non breathing time exceeds threshold, pause
                // If user starts blowing again, resume.

                if (FizzyoFramework.Instance.Device.Pressure() > FizzyoFramework.Instance.Recogniser.MinBreathThreshold)
                {
                    timeBreathpaused = 0;
                }

                if (FizzyoFramework.Instance.Device.Pressure() < FizzyoFramework.Instance.Recogniser.MinBreathThreshold)
                {
                    timeBreathpaused += Time.deltaTime;
                }

                if (timeBreathpaused > breathingPausedThreshold)
                {
                    PauseSession();
                }
            }
            else if (isSessionPaused)
            {
                if (FizzyoFramework.Instance.Device.Pressure() > FizzyoFramework.Instance.Recogniser.MinBreathThreshold)
                {
                    ResumeSession();
                }
            }
        }

        /// <summary>
        /// Initializes a new Session
        /// </summary>
        /// <param name="AutoStart">(optional) also start the set automatically</param>
        public void StartSession(bool AutoStart = false)
        {
            Debug.Log("Session Starting");
            currentSetCount = 0;
            currentBreathCount = 0;
            timeBreathpaused = 0;
            isSessionStarted = true;

            SessionStarted?.Invoke(this);

            if (AutoStart)
            {
                StartSet();
            }
        }

        /// <summary>
        /// Begin the next set
        /// </summary>
        /// <param name="SetNumber"></param>
        public void StartSet(int SetNumber = 0)
        {
            FizzyoFramework.Instance.Recogniser.BreathComplete += Recogniser_BreathComplete;

            Debug.Log("Set Starting");
            currentSetCount = SetNumber > 0 ? SetNumber : currentSetCount + 1;
            currentBreathCount = 0;
            isSetStarted = true;
            SetStarted?.Invoke(this, new SessionEventArgs(GetSessionStatus()));
        }

        /// <summary>
        /// Set has finished, inform the game
        /// </summary>
        public void FinishSet()
        {
            Debug.Log("Set Finishing");

            FizzyoFramework.Instance.Recogniser.BreathComplete -= Recogniser_BreathComplete;
            
            currentBreathCount = 0;
            isSetStarted = false;
            SetComplete?.Invoke(this, new SessionEventArgs(GetSessionStatus()));
            if (currentSetCount >= SessionSetCount)
            {
                FinishSession();
            }
        }

        /// <summary>
        /// Session is paused, either due to game event or player has paused breathing
        /// </summary>
        public void PauseSession()
        {
            Debug.Log("Session Paused");

            FizzyoFramework.Instance.Recogniser.BreathComplete -= Recogniser_BreathComplete;
                       
            isSessionPaused = true;
            SessionPaused?.Invoke(this, new SessionEventArgs(GetSessionStatus()));
        }

        /// <summary>
        /// Resuming session either due to game event or player has resumed breathing
        /// </summary>
        public void ResumeSession()
        {
            Debug.Log("Session Resumed");

            FizzyoFramework.Instance.Recogniser.BreathComplete += Recogniser_BreathComplete;

            timeBreathpaused = 0;
            isSessionPaused = false;
            SessionResumed?.Invoke(this, new SessionEventArgs(GetSessionStatus()));
        }

        /// <summary>
        /// Session is complete
        /// </summary>
        void FinishSession()
        {
            Debug.Log("Session Finished");

            isSessionStarted = false;
            SessionComplete?.Invoke(this, new SessionEventArgs(GetSessionStatus()));
        }

        /// <summary>
        /// Collate results of the current set for events
        /// </summary>
        /// <returns></returns>
        private SetResults GetSessionStatus()
        {
            return new SetResults() { Set = currentSetCount, Breath = currentBreathCount, BreathLength = lastBreathLength, MaxPressure = lastMaxPressure, Quality = lastQuality  };
        }

        /// <summary>
        /// Breathing event from the breath recognizer
        /// </summary>
        private void Recogniser_BreathComplete(object sender, ExhalationCompleteEventArgs e)
        {
            if (e.IsBreathFull)
            {
                currentBreathCount += 1;
                lastBreathLength = e.Breathlength;
                lastMaxPressure = e.ExhaledVolume;
                lastQuality = e.BreathQuality;

                //If we have reached the last breath for the set, finish the set
                if (currentBreathCount >= SessionBreathCount)
                {
                    FinishSet();
                }
            }
        }
        #endregion Functions

        #region Save/Load Set/Breath values
        // Loads the player prefs
        public void LoadPlayerPrefs()
        {
            if (PlayerPrefs.HasKey(sessionBreathCountKey))
                SessionBreathCount = PlayerPrefs.GetInt(sessionBreathCountKey);

            if (PlayerPrefs.HasKey(sessionSetCountKey))
                SessionSetCount = PlayerPrefs.GetInt(sessionSetCountKey);
        }

        // Saves the player prefs
        public void SavePlayerPrefs()
        {
            PlayerPrefs.SetInt(sessionBreathCountKey, SessionBreathCount);
            PlayerPrefs.SetInt(sessionSetCountKey, SessionSetCount);
            PlayerPrefs.Save();
        }
        #endregion
    }

    #region Dependencies

    public struct SetResults
    {
        public int Set;
        public int Breath;
        public float BreathLength;
        public float MaxPressure;
        public int Quality;
    }

    /// <summary>
    /// Provides data about the current session to the receiver when the event fires
    /// </summary>
    public class SessionEventArgs : EventArgs
    {
        private SetResults results;

        public SessionEventArgs(SetResults results)
        {
            this.results = results;
        }

        public int SetCount
        {
            get { return results.Set; }
        }
        public int BreathCount
        {
            get { return results.Breath; }
        }

        public float BreathLength
        {
            get { return results.BreathLength; }
        }
               
        public float BreathVolume
        {
            get { return results.MaxPressure; }
        }

        public int BreathQuality
        {
            get { return results.Quality; }
        }
    }

    public delegate void SessionStartedEventHandler(object sender);
    public delegate void SessionEventHandler(object sender, SessionEventArgs e);

    #endregion Dependencies
}