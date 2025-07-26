//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Wave motion simulation. C# version                                                                          //
//  Written by Angel Rapallo 2007                                                                               //
//  I should have called this RAIN simulation, it makes more sense.                                             //
//                                                                                                              //
// ORIGINAL:                                                                                                    //
//  Original Algorithm by Don Pattee at http://www.x-caiver.com/Software and Christian Tratz.                   //
//  This version though still maintains at the core the original mathematical algorithm has                     //
//  been changed quite a bit. For one thing it out performs the previous versions and uses                      //
//  a class for FAST bitmap manipulation which uses UNSAFE C# and can be found at                               //
//  http://www.mdibb.net/net/faster_pixel_manipulation_with_getpixel_and_setpixel_in_net/                       //
//                                                                                                              //
// THIS:                                                                                                        //
//  Besides improving the original algorithm I also wrote it using different ARRAY structures                   //
//  JAGGED, RECTANGULAR,LINEAR etc... to see if there was anything to the Myth of Arrays, I found               //
//  there is really some difference but not what I had seen before and not as far as C# 2.0 is concerned.       //
//  I also wrote the algorithm using two techniques BUFFERED and UNBUFFERED rendering.                          //
//                                                                                                              //
//  Though I am sure this code will yield different results in different machines this is how I see it in my    //
//  personal Laptop Dell Inspiron 8500 running a Pentium 2.0 with 1 GYG of RAM.                                 //
//      #1 JAGGED      ARRAY USING BUGGER RENDERING                                                             //
//      #2 RECTANGULAR ARRAY USING BUGGER RENDERING                                                             //
//      #1 LINEAR      ARRAY USING BUGGER RENDERING                                                             //
//  Non buffered version ranked last. Thouggh I still think the FASTBitmap is a pretty good shot.               //
//                                                                                                              //
// MATH:                                                                                                        //
//  About the math, this algorithm is one among many, it is not the best out there but it is not the            //
//  worst. I have found other simpler algorithms written in java which perform better but the effects           //
//  are not that good. There are some websites out there which explain the math part i cant recall              //
//  the ones i read at the moment (i should have), but the idea is quite simple, a drop is placed               //
//  on the view by calculating some height based on the distance of the PIXEL from the center                   //
//  of the drop, then the motion algorithm kicks in, it calculates the height of the pixel                      //
//  using the height of the surrounding pixels and dampering the waves as it moves forward                      //
//  i have commented out places were you can play with different values, and the original versions              //
//  also have plenty of explanation. I think if you really want to learn water motion simulation                //
//  you should get a good book. This code uses COS() but it could as well use SIN() since one is the            //
//  same as the other shifted by 90 degrees. The original version also used COS().                              //
//                                                                                                              //
// CONDITIONS:                                                                                                  //
//  This code is posted with the same permission as the previous versions were, that is I maintain the          //
//  sharing conditions the previous developers who wrote the previous versions gave.                            //
//                                                                                                              //
// FEEDBACK:                                                                                                    //
//  I would like anyone who uses this code to notify me of anything that might proove interesting               //
//  and please dont forget the original contributors too, I ma sure they want to hear from you,                 //
//  though I could get an email for any of them at their websites.                                              //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Play with these to see what you get in your machine.
//
#define _BUFFERED_RENDERING

#define _JAGGED_ARRAYS
//#define _RECTANGULAR_ARRAYS
//#define _LINEAR_ARRAYS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Reflection.Emit;
using System.Reflection;

namespace WaveEffects
{
    public partial class Main : Form
    {
        WaveEffects waveEffects;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Image image = Image.FromFile("1.jpg");
            this.ClientSize = new Size(image.Width, image.Height);

            waveEffects = new WaveEffects(this, image);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            waveEffects.IsRunning = !waveEffects.IsRunning;
        }
    }
}