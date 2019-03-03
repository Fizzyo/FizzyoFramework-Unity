﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Fizzyo
{
    [CreateAssetMenu(menuName = "Fizzyo/Create Fizzyo Configuration Profile", fileName = "FizzyoConfigurationProfile")]
    public class FizzyoConfigurationProfile : ScriptableObject
    {
        public enum TestHarnessData { p1_acapella, p1_pep, p2_acapella };

        [Header("Mandatory Game configuration")]
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

        [Tooltip("Automatically show calibration screen if never calibrated by user.\nBy default this should always be set to true")]
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
        private string apiPath = "https://api-staging.fizzyo-ucl.co.uk";

        ///<summary>
        ///API http path
        ///</summary>
        public string ApiPath
        {
            get
            {
                if (!apiPath.EndsWith("/"))
                {
                    apiPath = apiPath + "/";
                }
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


        [Header("Testing configuration")]
        [Tooltip("Automatically show login screen at start of game when the hub is not in use.")]
        [SerializeField]
        private bool loginFromDesktop = true;

        /// <summary>
        /// Show the Logon screen automatically when the application is launched from the desktop
        /// </summary>
        /// <remarks>Not to be used for a HUB deployment</remarks>
        public bool LoginFromDesktop
        {
            get
            {
                return loginFromDesktop;
            }

            set
            {
                loginFromDesktop = value;
            }
        }

        [Tooltip("Should always be set to true, unless testing the app without the hub.  Requires [LoginFromDesktop] to be set to allow login ")]
        [SerializeField]
        private bool requireLaunchFromHub = true;

        /// <summary>
        /// Require launch from Fizzyo Hub to run, also passes along login credentials. 
        /// </summary>
        public bool RequireLaunchFromHub
        {
            get
            {
                return requireLaunchFromHub;
            }

            set
            {
                requireLaunchFromHub = value;
            }
        }
    }
}
