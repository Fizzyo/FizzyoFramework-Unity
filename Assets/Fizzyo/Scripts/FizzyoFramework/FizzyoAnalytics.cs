using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Fizzyo
{
    public class FizzyoAnalytics: MonoBehaviour
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
