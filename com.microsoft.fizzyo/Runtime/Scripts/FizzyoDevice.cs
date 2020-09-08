// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR
using System.IO;
using System.Timers;
#endif

using UnityEngine;

namespace Fizzyo
{
    /// <summary>
    /// Class responsible for parsing data from the Fizzyo device
    /// </summary> 
    public class FizzyoDevice
    {
        /// <summary>
        /// True if pulling data from an internal file. Useful for testing. 
        /// </summary> 
        public bool useRecordedData = false;

        /// <summary>
        /// True if looping through recorded data. Useful for testing
        /// </summary> 
        public bool loop = true;

        /// <summary>
        /// Path of the recorded data 
        /// </summary> 
        public string recordedDataPath = "Fizzyo/Data/FizzyoData_3min.fiz";

        /// <summary>
        /// This is the maximum pressure to expect from the device, all incoming values will be mapped to it. 
        /// </summary>        
        public float maxPressureCalibrated = 1.0f;

        /// <summary>
        /// This is the maximum breath length to expect from the device, all incoming values will be mapped to it. 
        /// </summary>        
        public float maxBreathCalibrated = 1.0f;

        //protected
        // protected StreamReader fileReader = null;
        protected string text = " "; // assigned to allow first line to be read below
                                     // System.Timers.Timer pollTimer = new System.Timers.Timer();
        float pressure = 0;

#if WINDOWS_UWP
        private FizzyoHID fizzyoHID;
#endif

#if UNITY_EDITOR
        private StreamReader fileReader = null;
        private Timer pollTimer;
#endif

        /// <summary>
        /// If true, indicates the device has been already calibrated. 
        /// </summary>    
        public bool Calibrated = false;

        public FizzyoDevice()
        {
#if WINDOWS_UWP
            fizzyoHID = FizzyoHID.Instance();
#endif
        }

        //Cleanup  
        void OnApplicationQuit()
        {
#if UNITY_EDITOR
            //Close file pointer 
            if (fileReader != null)
                fileReader.Close();

            //Stop Timer 
            pollTimer.Stop();
#endif
        }

        /// <summary>
        /// If useRecordedData is set to false pressure data is streamed from the device or streamed from a log file if set to true.
        /// </summary>
        /// <returns>pressure data reported from device or log file with a range of -1 - 1.</returns>
        public float Pressure()
        {
            if (!useRecordedData)
            { 
                if (Input.GetKey(KeyCode.Z) && !FizzyoFramework.Instance.FizzyoConfigurationProfile.RequireLaunchFromHub)
                {
                    return 1f;
                }
                else
                {
#if WINDOWS_UWP
                    pressure = (float)fizzyoHID.CurrentPressureValue;
#else
                    pressure = Input.GetAxisRaw("Horizontal");
#endif
                    if (FizzyoFramework.Instance.ShowPressure) { Debug.Log("Pressure recorded at: " + pressure); } 

                    //Check if pressure value is beyond the 0..1 range
                    if (pressure > 1)
                    {
                        pressure = (pressure -127) / 128;
                    }
                }
            }

            return pressure / maxPressureCalibrated;
        }

        /// <summary>
        /// Checks if the button in the fizzyo device is being pushed down.  
        /// </summary>    
        public bool ButtonDown()
        {
            return Input.GetButtonDown("Fire1");
        }

#if UNITY_EDITOR
        internal void StartPreRecordedData(string path)
        {
            useRecordedData = true;
            recordedDataPath = path;

            //Open a StreamReader to our recorded data
            try
            {
                fileReader = new StreamReader(Application.dataPath + "/" + recordedDataPath);
            }
            catch
            {
                Debug.Log("could not load file " + recordedDataPath);
            }
            finally
            {
                Debug.Log("file loaded " + recordedDataPath);
                pollTimer = new Timer();
                pollTimer.Interval = 300; //load new pressure val every 30ms 
                pollTimer.Elapsed += PollLoggedData;
                pollTimer.Start();
            }
        }

        void PollLoggedData(object o, System.EventArgs e)
        {
            if (text != null)
            {
                text = fileReader.ReadLine();
                string[] parts = text.Split(' ');
                if (parts.Length == 2 && parts[0] == "v")
                {
                    float pressure = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat) / 100.0f;
                    this.pressure = pressure;
                }

                if (loop && fileReader.EndOfStream)
                {
                    fileReader.DiscardBufferedData();
                    fileReader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                }
            }
        }
#endif
    }
}