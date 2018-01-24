using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Fizzyo
{
		/// <summary>
        /// An instance of this class can be created to send game analytics once a session is over. 
        /// This class is waiting for the developments of the API endpoints to be created.
        /// </summary> 
    public class FizzyoAnalytics
    {


        private void Start()
        {
            PostGameStart(System.DateTime.Today);
        }
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                PostGameStart(System.DateTime.Today);
            }
            else
            {
                PostGameEnd(System.DateTime.Today);
            }
        }
        private void OnApplicationQuit()
        {
            PostGameEnd(System.DateTime.Today);
        }

        void PostGameStart(DateTime startTime)
        {
            //TODO: impliment api endpoint for game start analytics.
        }


        void PostGameEnd(DateTime endTime)
        {
            //TODO: impliment api endpoint for game end analytics.
        }
    }
}
