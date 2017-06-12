// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="StopwatchWithTimer.cs" company="AVPlus Integration Pty Ltd">
//     {c} AV Plus Pty Ltd 2017.
//     http://www.avplus.net.au
//     20170611 Rod Driscoll
//     e: rdriscoll@avplus.net.au
//     m: +61 428 969 608
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//     
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//     
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
//
//     For more details please refer to the LICENSE file located in the root folder 
//      of the project source code;
// </copyright>

namespace AVPlus.Stopwatch
{
    using System;
    using System.Text;
    using System.Globalization;
    using System.Collections.Generic;
    using Crestron.SimplSharp;
    
    public delegate void TimerEventHandler(object sender, TimerEventArgs e);
    public delegate void BoolEventHandler(object sender, BoolEventArgs e);

    public class StopwatchWithTimer
    {
        //delegates
        public event TimerEventHandler StopwatchFb;
        public event TimerEventHandler CountdownFb;
        public event TimerEventHandler LapFb;
        public event BoolEventHandler StopwatchRunningFb;
        public event BoolEventHandler CountdownRunningFb;
        //vars
        private CTimer timerMain;
        private TimeSpan tsRemaining;
        private DateTime endTime;
        private Stopwatch oStopWatch = new Stopwatch();
        private List<TimeSpan> laps = new List<TimeSpan>();

        private bool countdownRunning = false;
        public ushort maxLaps = 5;
        public ushort timerRefresh = 100;
        public int countdownSeconds = 300;
        public bool showSplitSeconds = false;
        public bool showSeconds = true;
        public bool showMinutes = false;
        public bool showHours = false;

        public StopwatchWithTimer()
        { 
        }
        private void Timer_Tick(object sender)
        {
            if (oStopWatch.IsRunning)
                DoStopwatchFb();
            if (countdownRunning)
            {
                DoCountdownFb();
                if (endTime.Subtract(DateTime.Now).TotalMilliseconds <= 0)
                {
                    countdownRunning = false;
                    if (CountdownRunningFb != null)
                        CountdownRunningFb(this, new BoolEventArgs(countdownRunning));
                    //timerMain.Stop();
                    //timerMain.Dispose();
                }
            }
        }
        #region feedback

