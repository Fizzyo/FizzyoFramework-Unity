// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Fizzyo
{
    [CreateAssetMenu(menuName = "Fizzyo/Create Fizzyo Configuration Profile", fileName = "FizzyoConfigurationProfile")]
    public class FizzyoConfigurationProfile : ScriptableObject
    {
        public enum TestHarnessData { p1_acapella, p1_pep, p2_acapella };

        [Header("Script Behaviour")]
        [Tooltip("Automatically show login screen at start of game.")]
        [SerializeField]
        private bool showLoginAutomatically = true;

        /// <summary>
        /// Show the Logon screen automatically when the application is launched
        /// </summary>
        public bool ShowLoginAutomatically
        {
            get
            {
                return showLoginAutomatically;
            }

            set
            {
                showLoginAutomatically = value;
            }
        }


        [Tooltip("Automatically show gamer tag editor if user does not have this set.")]
        [SerializeField]
        private bool showSetGamerTagAutomatically = false;

        /// <summary>
        /// Automatically show the GamerTag selection screen
        /// </summary>
        public bool ShowSetGamerTagAutomatically
        {
            get
            {
                return showSetGamerTagAutomatically;
            }

            set
            {
                showSetGamerTagAutomatically = value;
            }
        }

        [Tooltip("Automatically show calibration screen if never calibrated by user.")]
        [SerializeField]
        private bool showCalibrateAutomatically = true;

        /// <summary>
        /// Automatically show the calibration scene when the project starts
        /// </summary>
        public bool ShowCalibrateAutomatically
        {
            get
            {
                return showCalibrateAutomatically;
            }

            set
            {
                showCalibrateAutomatically = value;
            }
        }

        [Tooltip("Game ID given by Fizzyo API.")]
        [SerializeField]
        private string gameID = "<Enter GameID here/>";

        /// <summary>
        /// GameID from the Fizzyo.ucl app settings, used for connecting to the Fizzyo services
        /// </summary>
        public string GameID
        {
            get
            {
                return gameID;
            }

            set
            {
                gameID = value;
            }
        }

        [Tooltip("Game secret given by Fizzyo API.")]
        [SerializeField]
        private string gameSecret = "<Enter Game Secret here/>";

        /// <summary>
        /// GameSecret code from the Fizzyo.ucl app settings, used for connecting to the Fizzyo services
        /// </summary>
        public string GameSecret
        {
            get
            {
                return gameSecret;
            }

            set
            {
                gameSecret = value;
            }
        }


        [SerializeField]
        private string apiPath = "https://api.fizzyo-ucl.co.uk/";

        ///<summary>
        ///API http path
        ///</summary>
        public string ApiPath
        {
            get
            {
                return apiPath;
            }

            set
            {
                apiPath = value;
            }
        }

        [Header("Test Harness")]
        [Tooltip("Use test harness data.")]
        [SerializeField]
        private bool useTestHarnessData = false;

        /// <summary>
        /// Source the pressure reading data from a test file instead of an actual device
        /// </summary>
        public bool UseTestHarnessData
        {
            get
            {
                return useTestHarnessData;
            }

            set
            {
                useTestHarnessData = value;
            }
        }

        [SerializeField]
        private TestHarnessData testHarnessDataFile = TestHarnessData.p1_acapella;

        ///<summary>
        ///The type of data used for testing
        ///</summary>
        public TestHarnessData TestHarnessDataFile
        {
            get
            {
                return testHarnessDataFile;
            }

            set
            {
                testHarnessDataFile = value;
            }
        }
    }
}
