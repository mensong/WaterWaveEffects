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
        private struct DropData
        {
            public int x;
            public int y;
            public int radius;
            public int height;
        }

        private static int _BITMAP_WIDTH = 0;
        private static int _BITMAP_HEIGHT = 0;
        private static int _BITS = 4; /* Dont change this, it 24 bit bitmaps are not supported*/
#if _JAGGED_ARRAYS 
        private static int[][][] _waveHeight;
#endif
#if _RECTANGULAR_ARRAYS
        private static int[,,] _waveHeight;
#endif
#if _LINEAR_ARRAYS
        private static int[] _waveHeight;
#endif

        private static DropData[] _drops;
        private FastBitmap _image = null;
        private FastBitmap _originalImage = null;
        public  int _currentHeightBuffer = 0;
        public  int _newHeightBuffer = 0;
        private byte[] _bitmapOriginalBytes;
        private Random _r = new Random();

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.pbViewport.Top = 0;
            this.pbViewport.Left = 0;
            this.pbViewport.SizeMode = PictureBoxSizeMode.AutoSize;
            this.Width = this.pbViewport.Width;
            this.Height = this.pbViewport.Height;
            _BITMAP_WIDTH = this.pbViewport.Width;
            _BITMAP_HEIGHT = this.pbViewport.Height;            
            //
            //  
            //
#if _JAGGED_ARRAYS
            _waveHeight = new int[_BITMAP_WIDTH][][];
            for (int i = 0; i < _BITMAP_WIDTH; i++)
            {
                _waveHeight[i] = new int[_BITMAP_HEIGHT][];                
                for (int j = 0; j < _BITMAP_HEIGHT; j++)
                {
                    _waveHeight[i][j] = new int[2];
                }
            }
#endif
#if _RECTANGULAR_ARRAYS
            _waveHeight = new int[_BITMAP_WIDTH, _BITMAP_HEIGHT, 2];
#endif

#if _LINEAR_ARRAYS
            _waveHeight = new int[_BITMAP_WIDTH * _BITMAP_HEIGHT * 2];
