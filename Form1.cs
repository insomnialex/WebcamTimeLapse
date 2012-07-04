using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private VideoCaptureDevice VCD;
        private string ImagePath;
        private int Interval = 600;
        private int Current = 0;
        private Dictionary<string, string> MonikerMap = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
            ImagePath = exePath + @"\images\";
            textBox1.Text = ImagePath;            

            //load webcam devices
            FilterInfoCollection devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in devices)
            {
                MonikerMap.Add(device.Name, device.MonikerString);
                comboBox1.Items.Add(device.Name);
            }
            if(devices.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void StartCapture() 
        {
            string monikerString = MonikerMap[comboBox1.Text];
            VCD = new VideoCaptureDevice(monikerString);
            VCD.DesiredFrameRate = 10;
            VCD.DesiredFrameSize = new Size(640, 480);
            VCD.NewFrame += new NewFrameEventHandler(VCD_NewFrame);
            videoSourcePlayer1.VideoSource = VCD;
            VCD.Start();
        }

        void VCD_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (Current % Interval == 0)
            {
                Bitmap image = new Bitmap(eventArgs.Frame);
                if (!Directory.Exists(ImagePath))
                    Directory.CreateDirectory(ImagePath);
                string filename = ImagePath + DateTime.Now.ToString("MMMM_dd_yyyy-HH.mm.ss") + ".jpg"; ;
                //Console.WriteLine(filename);
                image.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                Current = 0;
            }
            Current++;
            //Console.WriteLine(Current);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCapture();
        }

        private void ChooseFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();            
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath;
            }
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ChooseFolder();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "start")
            {
                StartCapture();
                button1.Text = "stop";
            }
            else
            {
                StopCapture();
                button1.Text = "start";
            }
        }

        private void StopCapture()
        {
            try
            {
                VCD.Stop();
            }
            catch { }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Interval = (int)numericUpDown1.Value * 600;
        }
    }
}
