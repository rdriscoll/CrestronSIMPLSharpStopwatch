// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="AVPlus Integration Pty Ltd">
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

namespace AVPlus.StopwatchApp
{
    using System;
    using System.Windows.Forms;
    using System.Diagnostics;
    using System.Globalization;

    public partial class Form1 : Form
    {
        TimeSpan LastBreakTime;
        int LapCount = 0;
        Stopwatch objStopWatch = new Stopwatch();
        bool paused = false;
        private Timer timerMain = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void StopWatchTimer_Tick(object sender, EventArgs e)
        {
            if (objStopWatch.IsRunning)
            {
                TimeSpan objTimeSpan = TimeSpan.FromMilliseconds(objStopWatch.ElapsedMilliseconds);
                lblDisplayTime.Text = string.Format(CultureInfo.CurrentCulture, "{0:00}:{1:00}.{2:00}", objTimeSpan.Minutes, objTimeSpan.Seconds, objTimeSpan.Milliseconds / 10);
                if (paused)
                {
                    paused = false;
                }
            }
        }

        private void nameCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (nameCheckBox.Checked)
            {
                label2.Visible = false;
                nameLabel.Visible = false;
                inputNameTextBox.Visible = false;
            }
            else
            {
                label2.Visible = true;
                nameLabel.Visible = true;
                inputNameTextBox.Visible = true;
            }
        }

        private void timeLeftCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (timeLeftCheckBox.Checked)
            {
                lblDisplayTime.Visible = false;
                lblTimeLeft.Visible = false;
            }
            else
            {
                lblDisplayTime.Visible = true;
                lblTimeLeft.Visible = true;
            }
        }

        DateTime EndOfTime;
        private void TimeLeftTimer_Tick(object sender, EventArgs e)
        {

            TimeSpan ts = EndOfTime.Subtract(DateTime.Now);
            string s = string.Format("{0:d2}:{1:d2}", ts.Minutes, ts.Seconds); //declare s
            lblTimeLeft.Text = s;
            //if time left reaches 0, means time's up.
            if (ts.TotalMilliseconds < 0)
            {
                ((Timer)sender).Enabled = false;
                MessageBox.Show("Time is up!");
            }

        }

        private void rankCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (rankCheckBox.Checked)
            {
                rankPanel.Visible = false;
                lblRank.Visible = false;
                lblName.Visible = false;
                lblRank1.Visible = false;
                lblRank2.Visible = false;
                lblRank3.Visible = false;
                lblRank4.Visible = false;
                lblRank5.Visible = false;
                nameRankTextBox1.Visible = false;
                nameRankTextBox2.Visible = false;
                nameRankTextBox3.Visible = false;
                nameRankTextBox4.Visible = false;
                nameRankTextBox5.Visible = false;
            }
            else
            {
                rankPanel.Visible = true;
                lblRank.Visible = true;
                lblName.Visible = true;
                lblRank1.Visible = true;
                lblRank2.Visible = true;
                lblRank3.Visible = true;
                lblRank4.Visible = true;
                lblRank5.Visible = true;
                nameRankTextBox1.Visible = true;
                nameRankTextBox2.Visible = true;
                nameRankTextBox3.Visible = true;
                nameRankTextBox4.Visible = true;
                nameRankTextBox5.Visible = true;
            }
        }


        public void tick(object sender, EventArgs e)
        {
            totalTimeTextBox.Text = objStopWatch.Elapsed.ToString();
        }

        private void btnReset_Click_1(object sender, EventArgs e)
        {
            //if reset button is clicked, reset to 00:00.00
            lblDisplayTime.Text = "00:00.00";
            objStopWatch.Reset();
            listBox1.Items.Clear();
            totalTimeTextBox.Clear();
            LapCount = 0;
            LastBreakTime = new TimeSpan(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EndOfTime = DateTime.Now.AddMinutes(1); //5 minutes countdown timer
            Timer t = new Timer() { Interval = 1000, Enabled = true };
            t.Tick += new EventHandler(TimeLeftTimer_Tick);
            TimeLeftTimer_Tick(null, null);

            timerMain.Enabled = true;
            timerMain.Interval = 50;
            timerMain.Tick += new System.EventHandler(StopWatchTimer_Tick);
        }

        //private void totalTimeTextBox_TextChanged(object sender, EventArgs e)
        //{
        //    double total = 0;
        //    foreach (object item in listBox1.Items)
        //    {
        //        total += (double)item;
        //    }

        //        totalTimeTextBox.Text = total.ToString();

        //}

        private void inputNameTextBox_TextChanged(object sender, EventArgs e)
        {
            nameLabel.Text = inputNameTextBox.Text;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            LapCount = 0;
            objStopWatch.Start();//start timer running
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            objStopWatch.Stop();
            totalTimeTextBox.Text = objStopWatch.Elapsed.ToString();
            btnLap.Enabled = false;
        }

        private void btnLap_Click(object sender, EventArgs e)
        {
            // listBox1.Items.Add(objStopWatch.Elapsed.ToString());
            TimeSpan ts = objStopWatch.Elapsed;
            TimeSpan LapTime = ts - LastBreakTime;
            LastBreakTime = ts;
            ++LapCount;
            if (LapCount == 5)
            {
                btnStop.PerformClick();
            }
            listBox1.Items.Add(LapTime.ToString());
        }
    }
}
