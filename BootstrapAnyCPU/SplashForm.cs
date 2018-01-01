//
//      FILE:   SplashForm.cs
//
// COPYRIGHT:   Copyright 2010 
//              Infralution Pty Ltd
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Bootstrap.Properties;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Bootstrap
{
    /// <summary>
    /// Defines the splash screen for the application
    /// </summary>
    public partial class SplashForm : Form
    {
        #region Member Variables

        /// <summary>
        /// The number of steps over which the form is faded
        /// </summary>
        private const int NUM_FADE_STEPS = 32;

        /// <summary>
        /// The change in opacity for each fade step
        /// </summary>
        private const byte OPACITY_STEP = 256 / NUM_FADE_STEPS;

        /// <summary>
        /// Triggered once the thread has started and initialized the splash form
        /// </summary>
        private static ManualResetEvent _threadStartEvent = new ManualResetEvent(false); 

        /// <summary>
        /// The active instance of the splash form
        /// </summary>
        private static SplashForm _instance;

        /// <summary>
        /// The total time in milliseconds that the fade in effect should take
        /// </summary>
        private static int _fadeInTime;

        /// <summary>
        /// The total time in milliseconds that the fade out effect should take
        /// </summary>
        private static int _fadeOutTime;

        /// <summary>
        /// The current bitmap opacity
        /// </summary>
        private int _bitmapOpacity = 0;

        /// <summary>
        /// The alpha blended bitmap that is displayed as the splash
        /// </summary>
        private Bitmap _bitmap;

        /// <summary>
        /// The handle to the bitmap
        /// </summary>
        private IntPtr _hBitmap = IntPtr.Zero;

        #endregion

        #region Public Interface

        /// <summary>
        /// Display an instance of the splash screen (using a separate thread)
        /// </summary>
        /// <param name="fadeInTime">Time in milliseconds over which the splash will fade in</param>
        /// <param name="fadeOutTime">Time in milliseconds over which the splash will fade out (after 
        /// the main application completes loading)</param>
        public static void DisplaySplash(int fadeInTime, int fadeOutTime)
        {
            if (fadeInTime < 0) throw new ArgumentOutOfRangeException("fadeInTime");
            if (fadeOutTime < 0) throw new ArgumentOutOfRangeException("fadeOutTime");
            _fadeInTime = fadeInTime;
            _fadeOutTime = fadeOutTime;

            System.Windows.Forms.Application.Idle += new EventHandler(OnApplicationIdle);
#if SUPPORT_WPF
            System.Windows.Interop.ComponentDispatcher.ThreadIdle += new EventHandler(OnApplicationIdle);
#endif
            Thread thread = new Thread(new ThreadStart(SplashThread));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _threadStartEvent.WaitOne();
        }

        /// <summary>
        /// Forcibly closes the splash screen
        /// </summary>
        public static void CloseSplash()
        {
            System.Windows.Forms.Application.Idle -= new EventHandler(OnApplicationIdle);
#if SUPPORT_WPF
            System.Windows.Interop.ComponentDispatcher.ThreadIdle -= new EventHandler(OnApplicationIdle);
#endif
            SplashForm instance = _instance;
            if (instance != null)
            {
                instance.BeginInvoke(new MethodInvoker(instance.StartFadeOut));
            }
        }

        #endregion

        #region Windows API

        private enum GetWindowCmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        private const int GWL_STYLE = -16;
        private const UInt32 WS_VISIBLE = 0x10000000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        private const Int32 ULW_ALPHA = 0x00000002;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        #endregion

        #region Local Methods

        /// <summary>
        /// Create a new instance of the splash screen
        /// </summary>
        private SplashForm()
        {
            InitializeComponent();
            _bitmap = new Bitmap(BackgroundImage);
            _hBitmap = _bitmap.GetHbitmap(Color.FromArgb(0));
            ClientSize = _bitmap.Size;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                _bitmap.Dispose();
                if (_hBitmap != IntPtr.Zero)
                {
                    DeleteObject(_hBitmap);
                }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Set the window ExStyle - must be WS_EX_LAYERED to support alpha blended images
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cp;
            }
        }

        /// <summary>
        /// Update the background image when the opacity changes
        /// </summary>
        private void UpdateBackgroundImage()
        {
            // Create a memory DC for the bitmap
            //
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr oldBitmap = IntPtr.Zero;
           
            // Select the bitmap into the memory DC
            //
            oldBitmap = SelectObject(memDc, _hBitmap);

            // Setup the blending options
            //
            BLENDFUNCTION blend = new BLENDFUNCTION();
            blend.BlendOp = AC_SRC_OVER;
            blend.BlendFlags = 0;
            if (_bitmapOpacity < 0)
                blend.SourceConstantAlpha = 0;
            else if (_bitmapOpacity > 255)
                blend.SourceConstantAlpha = 255;
            else
                blend.SourceConstantAlpha = (byte)_bitmapOpacity;
            blend.AlphaFormat = AC_SRC_ALPHA;

            Size size = _bitmap.Size;
            Point pointSrc = new Point(0, 0);
            Point pointDst = new Point(this.Left, this.Top);
            UpdateLayeredWindow(this.Handle, screenDc, ref pointDst, ref size, memDc, ref pointSrc, 0, ref blend, ULW_ALPHA);

            // cleanup
            //
            ReleaseDC(IntPtr.Zero, screenDc);
            if (oldBitmap != IntPtr.Zero)
            {
                SelectObject(memDc, oldBitmap);
            }
            DeleteDC(memDc);
        }

        /// <summary>
        /// The thread to run the splash form
        /// </summary>
        private static void SplashThread()
        {
            _instance = new SplashForm();
            _threadStartEvent.Set();
            Application.Run(_instance);
        }

        /// <summary>
        /// Wait until the main application loop is idle before we close the splash form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationIdle(object sender, EventArgs e)
        {
            CloseSplash();
        }

        /// <summary>
        /// Start the close timer
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int interval = _fadeInTime / NUM_FADE_STEPS;
            if (interval == 0)
            {
                _bitmapOpacity = 255;
                UpdateBackgroundImage();
            }
            else
            {
                _fadeInTimer.Interval = interval;
                _fadeInTimer.Start();
            }
        }

        /// <summary>
        /// Start the fade out timer 
        /// </summary>
        private void StartFadeOut()
        {
            _fadeInTimer.Stop();
            _fadeOutTimer.Start();
            int interval = _fadeOutTime / NUM_FADE_STEPS;
            if (interval == 0)
            {
                this.Close();
            }
            else
            {
                _fadeOutTimer.Interval = interval;
                _fadeOutTimer.Start();
            }
        }


        /// <summary>
        /// Change the opacity of the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _fadeInTimer_Tick(object sender, EventArgs e)
        {
            if (_bitmapOpacity < 255)
            {
                _bitmapOpacity += OPACITY_STEP;
                UpdateBackgroundImage();
            }
            else
            {
                _fadeInTimer.Stop();
            }
        }

        /// <summary>
        /// Find the main application window
        /// </summary>
        /// <returns>The next visible window in the Z-Order belonging to the process</returns>
        private IntPtr FindMainWindow()
        {
            int thisProcessId = Process.GetCurrentProcess().Id;
        
            IntPtr hWindow = _instance.Handle;
            hWindow = GetWindow(hWindow, GetWindowCmd.GW_HWNDNEXT);
            while (hWindow != IntPtr.Zero)
            {
                uint processId = 0;
                GetWindowThreadProcessId(hWindow, out processId);
                if (processId == thisProcessId)
                {
                    int style = GetWindowLong(hWindow, GWL_STYLE);
                    if ((style & WS_VISIBLE) == WS_VISIBLE)
                    {
                        return hWindow;
                    }
                }
                hWindow = GetWindow(hWindow, GetWindowCmd.GW_HWNDNEXT);
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Set the main window of the application as the foreground - if we don't do 
        /// this then we get some strange z-order issues when the splash screen closes   
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            // Set the main window of the application as the foreground - if we don't do 
            // this then we get some strange z-order issues when the splash screen closes
            //
            IntPtr mainWindow = FindMainWindow();
            if (mainWindow != IntPtr.Zero)
            {
                SetForegroundWindow(mainWindow);
            }
            base.OnClosed(e);
            _instance = null;
        }

        /// <summary>
        /// Fade the form out and then close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _fadeOutTimer_Tick(object sender, EventArgs e)
        {
            _bitmapOpacity -= OPACITY_STEP;
            if (_bitmapOpacity <= 0)
            {
                this.Close();
            }
            else
            {
                UpdateBackgroundImage();
            }
        }

        #endregion
    }
}