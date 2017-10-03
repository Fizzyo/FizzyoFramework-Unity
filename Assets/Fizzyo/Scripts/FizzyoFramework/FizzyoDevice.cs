using System;
using System.IO;
// using System.Timers;
using UnityEngine;

namespace Fizzyo
{
    public class FizzyoDevice : MonoBehaviour
    {
        // True if pulling data from internal file
        public bool useRecordedData = false;

        // True if looping through recorded data
        public bool loop = true;

        // Data path of recorded data
        // public string recordedDataPath = "Data/FizzyoData_3min.fiz";

        // Used to create an instance of FizzyoDevice
        private static FizzyoDevice instance;


        private static object threadLock = new System.Object();

        //protected
        // protected StreamReader fileReader = null;
        protected string text = " "; // assigned to allow first line to be read below
                                     // System.Timers.Timer pollTimer = new System.Timers.Timer();
        float pressure = 0;


        public static FizzyoDevice Instance()
        {
            if (instance == null)
            {
                lock (threadLock)
                {
                    /*
                    if (instance == null)
                    {
                        instance = GameObject.FindObjectOfType<FizzyoDevice>();
                    }
                    */

                    if (instance == null)
                    {
                        instance = (new GameObject("EasySingleton")).AddComponent<FizzyoDevice>();
                    }

                }
            }
            return instance;
        }

        /*
        // Use this for initialization
        void Start()
        {
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


        //Cleanup  
        void OnApplicationQuit()
        {
            //Close file pointer 
            fileReader.Close();

            //Stop Timer 
            pollTimer.Stop();

            Debug.Log("OnApplicationQuit");
        }
        */

        /// <summary>
        /// If useRecordedData is set to false pressure data is streamed from the device or streamed from a log file if set to true.
        /// </summary>
        /// <returns>pressure data reported from device or log file with a range of -1 - 1.</returns>
        public float Pressure()
        {
            if (useRecordedData)
            {
                return pressure;
            }
            else
            {
                if (Input.GetKey(KeyCode.Z))
                {
                    return 1f;
                }
                else
                {
                    return Input.GetAxisRaw("Horizontal");
                }
            }
        }

        public bool ButtonDown()
        {
            return Input.GetButtonDown("Fire1");
        }


        /*
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
        */
    }
}