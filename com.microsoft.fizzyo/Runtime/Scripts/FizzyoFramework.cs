// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This namespace contains all of the classes that handle loading and uploading data via the Fizzyo API, calibration
/// and breath framework
/// </summary>
namespace Fizzyo
{
    public enum FizzyoRequestReturnType { SUCCESS, INCORRECT_TOKEN, FAILED_TO_CONNECT, NOT_FOUND, ALREADY_UNLOCKED }

    ///Fizzyo
    /// <summary>
    /// Interface class for Fizzyo Framework
    /// </summary>
    public class FizzyoFramework : MonoBehaviour
    {
        public FizzyoConfigurationProfile FizzyoConfigurationProfile;

        /// <summary>
        /// Flag to search for instance the first time Instance property is called.
        /// Subsequent attempts will generally switch this flag false, unless the instance was destroyed.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static bool searchForInstance = true;

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public static bool IsInitialized => _instance != null;

        ///<summary>
        ///The singleton instance of the Fizzyo Framework
        ///</summary>
        public static FizzyoFramework _instance = null;
        public FizzyoUser User { get; set; }
        public FizzyoDevice Device { get; set; }
        public FizzyoAchievements Achievements { get; set; }
        public BreathRecogniser Recogniser { get; set; }
        public FizzyoAnalytics Analytics { get; set; }
        public FizzyoSession Session { get; set; }

        private static bool applicationIsQuitting = false;
        public string CallbackScenePath { get; private set; }

        public bool ShowPressure = false;

        //Singleton instance
        public static FizzyoFramework Instance
        {
            get
            {
                if (IsInitialized)
                {
                    return _instance;
                }

                if (Application.isPlaying && !searchForInstance)
                {
                    return null;
                }

                var objects = FindObjectsOfType<FizzyoFramework>();
                searchForInstance = false;
                FizzyoFramework newInstance;

                switch (objects.Length)
                {
                    case 0:
                        newInstance = new GameObject(nameof(FizzyoFramework)).AddComponent<FizzyoFramework>();
                        break;
                    case 1:
                        newInstance = objects[0];
                        break;
                    default:
                        Debug.LogError($"Expected exactly 1 {nameof(FizzyoFramework)} but found {objects.Length}.");
                        return null;
                }

                Debug.Assert(newInstance != null);

                if (!applicationIsQuitting)
                {
                    // Setup any additional things the instance needs.
                    newInstance.InitializeInstance();
                }
                else
                {
                    // Don't do any additional setup because the app is quitting.
                    _instance = newInstance;
                }

                Debug.Assert(_instance != null);

                return _instance;
            }
        }

        private FizzyoFramework()
        {
            if (_instance != null)
                return;
        }

        private void InitializeInstance()
        {
            if (IsInitialized) { return; }

            _instance = this;

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(_instance.transform.root);
            }

            User = new FizzyoUser();
            Device = new FizzyoDevice();
            Recogniser = new BreathRecogniser();
            Achievements = new FizzyoAchievements();
            Analytics = new FizzyoAnalytics();
            Session = new FizzyoSession();
        }

        void Start()
        {
            if (!IsInitialized) { InitializeInstance(); }

#if ENABLE_WINMD_SUPPORT || UNITY_UWP
            //Pass credentials from hub
            string launchArguments = UnityEngine.WSA.Application.arguments;
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(launchArguments))
            {
                string[] firstParam = launchArguments.Split("?"[0]);
                if (firstParam.Length >= 2)
                {
                    string[] argumentsArray = firstParam[1].Split("&"[0]);
                    foreach (string argumentStr in argumentsArray)
                    {
                        if (!string.IsNullOrEmpty(argumentStr))
                        {
                            string[] argumentArray = argumentStr.Split('=');
                            if (argumentArray.Length >= 2)
                            {
                                arguments.Add(argumentArray[0], argumentArray[1]);
                            }
                        }
                    }
                }
            }
            //allow api endpoint override
            if (arguments.ContainsKey("apiPath"))
            {
                FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath = arguments["apiPath"];
            }

            if (arguments.ContainsKey("accessToken") && arguments.ContainsKey("userId"))
            {
                //Login using the Hubs pre-authenticated credentials
                FizzyoFramework.Instance.User.LoginUsingHub(arguments["userId"], arguments["accessToken"]);
            }
            else
            {
                if (FizzyoFramework.Instance.FizzyoConfigurationProfile.RequireLaunchFromHub)
                {
                    Debug.LogError("Launch Arguments -[" + launchArguments +"]");
                    SceneManager.LoadScene("Error");
                    return;
                }
            }

#endif
            Load();

            if (FizzyoConfigurationProfile.UseTestHarnessData)
            {
#if UNITY_EDITOR
                Device.StartPreRecordedData("Fizzyo/Examples/Data/" + FizzyoConfigurationProfile.TestHarnessDataFile.ToString() + ".fiz");
#endif
            }

