using System;
using UnityEngine;

namespace Framework.Services
{
    public class TimerService
    {
        public static double GameTimeStamp => DateTime.Now.Ticks;
        public static double GameTimeStampInSeconds => DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
    }
}