#endif
            //
            //
            //            
            CreateBitmap();
            CreateWaterDrops();

            this.waterTime.Enabled = true;
            this.dropsTime.Interval = 50;
            this.dropsTime.Enabled = true;
        }

        private void CreateBitmap()
        {
            _originalImage = new FastBitmap((Bitmap)(this.pbViewport.Image).Clone(),_BITS);
            _originalImage.LockBits();
            _image = new FastBitmap((Bitmap)(this.pbViewport.Image).Clone(), _BITS);
            _bitmapOriginalBytes = new byte[_BITS * _image.Width() * _image.Height()];
            _image.LockBits();
            Marshal.Copy(_image.Data().Scan0, _bitmapOriginalBytes, 0, _bitmapOriginalBytes.Length);
            _image.Release();
        }

        private void DropWater(int x, int y,int radius, int height)
        {
            long _distance;
            int _x;
            int _y;            
            Single _ratio;

            _ratio = (Single)((Math.PI / (Single)radius));

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    _x = x + i;
                    _y = y + j;
                    if ((_x >= 0) && (_x <= _BITMAP_WIDTH - 1) && (_y >= 0) && (_y <= _BITMAP_HEIGHT - 1))
                    {
                        _distance = (long)Math.Sqrt(i * i + j * j);
                        if (_distance <= radius)
                        {
#if _JAGGED_ARRAYS
                            _waveHeight[_x][_y][_currentHeightBuffer] = (int)(height * Math.Cos((Single)_distance * _ratio));
#endif
#if _RECTANGULAR_ARRAYS
                            _waveHeight[_x,_y,_currentHeightBuffer] = (int)(height * Math.Cos((Single)_distance * _ratio));
#endif
#if _LINEAR_ARRAYS
                            _waveHeight[INDEX3D(_x, _y, _currentHeightBuffer)] = (int)(height * Math.Cos((Single)_distance * _ratio));
#endif
                        }
                    }
                }
            }
        }

        private void PaintWater()
        {
            _newHeightBuffer = (_currentHeightBuffer + 1) % 2;
            _image.LockBits();
#if _BUFFERED_RENDERING
            byte[] _bufferBits = new byte[_BITS * _image.Width() * _image.Height()];
            Marshal.Copy(_image.Data().Scan0,_bufferBits,0,_bufferBits.Length );
#endif
            //
            // 
            //
            int _offX;
            int _offY;
                     
            for (int _x = 1; _x < _BITMAP_WIDTH - 1; _x++)
            {
                for (int _y = 1; _y < _BITMAP_HEIGHT - 1; _y++)
                {
#if _JAGGED_ARRAYS
                    //
                    //  Simulate movement.
                    //
                    unchecked
                    {
                        _waveHeight[_x][_y][_newHeightBuffer] = ((
                            _waveHeight[_x - 1][_y][_currentHeightBuffer] +
                            _waveHeight[_x - 1][_y - 1][_currentHeightBuffer] +
                            _waveHeight[_x][_y - 1][_currentHeightBuffer] +
                            _waveHeight[_x + 1][_y - 1][_currentHeightBuffer] +
                            _waveHeight[_x + 1][_y][_currentHeightBuffer] +
                            _waveHeight[_x + 1][_y + 1][_currentHeightBuffer] +
                            _waveHeight[_x][_y + 1][_currentHeightBuffer] +
                            _waveHeight[_x - 1][_y + 1][_currentHeightBuffer]) >> 2)
                        - _waveHeight[_x][_y][_newHeightBuffer];
                    }
                    //
                    //  Dampenning.
                    //
                    _waveHeight[_x][_y][_newHeightBuffer] -= (_waveHeight[_x][_y][_newHeightBuffer] >> 5);
                    //
                    //
                    //
                    _offX = ((_waveHeight[_x - 1][_y][_newHeightBuffer] - _waveHeight[_x + 1][_y][_newHeightBuffer])) >> 3;
                    _offY = ((_waveHeight[_x][_y - 1][_newHeightBuffer] - _waveHeight[_x][_y + 1][_newHeightBuffer])) >> 3;
#endif
#if _RECTANGULAR_ARRAYS
                    unchecked
                    {
                        _waveHeight[_x,_y,_newHeightBuffer] = ((
                            _waveHeight[_x - 1,_y,_currentHeightBuffer] +
                            _waveHeight[_x - 1,_y - 1,_currentHeightBuffer] +
                            _waveHeight[_x,_y - 1,_currentHeightBuffer] +
                            _waveHeight[_x + 1,_y - 1,_currentHeightBuffer] +
                            _waveHeight[_x + 1,_y,_currentHeightBuffer] +
                            _waveHeight[_x + 1,_y + 1,_currentHeightBuffer] +
                            _waveHeight[_x,_y + 1,_currentHeightBuffer] +
                            _waveHeight[_x - 1,_y + 1,_currentHeightBuffer]) >> 2)
                        - _waveHeight[_x,_y,_newHeightBuffer];
                    }
                    //
                    //  Dampenning.
                    //
                    _waveHeight[_x,_y,_newHeightBuffer] -= (_waveHeight[_x,_y,_newHeightBuffer] >> 5);
                    //
                    //
                    //
                    _offX = ((_waveHeight[_x - 1,_y,_newHeightBuffer] - _waveHeight[_x + 1,_y,_newHeightBuffer])) >> 4;
                    _offY = ((_waveHeight[_x,_y - 1,_newHeightBuffer] - _waveHeight[_x,_y + 1,_newHeightBuffer])) >> 4;
#endif
#if _LINEAR_ARRAYS
                    unchecked
                    {
                        _waveHeight[INDEX3D(_x,_y, _newHeightBuffer)] = ((
                            _waveHeight[INDEX3D(_x - 1, _y + 0, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x - 1, _y - 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x - 0, _y - 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 1, _y - 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 1, _y + 0, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 1, _y + 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 0, _y + 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x - 1, _y + 1, _currentHeightBuffer)]) >> 2)
                        - _waveHeight[INDEX3D(_x, _y, _newHeightBuffer)];
                    }
                    //
                    //  Dampenning.
                    //
                    _waveHeight[INDEX3D(_x, _y, _newHeightBuffer)] -= (_waveHeight[INDEX3D(_x, _y, _newHeightBuffer)] >> 5);
                    //
                    //
                    //
                    _offX = ((_waveHeight[INDEX3D(_x - 1, _y - 0, _newHeightBuffer)] - _waveHeight[INDEX3D(_x + 1, _y + 0, _newHeightBuffer)])) >> 4;
                    _offY = ((_waveHeight[INDEX3D(_x + 0, _y - 1, _newHeightBuffer)] - _waveHeight[INDEX3D(_x + 0, _y + 1, _newHeightBuffer)])) >> 4;
#endif
                    //
                    //  Nothing to do
                    //
                    if ((_offX == 0) && (_offY == 0)) continue;
                    //
                    //  Fix boundaries
                    //
                    if (_x + _offX <= 0) _offX = -_x;
                    if (_x + _offX >= _BITMAP_WIDTH - 1) _offX = _BITMAP_WIDTH - _x - 1;
                    if (_y + _offY <= 0) _offY = -_y;
                    if (_y + _offY >= _BITMAP_HEIGHT - 1) _offY = _BITMAP_HEIGHT - _y - 1;                    
                    //
                    //  
                    //
#if _BUFFERED_RENDERING
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 0] = _bitmapOriginalBytes[_BITS * (_x + _offX + (_y + _offY) * _BITMAP_WIDTH) + 0];
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 1] = _bitmapOriginalBytes[_BITS * (_x + _offX + (_y + _offY) * _BITMAP_WIDTH) + 1];
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 2] = _bitmapOriginalBytes[_BITS * (_x + _offX + (_y + _offY) * _BITMAP_WIDTH) + 2];
                    // I dont not implement the ALPHA as previous version did. you can if you want.
                    //_bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 3] = alpha                    
