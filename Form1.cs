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
using System.Xml.Serialization;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private VideoCaptureDevice VCD;
        private string ImagePath;
        private int Interval = 60000;
        private Dictionary<string, string> MonikerMap = new Dictionary<string, string>();
        //system.timers.timer code
        private bool GrabImage = true;
        System.Timers.Timer ImageCaptureTimer;

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
            LoadConfig();
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

            GrabImage = true;
            ImageCaptureTimer = new System.Timers.Timer(Interval);
            ImageCaptureTimer.Elapsed += new System.Timers.ElapsedEventHandler(ImageCaptureTimer_Elapsed);
            ImageCaptureTimer.Start();
        }

        void ImageCaptureTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            GrabImage = true;
        }

        void VCD_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (GrabImage)
            {
                GrabImage = false;
                Bitmap img = eventArgs.Frame;
                if (!Directory.Exists(ImagePath))
                    Directory.CreateDirectory(ImagePath);
                string filename = ImagePath + DateTime.Now.ToString("MMMM_dd_yyyy-HH.mm.ss") + ".jpg"; ;
                //Console.WriteLine(filename);
                img.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);                
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCapture();
            SaveConfig();
        }

        private void ChooseFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();            
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath;
                ImagePath = fbd.SelectedPath;
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
                ImageCaptureTimer.Stop();
            }
            catch { }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Interval = (int)numericUpDown1.Value * 60 * 1000;
        }

        private void SaveConfig() 
        {
            try
            {
                string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
                Config currentConfig = new Config();
                currentConfig.ImagePath = ImagePath;
                currentConfig.Interval = Interval / 60000;
                XmlSerializer xmlConfig = new XmlSerializer(typeof(Config));
                TextWriter configFile = new StreamWriter(exePath + @"\config");
                xmlConfig.Serialize(configFile, currentConfig);
                configFile.Close();
            }
            catch { }
        }

        private void LoadConfig()
        {
            try
            {
                string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
                TextReader configReader = new StreamReader(exePath + @"\config");
                XmlSerializer xmlConfig = new XmlSerializer(typeof(Config));
                Config savedConfig = (Config)xmlConfig.Deserialize(configReader);
                Interval = savedConfig.Interval * 60000;
                numericUpDown1.Value = savedConfig.Interval;
                ImagePath = savedConfig.ImagePath;
                numericUpDown1.Value = savedConfig.Interval;
                configReader.Close();
                textBox1.Text = ImagePath;                
            }
            catch { }
        }
    }

    public class Config 
    {
        public string ImagePath;
        public int Interval;
    }
}
