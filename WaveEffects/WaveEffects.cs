#define _BUFFERED_RENDERING

#define _JAGGED_ARRAYS
//#define _RECTANGULAR_ARRAYS
//#define _LINEAR_ARRAYS

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WaveEffects
{
    public class WaveEffects
    {
        Control _control;
        private System.Windows.Forms.Timer waterTime;
        private System.Windows.Forms.Timer dropsTime;

        bool _isStop = false;

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
#if !_BUFFERED_RENDERING
        private FastBitmap _originalImage = null;
#endif
        public int _currentHeightBuffer = 0;
        public int _newHeightBuffer = 0;
        private byte[] _bitmapOriginalBytes;
        private Random _r = new Random();


        public WaveEffects(Control control, System.Drawing.Image image)
        {
            _control = control;

            _BITMAP_WIDTH = image.Width;
            _BITMAP_HEIGHT = image.Height;

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
                    
            CreateBitmap(image);
            CreateWaterDrops();

            _control.Paint += new PaintEventHandler(control_Paint);

            this.waterTime = new System.Windows.Forms.Timer();
            this.dropsTime = new System.Windows.Forms.Timer();
            this.waterTime.Tick += new System.EventHandler(this.waterTime_Tick);
            this.dropsTime.Tick += new System.EventHandler(this.dropsTime_Tick);
            this.waterTime.Interval = 15;
            this.dropsTime.Interval = 50;
            this.waterTime.Enabled = true;
            this.dropsTime.Enabled = true;
        }

        public void Start()
        {
            _isStop = false;
            this.waterTime.Start();
            this.dropsTime.Start();
        }

        public void Stop()
        {
            _isStop = true;
            this.waterTime.Stop();
            this.dropsTime.Stop();
        }

        public bool IsRunning
        {
            get
            {
                return !_isStop;
            }
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        private void CreateBitmap(System.Drawing.Image image)
        {
#if !_BUFFERED_RENDERING
            _originalImage = new FastBitmap((Bitmap)image.Clone(), _BITS);
            _originalImage.LockBits();
#endif
            _image = new FastBitmap((Bitmap)image.Clone(), _BITS);
            _bitmapOriginalBytes = new byte[_BITS * _image.Width() * _image.Height()];
            _image.LockBits();
            Marshal.Copy(_image.Data().Scan0, _bitmapOriginalBytes, 0, _bitmapOriginalBytes.Length);
            _image.Release();
        }

        private void CreateWaterDrops()
        {
            int dropX;
            int dropY;
            int dropRadius;
            int height;

            //int percent = (int)(0.0015 * (this._control.Width + this._control.Height));
            int percent = (int)(0.0015 * (_BITMAP_WIDTH + _BITMAP_HEIGHT));
            _drops = new DropData[100];

            for (int i = 0; i < _drops.Length; i++)
            {
                dropX = _r.Next(_BITMAP_WIDTH);
                dropY = _r.Next(_BITMAP_HEIGHT);
                height = _r.Next(400);
                dropRadius = _r.Next(4 * percent);

                if (dropRadius < 4) dropRadius = 4;

                _drops[i].x = dropX;
                _drops[i].y = dropY;
                _drops[i].radius = dropRadius;
                _drops[i].height = height;
            }

        }

        private void waterTime_Tick(object sender, EventArgs e)
        {
            if (_isStop) return;
            if (_image.IsLocked()) return;
            waterTime.Stop();
            PaintWater(this._control);
            this.waterTime.Start();
        }

        private void dropsTime_Tick(object sender, EventArgs e)
        {
            if (_isStop) return;
            this.dropsTime.Enabled = false;
            int percent = (int)(0.005 * (_BITMAP_WIDTH + _BITMAP_HEIGHT));
            int dropsNumber = _r.Next(percent);
            int drop = 0;

            for (int i = 0; i < dropsNumber; i++)
            {
                drop = _r.Next(_drops.Length);
                DropWater(_drops[drop].x, _drops[drop].y, _drops[drop].radius, _drops[drop].height);
            }

            this.dropsTime.Interval = _r.Next(15 * percent) + 1;
            this.dropsTime.Enabled = true;
        }

        private void PaintWater(Control waterControl)
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
            int offX;
            int offY;

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
                    offX = ((_waveHeight[_x - 1][_y][_newHeightBuffer] - _waveHeight[_x + 1][_y][_newHeightBuffer])) >> 3;
                    offY = ((_waveHeight[_x][_y - 1][_newHeightBuffer] - _waveHeight[_x][_y + 1][_newHeightBuffer])) >> 3;
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
                    if ((offX == 0) && (offY == 0)) continue;
                    //
                    //  Fix boundaries
                    //
                    if (_x + offX <= 0) offX = -_x;
                    if (_x + offX >= _BITMAP_WIDTH - 1) offX = _BITMAP_WIDTH - _x - 1;
                    if (_y + offY <= 0) offY = -_y;
                    if (_y + offY >= _BITMAP_HEIGHT - 1) offY = _BITMAP_HEIGHT - _y - 1;
                    //
                    //  
                    //
#if _BUFFERED_RENDERING
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 0] = _bitmapOriginalBytes[_BITS * (_x + offX + (_y + offY) * _BITMAP_WIDTH) + 0];
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 1] = _bitmapOriginalBytes[_BITS * (_x + offX + (_y + offY) * _BITMAP_WIDTH) + 1];
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 2] = _bitmapOriginalBytes[_BITS * (_x + offX + (_y + offY) * _BITMAP_WIDTH) + 2];
                    // I dont not implement the ALPHA as previous version did. you can if you want.
                    //_bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 3] = alpha;
#else
                    _image.SetPixel(_x, _y, _originalImage.GetPixel(_x + _offX, _y + _offY));
#endif
                }
            }
#if _BUFFERED_RENDERING
            Marshal.Copy(_bufferBits,0,_image.Data().Scan0, _bufferBits.Length);
#endif
            _currentHeightBuffer = _newHeightBuffer;
            waterControl.Invalidate();
        }

        private void DropWater(int x, int y, int radius, int height)
        {
            long distance;
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
                        distance = (long)Math.Sqrt(i * i + j * j);
                        if (distance <= radius)
                        {
#if _JAGGED_ARRAYS
                            _waveHeight[_x][_y][_currentHeightBuffer] = (int)(height * Math.Cos((Single)distance * _ratio));
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
#if _LINEAR_ARRAYS
        private int INDEX3D(int x, int y, int z) { unchecked { return x * _BITMAP_HEIGHT * 2 + y * 2 + z; } }
#endif

        void control_Paint(object sender, PaintEventArgs e)
        {
            _image.Release();
            e.Graphics.DrawImage(_image.Bitmap, 0, 0, _image.Width(), _image.Height());
        }

    }
}
