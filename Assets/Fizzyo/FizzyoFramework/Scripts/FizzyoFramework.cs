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
    public enum FizzyoRequestReturnType { SUCCESS, INCORRECT_TOKEN, FAILED_TO_CONNECT }

    ///Fizzyo
    /// <summary>
    /// Interface class for Fizzyo Framework
    /// </summary>

    public class FizzyoFramework : MonoBehaviour
    {
        public FizzyoConfigurationProfile FizzyoConfigurationProfile;

        ///<summary>
        ///The singleton instance of the Fizzyo Framework
        ///</summary>
        public static FizzyoFramework _instance = null;
        public FizzyoUser User { get; set; }
        public FizzyoDevice Device { get; set; }
        public FizzyoAchievements Achievements { get; set; }
        public BreathRecogniser Recogniser { get; set; }
        public FizzyoAnalytics Analytics { get; set; }

        private static bool applicationIsQuitting = false;
        public string CallbackScenePath { get; private set; }

        //Singleton instance
        public static FizzyoFramework Instance
        {
            //singleton pattern from: http://wiki.unity3d.com/index.php?title=Singleton
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(FizzyoFramework) +
                        "' already destroyed on application quit." +
                        " Won't create again - returning null.");
                }

                if (_instance == null)
                {
                    _instance = (FizzyoFramework)FindObjectOfType(typeof(FizzyoFramework));

                    if (FindObjectsOfType(typeof(FizzyoFramework)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<FizzyoFramework>();
                        singleton.name = "(singleton) " + typeof(FizzyoFramework).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] An instance of " + typeof(FizzyoFramework) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[Singleton] Using instance already created: " +
                            _instance.gameObject.name);
                    }
                }

                return _instance;
            }
        }

        private FizzyoFramework()
        {
            if (_instance != null)
                return;

            Debug.Log("[FizzyoFramework] Instantiate.");

            User = new FizzyoUser();
            Device = new FizzyoDevice();
            Recogniser = new BreathRecogniser();
            Achievements = new FizzyoAchievements();
            Analytics = new FizzyoAnalytics();


        }

        void Start()
        {
            Debug.Log("[FizzyoFramework] Start.");

            if (_instance != null)
                return;

            DontDestroyOnLoad(gameObject);

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

                    if(arguments.ContainsKey("accessToken") && arguments.ContainsKey("userId"))
                    {
                        User.Login(arguments["userId"], arguments["accessToken"]);
                    }
                    else
                    {
                        if (FizzyoFramework.Instance.FizzyoConfigurationProfile.RequireLaunchFromHub)
                        {
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

            if (FizzyoConfigurationProfile.ShowCalibrateAutomatically && !Device.Calibrated)
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
            Debug.Log("[FizzyoFramework] Analytics is Null.");
        }
        private void OnApplicationFocus(bool focus)
        {
            if (focus == false && Analytics != null)
            {
                Analytics.OnApplicationFocus(focus);
            }


        }
            private void Update()
        {
            //update the breath recoginiser
            if (Device != null)
            {
                Recogniser.AddSample(Time.deltaTime, Device.Pressure());
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
            //Login to server
            if (FizzyoConfigurationProfile != null && FizzyoConfigurationProfile.ShowLoginAutomatically && !User.LoggedIn)
            {
                LoginReturnType loginResult = User.Login();

                if (loginResult != LoginReturnType.SUCCESS)
                {
                    PlayOffline();
                    return false;
                }
            }
            else
            {
                PlayOffline();
                return false;
            }

            User.Load();
            Achievements.Load();

            return true;
        }

        /// <summary>
        /// Sets up the player preferences to allow the user to play offline
        /// </summary>
        private static void PlayOffline()
        {
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