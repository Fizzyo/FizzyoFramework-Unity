using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        [Header("Script Behaviour")]
        [Tooltip("Automatically show login screen at start of game.")]
        ///<summary>
        ///Set to true, shows login screen at start of the game
        ///</summary>
        public bool showLoginAutomatically = true;

        [Tooltip("Automatically show gamer tag editor if user does not have this set.")]
        ///<summary>
        ///Set to true, shows login screen at start of the game
        ///</summary>
        public bool showSetGamerTagAutomatically = false;

        [Tooltip("Automatically show calibration screen if never calibratd by user.")]
        ///<summary>
        ///Set to true, shows calibration screen if there has never been a calibration
        ///</summary>
        public bool showCalibrateAutomatically = true;

        [Tooltip("Game ID given by Fizzyo API.")]
        ///<summary>
        ///The game ID given by the Fizzyo API
        ///</summary>
        public string gameID = "87f2ae03-d34f-4045-a308-feb746e857b2";

        [Tooltip("Game secret given by Fizzyo API.")]
        ///<summary>
        ///The game secret given by the Fizzyo API
        ///</summary>
        public string gameSecret = "7BErm0wMvbmXMpq6ANBNLnAaYAlO1nqVqM15wNJAPdRom7lYyKKOEzqeGyOXpZKn";

        [Header("Test Harness")]

        [Tooltip("Use test harness data.")]
        ///<summary>
        ///Set false, enables use of test data instead of live data
        ///</summary>
        public bool useTestHarnessData = false;

        //Use test harness instead of live data
        public enum TestHarnessData { p1_acapella, p1_pep, p2_acapella };
        ///<summary>
        ///The type of data used for testing
        ///</summary>
        public TestHarnessData testHarnessDataFile = TestHarnessData.p1_acapella;

        ///<summary>
        ///The singleton instance of the Fizzyo Framework
        ///</summary>
        public static FizzyoFramework _instance = null;
        public FizzyoUser User { get; set; }
        public FizzyoDevice Device { get; set; }
        public FizzyoAchievements Achievements { get; set; }
        public BreathRecogniser Recogniser { get; set; }

        private static object _lock = new object();
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
               //     return null;
                }

               // lock (_lock)
                {
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
        }



        void Start()
        {
            Debug.Log("[FizzyoFramework] Start.");

            if (_instance != null)
                return;

            DontDestroyOnLoad(gameObject);

            Load();


            if (useTestHarnessData)
            {
#if UNITY_EDITOR
                Device.StartPreRecordedData("Fizzyo/Data/" + testHarnessDataFile.ToString() + ".fiz");
#endif
            }


            if (showCalibrateAutomatically && !Device.Calibrated)
            {
                Scene scene = SceneManager.GetActiveScene();
                CallbackScenePath = scene.path;
                SceneManager.LoadScene("Fizzyo/Scenes/Calibration");
            }



        }


        private void Update()
        {
            //update the breath recoginiser
            if (Device != null) {
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
        /// "accessToken" - String - Holds the access token that is aquired for the current user
        ///
        /// "tagDone" - Integer - 0 or 1 - Tells the developer if the user has completed setting a tag
        ///
        /// "userTag" - String - Holds the user tag
        ///
        /// "calPressure" - Float - Holds the pressure that the user has set in their calibration
        ///
        /// "calTime" - Integer - Holds the breath length that the user has set in their calibration
        ///
        /// "userId" - String - Holds the user Id that is aquired for the current user
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

            if (showLoginAutomatically)
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
               // ResetPlayerPrefs();
            }

            /// <summary>
            /// Resets all of the player preferences
            /// </summary>



            public void OnDestroy()
            {
                applicationIsQuitting = true;
            }


    }


}
