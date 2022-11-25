using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SleepWorm
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Running = true;
        private int MaxSpan = 60;
        private uint PointX;
        private uint PointY;
        private Point LastPoint;
        private DateTime LastTime = DateTime.Now;

        public MainWindow()
        {
            InitializeComponent();

            PointX = (uint)System.Windows.SystemParameters.PrimaryScreenWidth / 2 + 100;
            PointY = (uint)System.Windows.SystemParameters.PrimaryScreenHeight;

            Task.Run(Timer);
        }

        private void Topper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void StopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (StopButton.Opacity == 0.5)
                return;

            Running = false;
            StartButton.Opacity = 1;
            StopButton.Opacity = 0.5;
            MainTextBox.Opacity = 1;
        }

        private void StartButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (StartButton.Opacity == 0.5)
                return;

            LastTime = DateTime.Now;
            if (MainTextBox.Text == "")
                MainTextBox.Text = "9";
            MainLabel.Content = MainTextBox.Text + " минут";
            MaxSpan = Convert.ToInt32(MainTextBox.Text) * 60;

            Running = true;
            StartButton.Opacity = 0.5;
            StopButton.Opacity = 1;
            MainTextBox.Opacity = 0;
        }

        private void MainTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                Convert.ToInt32(MainTextBox.Text);
            }
            catch
            {
                MainTextBox.Text = "";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(EXECUTION_STATE esFlags);

        [Flags]
        private enum EXECUTION_STATE
        {
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);

            return lpPoint;
        }

        private void Timer()
        {
            TimeSpan time;
            Random r = new Random();
            LastPoint = GetCursorPosition();

            while (true)
            {
                if (Running)
                {                    
                    if (GetCursorPosition() != LastPoint)
                    {
                        LastPoint = GetCursorPosition();
                        LastTime = DateTime.Now;
                    }

                    time = DateTime.Now - LastTime;

                    if (time.TotalSeconds >= MaxSpan)
                    {
                        SetCursorPos((int)PointX, r.Next((int)PointY - 20, (int)PointY));
                        mouse_event((uint)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                        mouse_event((uint)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
                        SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
                        LastTime = DateTime.Now;
                    }

                    this.Dispatcher.Invoke(() => TimerLabel.Content = time.Minutes + "'" + time.Seconds + "''");

                    Thread.Sleep(200);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
