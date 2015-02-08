using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChameleonAPI
{
    public partial class histagram
    {    
        

        public bool histogram(System.Drawing.Bitmap bmp,int R, int G, int B, int rdev, int gdev, int bdev)
        {
           
           int Red = Convert.ToInt32(R);
           int  Green = Convert.ToInt32(G);
           int  Blue = Convert.ToInt32(B);
           int rdevi = Convert.ToInt32(rdev);
           int  gdevi = Convert.ToInt32(gdev);
           int  bdevi = Convert.ToInt32(bdev);
           int redmin, redmax, greenmin, greenmax, bluemin, bluemax;
           
             ProcessRed(Red,rdevi, out redmin, out redmax);
             ProcessGreen(Red,gdevi, out greenmin, out greenmax);
             ProcessBlue(Red,bdevi, out bluemin, out bluemax);
            return (ProcessImage(bmp,redmin,redmax,greenmin,greenmax,bluemin,bluemax));

        }

        public void ProcessRed(int pixel,int rdevi, out int redmin, out int redmax)
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

        private void ProcessGreen(int pixel,int rdevi, out int greenmin, out int greenmax)
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

        private void ProcessBlue(int pixel,int rdevi, out int Bluemin, out int Bluemax)
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


        private bool ProcessImage(System.Drawing.Bitmap bmp,int redmin,int redmax,int greenmin,int greenmax,int bluemin,int bluemax)
        {
            //Converting loaded image into bitmap
            
            int found = 0;
            //Iterate whole bitmap to findout the picked color
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    //Get the color at each pixel
                    Color now_color = bmp.GetPixel(j, i);
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
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
