using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tetris
{

    public class GdiWrapper
    {
        [DllImport("gdi32.dll")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern int GetWindowDC(int hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(int hwnd, int hDC);
        
        public static Color GetPixelColor(Point point)
        {
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);

            ReleaseDC(0, lDC);

            byte a = (byte)((intColor >> 0x18) & 0xffL);
            byte b = (byte)((intColor >> 0x10) & 0xffL);
            byte g = (byte)((intColor >> 8) & 0xffL);
            byte r = (byte)(intColor & 0xffL);

            return Color.FromRgb(r, g, b);
        }
    }
    /// <summary>
    /// ThemeWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ThemeWindow : Window
    {
        

        public Color Result { get; private set; }


        private Ellipse el;
        private Point pos;

        public ThemeWindow()
        {
            InitializeComponent();

            picker.MouseDown += Picker_MouseDown;
            picker.MouseMove += Picker_MouseMove;
            picker.MouseUp += Picker_MouseUp;
        }

        bool pickMode = false;

        private void Picker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            pickMode = false;
        }

        private void Picker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pickMode = true;

            SelectColor(sender);
        }

        private void Picker_MouseMove(object sender, MouseEventArgs e)
        {
            if (!pickMode)
                return;

            SelectColor(sender);
        }

        private void SelectColor(object sender)
        {
            if (el != null)
            {
                picker.Children.Remove(el);
            }

            pos = Mouse.GetPosition((Canvas)sender);

            el = new Ellipse()
            {
                Margin = new Thickness(pos.X - 2, pos.Y - 2, 0, 0),
                Width = 4,
                Height = 4,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 0.5
            };

            picker.Children.Add(el);
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            pos = picker.PointToScreen(pos);
            Result = GdiWrapper.GetPixelColor(pos);

            DialogResult = true;
        }
    }
}