        private string MakeTimeString(TimeSpan ts, bool showMinutes_)
        {
            StringBuilder sb = new StringBuilder();
            if (ts.Hours > 0 || showHours)
                sb = sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}", ts.Hours));
            if (showMinutes_ || showMinutes || ts.Minutes > 0 || sb.Length > 0)
            {
                if (sb.Length > 0)
                {
                    sb = sb.Append(":");
                    sb = sb.Append(string.Format(CultureInfo.CurrentCulture, "{0:00}", ts.Minutes));
                }
                else
                    sb = sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}", ts.Minutes));
            }
            if (showSeconds)
            {
                if (sb.Length > 0)
                    sb = sb.Append(":");
                sb = sb.Append(string.Format(CultureInfo.CurrentCulture, "{0:00}", ts.Seconds));
            }
            if (showSplitSeconds && timerRefresh < 1000)
            {
                if (sb.Length > 0)
                    sb = sb.Append(".");
                if (timerRefresh > 99)
                    sb = sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}", ts.Milliseconds / 100));
                else if (timerRefresh > 9)
                    sb = sb.Append(string.Format(CultureInfo.CurrentCulture, "{0:00}", ts.Milliseconds / 10));
                else
                    sb = sb.Append(string.Format(CultureInfo.CurrentCulture, "{0:000}", ts.Milliseconds));
            }
            return sb.ToString();
        }

        private void DoStopwatchFb()
        {
            if (StopwatchFb != null)
            {
                TimeSpan ts = TimeSpan.FromMilliseconds(oStopWatch.ElapsedMilliseconds);
                StopwatchFb(this, new TimerEventArgs(MakeTimeString(ts, true), (ushort)ts.Hours, (ushort)ts.Minutes, (ushort)ts.Seconds, 1));
            }
        }
        private void DoLapFb()
        {
            if (LapFb != null)
            {
                TimeSpan ts = oStopWatch.Elapsed;
                LapFb(this, new TimerEventArgs(MakeTimeString(ts, true), (ushort)ts.Hours, (ushort)ts.Minutes, (ushort)ts.Seconds, (ushort)laps.Count));
            }
        }
        private void DoCountdownFb()
        {
            if (CountdownFb != null)
            {
                TimeSpan ts;
                if (countdownRunning)
                    ts = endTime.Subtract(DateTime.Now);
                else
                    ts = new TimeSpan((int)(countdownSeconds / 3600),
                                      (int)(countdownSeconds / 60 % 60),
                                      (int)(countdownSeconds % 60));
                CountdownFb(this, new TimerEventArgs(MakeTimeString(ts, false), (ushort)ts.Hours, (ushort)ts.Minutes, (ushort)ts.Seconds, 0));
                tsRemaining = ts;
            }
        }

        #endregion
        #region public

        public void StartStopwatch()
        {
            if (timerMain == null || timerMain.Disposed)
                timerMain = new CTimer(Timer_Tick, null, timerRefresh, timerRefresh);
            else
                timerMain.Reset(timerRefresh, timerRefresh);
            laps.Clear();
            oStopWatch.Start(); //start timer running
            if (StopwatchRunningFb != null)
                StopwatchRunningFb(this, new BoolEventArgs(oStopWatch.IsRunning));
            if (countdownSeconds > 0)
                StartCountdown();
        }
        public void StartCountdown()
        {
            if (countdownRunning)
                endTime = DateTime.Now.AddSeconds(tsRemaining.TotalSeconds);
            else
                endTime = DateTime.Now.AddSeconds(countdownSeconds); //start from beginning
            countdownRunning = true;
            if (CountdownRunningFb != null)
                CountdownRunningFb(this, new BoolEventArgs(countdownRunning));
            if(timerMain == null)
                timerMain = new CTimer(Timer_Tick, null, timerRefresh, timerRefresh);
        }
        public void Stop()
        {
            oStopWatch.Stop();
            timerMain.Stop();
            DoStopwatchFb();
            if (StopwatchRunningFb != null)
                StopwatchRunningFb(this, new BoolEventArgs(oStopWatch.IsRunning));
            //countdownRunning = false;
            DoCountdownFb();
        }
        public void Lap()
        {
            TimeSpan LapTime = laps.Count > 0 ? oStopWatch.Elapsed - laps[laps.Count - 1] : oStopWatch.Elapsed;
            laps.Add(LapTime);
            //CrestronConsole.PrintLine("laps.count: {0}", laps.Count);
            DoLapFb();
        }
        public void Reset()
        {
            oStopWatch.Reset();
            DoStopwatchFb();
            //if (StopwatchFb != null)
            //    StopwatchFb(this, new TimerEventArgs("00:00", (ushort)0, (ushort)0, (ushort)0, 0));
            if (StopwatchRunningFb != null)
                StopwatchRunningFb(this, new BoolEventArgs(oStopWatch.IsRunning));
            countdownRunning = false;
            DoCountdownFb();
            if (LapFb != null)
                for (ushort i = 0; i < laps.Count; i++)
                    LapFb(this, new TimerEventArgs("", (ushort)0, (ushort)0, (ushort)0, (ushort)(i + 1)));
            laps.Clear();
            if (timerMain != null)
            {
                timerMain.Stop();
                timerMain.Dispose();
            }
        }
        public void ShowSplitSeconds(ushort val)
        {
            showSplitSeconds = val > 0;
        }

        #endregion
        #region countdown

        public void SecondsUp()
        {
            if (countdownRunning)
                endTime = endTime.AddSeconds(1);
            else
                countdownSeconds++;
            DoCountdownFb();
        }
        public void SecondsDown()
        {
            if (countdownRunning)
                endTime = endTime.Subtract(new TimeSpan(0, 0, 1));
            else
            {
                if (countdownSeconds > 0)
                    countdownSeconds--;
                else
                    countdownSeconds = 0;
            }
            DoCountdownFb();
        }
        public void MinutesUp()
        {
            if (countdownRunning)
                endTime = endTime.AddMinutes(1);
            else
                countdownSeconds += 60;
            DoCountdownFb();
        }
        public void MinutesDown()
        {
            if (countdownRunning)
                endTime = endTime.Subtract(new TimeSpan(0, 1, 0));
            else
            {
                if (countdownSeconds > 60)
                    countdownSeconds -= 60;
                else
                    countdownSeconds = 0;
            }
            DoCountdownFb();
        }
        public void HoursUp()
        {
            if (countdownRunning)
                endTime = endTime.AddHours(1);
            else
                countdownSeconds += 3600;
            DoCountdownFb();
        }
        public void HoursDown()
        {
            if (countdownRunning)
                endTime = endTime.Subtract(new TimeSpan(1, 0, 0));
            else
            {
                if (countdownSeconds > 3600)
                    countdownSeconds -= 3600;
                else
                    countdownSeconds = 0;
            }
            DoCountdownFb();
        }

        #endregion
    }

    public class BoolEventArgs : EventArgs
    {
        public ushort val { get; set; }
        public BoolEventArgs() { }
        public BoolEventArgs(bool val)
        {
            this.val = (ushort)(val ? 1:0);
        }
    }
    public class TimerEventArgs : EventArgs
    {
        public string str { get; set; }
        public ushort hours { get; set; }
        public ushort minutes { get; set; }
        public ushort seconds { get; set; }
        public ushort index { get; set; } // laps
        public TimerEventArgs() { }
        public TimerEventArgs(string str, ushort hours, ushort minutes, ushort seconds, ushort index)
        {
            this.str = str;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.index = index;
        }
    }
}