            if (FizzyoConfigurationProfile.ShowCalibrateAutomatically && Device != null && !Device.Calibrated)
            {
                Scene scene = SceneManager.GetActiveScene();
                CallbackScenePath = scene.path;
                SceneManager.LoadScene("Calibration");
            }
        }

        void OnApplicationQuit()
        {
            if (Analytics != null)
            {
                Analytics.PostOnQuit();
            }
            if (Session != null)
            {
                Session.SavePlayerPrefs();
            }
            Debug.Log("[FizzyoFramework] Analytics is Null.");
        }

        private void OnApplicationFocus(bool focus)
        {
            if (Analytics != null)
            {
                Analytics.OnApplicationFocus(focus);
            }
        }

        private void Update()
        {
            //update the framework
            if (Device != null)
            {
                Recogniser?.AddSample(Time.deltaTime, Device.Pressure());
                Session?.Update();
            }
        }

        /// <summary>
        /// Loads the user data from the Fizzyo API.
        ///
        /// PlayerPrefs holds the users information in the following configuration:
        ///
        /// "online" - Integer - 0 or 1 - Tells the developer if the user is playing offline or online
        ///
        /// "calDone" - Integer - 0 or 1 - Tells the developer if the user has completed calibration
        ///
        /// "achievements" - String - Holds the achievements for the game and, if the user is online, tells the developer
        /// which achievements have been unlocked
        ///
        /// "achievementsToUpload" - String - Holds the achievements that have been unlocked in the current session
        ///
        /// Current players userID + "AchievementProgress" - String - Holds data on the achievement progress that the user has made in this game
        ///
        /// "accessToken" - String - Holds the access token that is acquired for the current user
        ///
        /// "tagDone" - Integer - 0 or 1 - Tells the developer if the user has completed setting a tag
        ///
        /// "userTag" - String - Holds the user tag
        ///
        /// "calPressure" - Float - Holds the pressure that the user has set in their calibration
        ///
        /// "calTime" - Integer - Holds the breath length that the user has set in their calibration
        ///
        /// "userId" - String - Holds the user Id that is acquired for the current user
        ///
        /// "gameId" - String - Holds the game Id for this specific game
        ///
        /// "gameSecret" - String - Holds the game secret for this specific game
        ///
        /// "userLoaded" - Integer - 0 or 1 - Shows if the users access token was loaded
        ///
        /// "calLoaded" - Integer - 0 or 1 - Shows if the users calibration data was loaded
        ///
        /// "achLoaded" - Integer - 0 or 1 - Shows if the users achievement data was loaded
        ///
        /// "tagLoaded" - Integer - 0 or 1 - Shows if the users tag was loaded
		///
        /// </summary>
        /// <param name="gameId">String that contains the current games ID </param>
        /// <param name="gameSecret">String that contains the current games secret</param>
        /// <returns>
        /// true if data is loaded and playing online
        ///
        /// false if data is not loaded and playing offline
        /// </returns>
        public bool Load()
        {
            if (FizzyoConfigurationProfile == null)
            {
                PlayOffline();
                return false;
            }

            //Login to server directly without the hub
            if (FizzyoConfigurationProfile.LoginFromDesktop && FizzyoFramework.Instance.User != null && !FizzyoFramework.Instance.User.LoggedIn)
            {
                FizzyoNetworking.loginResult = FizzyoFramework.Instance.User.LoginMSA(FizzyoConfigurationProfile.GameID, FizzyoConfigurationProfile.ApiPath);

                if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
                {
                    PlayOffline();
                    return false;
                }
            }

            FizzyoFramework.Instance.User.Load();
            FizzyoFramework.Instance.Achievements.Load();

            return true;
        }

        /// <summary>
        /// Sets up the player preferences to allow the user to play offline
        /// </summary>
        private static void PlayOffline()
        {
        }

        /// <summary>
        /// Changes the maximum calibrated breath pressure and length to the specified values. Will set the Calibrated boolean to true.  
        /// </summary>    
        public void SetCalibrationLimits(float maxPressure = 1, float maxBreath = 1)
        {
            //Set the device calibration values
            if (Device != null)
            {
                Device.maxPressureCalibrated = maxPressure;
                Device.maxBreathCalibrated = maxBreath;

                Device.Calibrated = true;
            }

            //update the recognizer if in use.
            if (Recogniser != null)
            {
                Recogniser.MaxPressure = maxPressure;
                Recogniser.MaxBreathLength = maxBreath;
            }
        }

        /// <summary>
        /// Resets all of the player preferences
        /// </summary>
        public void OnDestroy()
        {
            applicationIsQuitting = true;
        }

        private void OnEnable()
        {
            applicationIsQuitting = false;
        }
    }
}