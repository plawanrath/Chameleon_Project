//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing.Imaging;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Timers;
    using System.Windows.Threading;
    using System.Threading;


    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    /// 

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        int Red, Green, Blue, rdevi, gdevi, bdevi, redmin, redmax, greenmax, greenmin, bluemin, bluemax;
        private System.Windows.Forms.Label ImagePathLbl;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button Btn_ChooseImage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_PickColor;
        private System.Windows.Forms.Button Btn_DetectColor;
        private System.Windows.Forms.Label Lbl_SelectedImage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelSelectedColor = new Panel();
        private System.Windows.Forms.Label SelectedColorNameLbl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        System.Drawing.Color actualColor;
        
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// 
        public MainWindow()
        {
            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

        //  System.Drawing.Color  actualColor = new System.Drawing.Color();
          //  actualColor = panelSelectedColor.BackColor;

            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close(); 
                this.kinectSensor = null;
            }
        }

        private void time_wrapper(Object sender, RoutedEventArgs e)
        {
            /*while (true)
            {
                //_timer.Elapsed += new ElapsedEventHandler(ScreenshotButton_Click);
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (Action)delegate()
                {
                    ScreenshotButton_Click();
                });
                Thread.Sleep(5000);
            }
             * */

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 5);
            dt.Tick += new EventHandler(ScreenshotButton_Click);
            dt.Start();

        }

        private void my_func(Object obj1, EventArgs e)
        {
            if (this.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

                string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
                }
                catch (IOException)
                {
                    this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }            
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        int photoindex = 1;
        private void ScreenshotButton_Click(object sender, EventArgs e)
        {
            if (this.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "kinect" + photoindex + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                        photoindex++;
                    }

                    this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
                }
                catch (IOException)
                {
                    this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }
        }
       
          private void SetColorBtn_Click(object sender, EventArgs e)
        {
         

            Red = Convert.ToInt32(R.Text);
            Green = Convert.ToInt32(G.Text);
            Blue = Convert.ToInt32(B.Text);
            rdevi = Convert.ToInt32(RDevi.Text);
            gdevi = Convert.ToInt32(GDevi.Text);
            bdevi = Convert.ToInt32(BDevi.Text);
            ProcessRed(Red,out redmin, out redmax);
            ProcessGreen(Red, out greenmin, out greenmax);
            ProcessBlue(Red, out bluemin, out bluemax);
            ProcessImage();

        }

          private void ProcessRed(int pixel, out int redmin, out int redmax)
          {
              if (pixel + rdevi > 255)
              {
                  redmax = 255;
              }
              else
              {
                  redmax = pixel + rdevi;
              }

              if (pixel - rdevi > 0)
              {
                  redmin = pixel - rdevi;
              }
              else
              {
                  redmin = 0;
              }
          }

          private void ProcessGreen(int pixel, out int greenmin, out int greenmax)
          {
              if (pixel + rdevi > 255)
              {
                  greenmax = 255;
              }
              else
              {
                  greenmax = pixel + rdevi;
              }

              if (pixel - rdevi > 0)
              {
                  greenmin = pixel - rdevi;
              }
              else
              {
                  greenmin = 0;
              }
          }

          private void ProcessBlue(int pixel, out int Bluemin, out int Bluemax)
          {
              if (pixel + rdevi > 255)
              {
                  Bluemax = 255;
              }
              else
              {
                  Bluemax = pixel + rdevi;
              }

              if (pixel - rdevi > 0)
              {
                  Bluemin = pixel - rdevi;
              }
              else
              {
                  Bluemin = 0;
              }
          }

          public Boolean RedRange(int newpixel, int redmin, int redmax)
          {
              if (newpixel > redmin && redmax > newpixel)
              {
                  return true;
              }
              else
              {
                  return false;
              }
          }

          public Boolean GreenRange(int newpixel, int Greenmin, int Greenmax)
          {
              if (newpixel > Greenmin && Greenmax > newpixel)
              {
                  return true;
              }
              else
              {
                  return false;
              }
          }

          public Boolean BlueRange(int newpixel, int bluemin, int bluemax)
          {
              if (newpixel > bluemin && bluemax > newpixel)
              {
                  return true;
              }
              else
              {
                  return false;
              }
          }

          private void ProcessImage()
          {
              //Converting loaded image into bitmap
              Bitmap bmp = new Bitmap("C:\\Users\\Hackathon\\Pictures\\kinect" + (photoindex-1) +".png");
              int found = 0;
              //Iterate whole bitmap to findout the picked color
              for (int i = 0; i < bmp.Height; i++)
              {
                  for (int j = 0; j < bmp.Width; j++)
                  {
                      //Get the color at each pixel
                      System.Drawing.Color now_color = bmp.GetPixel(j, i);
                      bool R = RedRange(now_color.R, redmin, redmax);
                      bool G = GreenRange(now_color.G, greenmin, greenmax);
                      bool B = BlueRange(now_color.B, bluemin, bluemax);

                      if (R == true && B == true && G == true)
                      {
                          found = 1;
                          break;
                      }

                  }
              }
              if (found == 0)
              {
                  System.Windows.Forms.MessageBox.Show("No Match Found");
              }
              else
              {
                 System.Windows.Forms.MessageBox.Show("Color Match Found");
              }
          }   
   
      
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}