#else
                    _image.SetPixel(_x, _y, _originalImage.GetPixel(_x + _offX, _y + _offY));
#endif
                }
            }
#if _BUFFERED_RENDERING
            Marshal.Copy(_bufferBits,0,_image.Data().Scan0, _bufferBits.Length);
#endif
            _currentHeightBuffer = _newHeightBuffer;
            this.Invalidate();
        }
        private void waterTime_Tick(object sender, EventArgs e)
        {
            if (_image.IsLocked()) return;
            waterTime.Stop();            
            PaintWater();                        
            waterTime.Start();
        }

        private void dropsTime_Tick(object sender, EventArgs e)
        {
            this.dropsTime.Enabled = false; 

            int _percent = (int)(0.005 * (this.Width + this.Height));
            int _dropsNumber = _r.Next(_percent);
            int _drop = 0;

            for (int i = 0; i < _dropsNumber; i++)
            {             
                _drop = _r.Next(_drops.Length);                
                DropWater(_drops[_drop].x, _drops[_drop].y, _drops[_drop].radius, _drops[_drop].height);
            }

            this.dropsTime.Interval = _r.Next(15*_percent)+1;
            this.dropsTime.Enabled = true;
        }

        private void CreateWaterDrops()
        {
            int _dropX;
            int _dropY;
            int _dropRadius;
            int _height;

            int _percent = (int)(0.0015 * (this.Width + this.Height));
            _drops = new DropData[100];

            for (int i = 0; i < _drops.Length; i++)
            {
                _dropX = _r.Next(_BITMAP_WIDTH);
                _dropY = _r.Next(_BITMAP_HEIGHT);
                _height = _r.Next(400);
                _dropRadius = _r.Next(4 * _percent);

                if (_dropRadius < 4) _dropRadius = 4;

                _drops[i].x = _dropX;
                _drops[i].y = _dropY;
                _drops[i].radius = _dropRadius;
                _drops[i].height = _height;
            }

        }

        private void pbViewport_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Main_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            _image.Release();
            e.Graphics.DrawImage(_image.Bitmap, 0, 0, _image.Width(), _image.Height());
        }

#if _LINEAR_ARRAYS
        private int INDEX3D(int x, int y, int z) { unchecked { return x * _BITMAP_HEIGHT * 2 + y * 2 + z; } }
#endif
    }
}