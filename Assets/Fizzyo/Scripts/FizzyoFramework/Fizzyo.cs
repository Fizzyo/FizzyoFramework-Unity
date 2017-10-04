using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// This namespace contains all of the classes that handle loading and uploading data via the Fizzyo API, calibration
/// and breath framework
/// </summary>
namespace Fizzyo
{

    //Fizzyo
    /*
     * Interface class for Fizzyo Framework
     */
    public class Fizzyo : MonoBehaviour
    {
        public bool showLoginAutomatically = false;
        public bool showSetGamerTagAutomatically = false;
        public bool showCalibrateAutomatically = false;

        public string GameID { get; set; }

        private static Fizzyo _instance = null;
        public FizzyoUser User { get; set; }
        private FizzyoDevice device;
        public FizzyoAchievments Achievments { get; set; }

        private Fizzyo()
        {
            User = new FizzyoUser();
            device = new FizzyoDevice();
            Achievments = new FizzyoAchievments();
        }
        public static Fizzyo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Fizzyo();
                }
                return _instance;
            }
        }

        void Awake()
        {
           // DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            User = new FizzyoUser();
            User.Login();

        }


        /// <summary>
        /// Loads the user data from the Fizzyo API
        /// PlayerPrefs holds the users information in the following configuration:
        /// "online" - Integer - 0 or 1 - Tells the developer if the user is playing offline or online 
        /// "calDone" - Integer - 0 or 1 - Tells the developer if the user has completed calibration 
        /// "achievments" - String - Holds the achievements for the game and, if the user is online, tells the developer
        /// which achievements have been unlocked 
        /// "achievmentsToUpload" - String - Holds the achievements that have been unlocked in the current session
        /// Current players userID + "AchievementProgress" - String - Holds data on the achievement progress that the user has made in this game 
        /// "accessToken" - String - Holds the access token that is aquired for the current user
        /// "tagDone" - Integer - 0 or 1 - Tells the developer if the user has completed setting a tag
        /// "userTag" - String - Holds the user tag
        /// "calPressure" - Float - Holds the pressure that the user has set in their calibration
        /// "calTime" - Integer - Holds the breath length that the user has set in their calibration
        /// "userId" - String - Holds the user Id that is aquired for the current user
        /// "gameId" - String - Holds the game Id for this specific game
        /// "gameSecret" - String - Holds the game secret for this specific game
        /// "userLoaded" - Integer - 0 or 1 - Shows if the users access token was loaded
        /// "calLoaded" - Integer - 0 or 1 - Shows if the users calibration data was loaded
        /// "achLoaded" - Integer - 0 or 1 - Shows if the users achievement data was loaded
        /// "tagLoaded" - Integer - 0 or 1 - Shows if the users tag was loaded
        /// </summary>
        /// <param name="gameId"> 
        /// String that contains the current games ID
        /// </param>  
        /// <param name="gameSecret"> 
        /// String that contains the current games secret
        /// </param>  
        /// <returns>
        /// true if data is loaded and playing online
        /// false if daa is not loaded and playing offline
        /// </returns>  
        public static bool LoadUserData()
            {

                ResetPlayerPrefs();

                if (PlayerPrefs.GetInt("userLoaded") == 0)
                {
                    PlayOffline();
                    return false;

                } else
                {

                  //  GetCalibrationData(); 
                  //  GetUnlockedAchievements(); 
                  //  user.GetUserTag();
               
                    if (PlayerPrefs.GetInt("calLoaded") == 0 || PlayerPrefs.GetInt("achLoaded") == 0 || PlayerPrefs.GetInt("tagLoaded") == 0)
                    {

                        PlayOffline();
                        return false;
                    }
                    PlayerPrefs.SetInt("online", 1);
                    return true;
                }

            

            }





            /// <summary>
            /// Sets up the player preferences to allow the user to play offline
            /// </summary>
            public static void PlayOffline()
            {
                ResetPlayerPrefs();
            }

            /// <summary>
            /// Resets all of the player preferences
            /// </summary>
            private static void ResetPlayerPrefs()
            {
                PlayerPrefs.SetInt("online", 0);
                PlayerPrefs.SetInt("calDone", 0);
                string path = Application.streamingAssetsPath + "/Achievements.json";
                string achJSONData = File.ReadAllText(path);
                PlayerPrefs.SetString("achievements", achJSONData);
                PlayerPrefs.SetString("achievementsToUpload", "");
                PlayerPrefs.SetString("achievementsToProgress", "");

                PlayerPrefs.DeleteKey("accessToken");
                PlayerPrefs.DeleteKey("tagDone");
                PlayerPrefs.DeleteKey("userTag");
                PlayerPrefs.DeleteKey("calPressure");
                PlayerPrefs.DeleteKey("calTime");
                PlayerPrefs.DeleteKey("userId");
                PlayerPrefs.DeleteKey("gameId");
                PlayerPrefs.DeleteKey("gameSecret");
                PlayerPrefs.DeleteKey("userLoaded");
                PlayerPrefs.DeleteKey("calLoaded");
                PlayerPrefs.DeleteKey("achLoaded");
                PlayerPrefs.DeleteKey("tagLoaded"); 
            }

    
    

            /// <summary>
            /// Uploads a players session data and achievements
            /// </summary>
            /// <param name="goodBreathCount"> 
            /// Integer that contains the amount of good breaths that were completed in the session
            /// </param>  
            /// <param name="badBreathCount"> 
            /// Integer that contains the amount of bad breaths that were completed in the session
            /// </param>  
            /// <param name="score"> 
            /// Integer that holds the players score for that session
            /// </param>  
            /// <param name="startTime"> 
            /// Integer that holds the time that the session was started
            /// </param>  
            /// <param name="setCount"> 
            /// Integer that holds the amount of sets that were completed in the session
            /// </param>  
            /// <param name="breathCount"> 
            /// Integer that holds the amount of breaths that were completed in the session
            /// <returns>
            /// String - "Session Upload Complete /nAchievement Upload Complete" - If session upload completes and achievement upload completes
            /// String - "Session Upload Complete /nAchievement Upload Failed" - If session upload completes and achievement upload fails
            /// String - "Session Upload Failed /nAchievement Upload Complete" - If session upload fails and achievement upload completes
            /// String - "Session Upload Failed /nAchievement Upload Failed" - If session upload fails and achievement upload fails
            /// </returns>
            public static string Session(int goodBreathCount, int badBreathCount, int score, int startTime, int setCount, int breathCount)
            {

                if (PlayerPrefs.GetInt("online") == 0)
                {
                    return "Session Upload Failed";
                }

                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                TimeSpan diff = DateTime.UtcNow - origin;
                int endTime = (int)Math.Floor(diff.TotalSeconds);

                string postSession = "https://api.fizzyo-ucl.co.uk/api/v1/game/:id/sessions";

                WWWForm form = new WWWForm();

                form.AddField("id", PlayerPrefs.GetString("gameId"));
                form.AddField("secret", PlayerPrefs.GetString("gameSecret"));
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

                WWW sendPostSession = new WWW(postSession, rawData, headers);

                string status = "Session Upload Complete";

                while (!sendPostSession.isDone) { }

                if (sendPostSession.error != null)
                {
                    status = "Session Upload Failed";
                }
 

                return status;

            }

           
    }

    /// <summary>
    /// An Instance of this class can be created to calibrate the game based on the users device input
    /// </summary>
    public class Calibration : MonoBehaviour
    {
        // Time that current breath has been held for
        private float breathLength;

        // How many calibration steps have been completed
        public int calibrationStep = 1;

        // How many calibration steps need to be completed
        public int requiredSteps = 3;

        // Status of calibration
        public string calibrationStatus;

        // A color reflecting of the status of the calibration
        public Color calibrationColor;

        // List that holds pressure readings from calibration
        private List<float> pressureReadings = new List<float>();

        // List that holds pressure readings from calibration
        private List<float> avgPressureReadings = new List<float>();

        // List that holds pressure readings from calibration
        private List<float> avgLengths = new List<float>();

        // Holds average pressure reading
        private float avgPressureReading;

        // Average time of each calibration step
        private float avgLength;

        // Breath has to be above this to register
        private float minPressureThreshold = 0.1f;

        // Pressure used for calibration from device
        public float pressure;

        // If true calibration script is running
        public bool calibrating = false;

        // If true calibration is finished
        public bool calibrationFinished = false;


        /// <summary>
        /// Used to get input from the device to get a pressure and time value that can be used in the breath framework, according to the breathing capacity of the user
        /// Pressure is a float value that determines how hard the user needs to blow into the device to constitute a good breath
        /// Time is an integer value that determines how long the user needs to blow into the device to constitute a good breath
        /// Calibration pressure and time are saved in the player preferences as "calPressure" and "calTime"
        /// </summary>
        public void Calibrate()
        {

            // Pressure comes from device
            pressure = FizzyoDevice.Instance().Pressure();

            // if incoming pressure is above threshold
            if (pressure > minPressureThreshold)
            {
                // Pressure readings are taken every update
                pressureReadings.Add(pressure);
                breathLength += Time.deltaTime;

                calibrationStatus = "Status: Calibraion Step " + calibrationStep.ToString() + " In Progress";
                calibrationColor = Color.yellow;
            }
            // If the pressure is not being maintained
            else
            {

                if (breathLength > 1.0)
                {

                    // Average and maximum values are taken from the calibration pressure readings
                    avgPressureReadings.Add(pressureReadings.Sum() / pressureReadings.Count);
                    avgLengths.Add(breathLength);

                    int step = calibrationStep - 1;

                    if (calibrationStep == requiredSteps)
                    {

                        avgPressureReading = avgPressureReadings.Sum() / avgPressureReadings.Count;
                        avgLength = avgLengths.Sum() / avgLengths.Count;

                        calibrationStatus = "Status: Uploading...";
                        calibrationColor = Color.green;

                       // calibrationStatus = "Status: " + Data.Upload.Calibration(avgPressureReading, avgLength);
                        if (calibrationStatus == "Status: Upload Failed")
                        {
                            calibrationColor = Color.red;
                        } else
                        {
                            calibrationColor = Color.green;
                        }

                        calibrating = false;
                        calibrationFinished = true;

                    }
                    else
                    {

                        pressureReadings.Clear();
                        breathLength = 0;

                        calibrating = false;

                        calibrationStatus = "Status: Calibraion Step " + calibrationStep.ToString() + " Complete";
                        calibrationColor = Color.green;

                        calibrationStep += 1;

                    }

                }
                // If length of breath was less than 1
                else
                {

                    calibrating = false;

                    pressureReadings.Clear();
                    breathLength = 0;

                    // Reset countdown and text
                    calibrationStatus = "Status: Please Try Again";
                    calibrationColor = Color.red;
                }

            }

        }
    }

}
