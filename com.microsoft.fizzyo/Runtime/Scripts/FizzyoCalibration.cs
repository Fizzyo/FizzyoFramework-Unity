// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fizzyo
{
    /// <summary>
    /// An Instance of this class can be created to calibrate the game based on the users device input
    /// </summary>
    public class Calibration : MonoBehaviour
    {
        // Time that current breath has been held for
        private float breathLength;

        /// <summary>
        /// How many calibration steps have been completed
        /// </summary>
        public int calibrationStep = 1;

        /// <summary>
        /// How many calibration steps are required
        /// </summary>
        public int requiredSteps = 3;

        /// <summary>
        /// Status of calibration
        /// </summary>
        public string calibrationStatus;

        /// <summary>
        /// A color reflecting the status of calibration
        /// </summary>
        public Color calibrationColor;

        // List that holds pressure readings from calibration
        private List<float> pressureReadings = new List<float>();

        // List that holds pressure readings from calibration
        private List<float> avgPressureReadings = new List<float>();

        // List that holds pressure readings from calibration
        private List<float> avgLengths = new List<float>();

        // Breath has to be above this to register
        private float minPressureThreshold = 0.1f;

        /// <summary>
        /// Pressure used for calibration from device
        /// </summary>
        public float pressure;

        /// <summary>
        /// If true calibration script is running
        /// </summary>
        public bool calibrating = false;

        /// <summary>
        /// If true calibration is finished
        /// </summary>
        public bool calibrationFinished = false;

        /// <summary>
        /// Used to get input from the device to get a pressure and time value that can be used in the breath framework, according to the breathing capacity of the user.
		///
        /// Pressure is a float value that determines how hard the user needs to blow into the device to constitute a good breath.
		///
        /// Time is an integer value that determines how long the user needs to blow into the device to constitute a good breath.
		///
        /// Calibration pressure and time are saved in the player preferences as "calPressure" and "calTime".
        /// </summary>
        public void Calibrate()
        {
            // Pressure comes from device
            pressure = FizzyoFramework.Instance.Device.Pressure();

            // if incoming pressure is above threshold
            if (pressure > minPressureThreshold)
            {
                // Pressure readings are taken every update
                pressureReadings.Add(pressure);
                breathLength += Time.deltaTime;

                calibrationStatus = "Status: Calibration Step " + calibrationStep.ToString() + " In Progress";
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

                    if (calibrationStep == requiredSteps)
                    {
                        calibrationStatus = "Status: Uploading...";
                        calibrationColor = Color.green;

                        if (calibrationStatus == "Status: Upload Failed")
                        {
                            calibrationColor = Color.red;
                        }
                        else
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

                        calibrationStatus = "Status: Calibration Step " + calibrationStep.ToString() + " Complete";
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