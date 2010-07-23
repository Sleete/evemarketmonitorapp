using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace EveMarketMonitorApp.Common
{
    /// <summary>
    /// This class provides support functions for timing diagnostics.
    /// Any calls to a method marked with the 'Conditional' attribute will only be run if
    /// the specified condition has been set when compiling the project.
    /// Note that methods with return values cannot be marked with the conditional attribute.
    /// </summary>
    public static class Diagnostics
    {
        private static Dictionary<int, MiniTimer> _timers = new Dictionary<int, MiniTimer>();
        private static TimeSpan tmpTime;

        [Conditional("DIAGNOSTICS")]
        public static void StartTimer(int timerIndex)
        {
            if (!_timers.ContainsKey(timerIndex))
            {
                _timers.Add(timerIndex, new MiniTimer());
            }
            MiniTimer timer = _timers[timerIndex];
            timer.Start = DateTime.UtcNow;
            timer.Active = true;
            _timers[timerIndex] = timer;
        }
        [Conditional("DIAGNOSTICS")]
        public static void StartTimer(string timerName)
        {
            int timerIndex = timerName.ToString().GetHashCode();
            StartTimer(timerIndex);
        }

        [Conditional("DIAGNOSTICS")]
        public static void StopTimer(int timerIndex)
        {
            if (_timers.ContainsKey(timerIndex))
            {
                MiniTimer timer = _timers[timerIndex];
                if (timer.Active)
                {
                    timer.Active = false;
                    timer.RunningTime = timer.RunningTime.Add(DateTime.UtcNow.Subtract(timer.Start));
                    _timers[timerIndex] = timer;
                }
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Error, "Timer index not valid");
            }
        }
        [Conditional("DIAGNOSTICS")]
        public static void StopTimer(string timerName)
        {
            int timerIndex = timerName.ToString().GetHashCode();
            StopTimer(timerIndex);         
        }

        [Conditional("DIAGNOSTICS")]
        public static void ResetTimer(int timerIndex)
        {
            MiniTimer timer;
            if (!_timers.ContainsKey(timerIndex))
            {
                timer = new MiniTimer();
                _timers.Add(timerIndex, timer);
            }
            else
            {
                timer = _timers[timerIndex];
            }
            timer.RunningTime = new TimeSpan();
            _timers[timerIndex] = timer;
        }
        [Conditional("DIAGNOSTICS")]
        public static void ResetTimer(string timerName)
        {
            int timerIndex = timerName.ToString().GetHashCode();
            ResetTimer(timerIndex);
        }
        [Conditional("DIAGNOSTICS")]
        public static void ResetAllTimers()
        {
            _timers = new Dictionary<int, MiniTimer>();
        }

        public static TimeSpan GetRunningTime(int timerIndex)
        {
            SetTmpRunningTime(timerIndex);
            return tmpTime;
        }
        public static TimeSpan GetRunningTime(string timerName)
        {
            int timerIndex = timerName.ToString().GetHashCode();
            SetTmpRunningTime(timerIndex);
            return tmpTime;
        }
        [Conditional("DIAGNOSTICS")]
        public static void SetTmpRunningTime(int timerIndex)
        {
            TimeSpan retVal = new TimeSpan();
            if (_timers.ContainsKey(timerIndex))
            {
                MiniTimer timer = _timers[timerIndex];
                if (timer.Active)
                {
                    retVal = DateTime.UtcNow.Subtract(timer.Start);
                }
                else
                {
                    retVal = timer.RunningTime;
                }
            }
            else
            {
                //throw new EMMAException(ExceptionSeverity.Error, "Timer index not valid");
            }
            tmpTime = retVal;
        }

        [Conditional("DIAGNOSTICS")]
        public static void DisplayDiag(string message)
        {
            MessageBox.Show(message, "Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private struct MiniTimer
        {
            public DateTime Start;
            public bool Active;
            public TimeSpan RunningTime;

            public MiniTimer(DateTime startdate)
            {
                Start = startdate;
                Active = true;
                RunningTime = new TimeSpan();
            }
        }
    }

}
