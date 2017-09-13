#define debug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tetris.Classes;

namespace Tetris
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random;

        Game game;

        public MainWindow()
        {
            InitializeComponent();

            random = new Random((int)DateTime.Now.Ticks);

            InitGame();
        }

        // 최초 화면 그리기
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            DrawCanvas(null);
            DrawNext(null);
            DrawHold(null);

            base.OnRenderSizeChanged(sizeInfo);
        }

        private void DrawNext(Classes.Block nextBlock)
        {
            canvasNext.Children.Clear();

            for (int i = 0; i < canvasNext.ActualWidth; i += 9)
            {
                for (int j = 0; j < canvasNext.ActualHeight; j += 9)
                {
                    Rectangle rect = new Rectangle()
                    {
                        Width = 10,
                        Height = 10,
                        Stroke = new SolidColorBrush(Colors.Gray)
                    };

                    Canvas.SetLeft(rect, i);
                    Canvas.SetTop(rect, j);

                    canvasNext.Children.Add(rect);
                }
            }

            if (nextBlock != null)
            {
                for (int bx = 0; bx < nextBlock.Shape.GetLength(1); ++bx)
                {
                    for (int by = 0; by < nextBlock.Shape.GetLength(0); ++by)
                    {
                        if (nextBlock.Shape[by, bx] == 0)
                            continue;

                        int x = (2 + bx) * 9;
                        int y = (1 + by) * 9;

                        Rectangle rect = new Rectangle()
                        {
                            Width = 9,
                            Height = 9,
                            Fill = new SolidColorBrush(nextBlock.Color)
                        };

                        Canvas.SetLeft(rect, x);
                        Canvas.SetTop(rect, y);

                        canvasNext.Children.Add(rect);
                    }
                }
            }
        }

        private void DrawHold(Classes.Block holdBlock)
        {
            canvasHold.Children.Clear();

            for (int i = 0; i < canvasHold.ActualWidth; i += 9)
            {
                for (int j = 0; j < canvasHold.ActualHeight; j += 9)
                {
                    Rectangle rect = new Rectangle()
                    {
                        Width = 10,
                        Height = 10,
                        Stroke = new SolidColorBrush(Colors.Gray)
                    };

                    Canvas.SetLeft(rect, i);
                    Canvas.SetTop(rect, j);

                    canvasHold.Children.Add(rect);
                }
            }

            if (holdBlock != null)
            {
                for (int bx = 0; bx < holdBlock.Shape.GetLength(1); ++bx)
                {
                    for (int by = 0; by < holdBlock.Shape.GetLength(0); ++by)
                    {
                        if (holdBlock.Shape[by, bx] == 0)
                            continue;

                        int x = (2 + bx) * 9;
                        int y = (1 + by) * 9;

                        Rectangle rect = new Rectangle()
                        {
                            Width = 9,
                            Height = 9,
                            Fill = new SolidColorBrush(holdBlock.Color)
                        };

                        Canvas.SetLeft(rect, x);
                        Canvas.SetTop(rect, y);

                        canvasHold.Children.Add(rect);
                    }
                }
            }
        }

        private void DrawCanvas(Classes.Block b)
        {
            bool[,] map = game.Board.Map;

            canvas.Children.Clear();

            for (int i = 0; i < canvas.ActualWidth; i += 20)
            {
                for (int j = 0; j < canvas.ActualHeight; j += 20)
                {
                    Rectangle rect = new Rectangle();
                    if (map[j / 20 + 2, i / 20])
                    {
                        rect.Width = 20;
                        rect.Height = 20;
                        rect.Fill = new SolidColorBrush(
                                Colors.Gray
                            );
                    }
                    else
                    {
                        rect.Width = 21;
                        rect.Height = 21;
                        rect.Stroke = new SolidColorBrush(Colors.Gray);
                    }

                    Canvas.SetLeft(rect, i);
                    Canvas.SetTop(rect, j);

                    canvas.Children.Add(rect);
                }
            }

            foreach (var block in game.Board.BlockList)
            {
                for (int bx = 0; bx < block.Shape.GetLength(1); bx++)
                {
                    for (int by = 0; by < block.Shape.GetLength(0); by++)
                    {
                        if (block.Shape[by, bx] == 0)
                            continue;

                        Rectangle rect = new Rectangle()
                        {
                            Width = 20,
                            Height = 20,
                            Fill = new SolidColorBrush(block.Color)
                        };

                        Canvas.SetLeft(rect, (block.X + bx) * 20);
                        Canvas.SetTop(rect, (block.Y + by - 2) * 20);

                        canvas.Children.Add(rect);
                    }
                }


            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (game.IsPlaying && e.Key == Key.P)
            {
                if (game.IsPaused)
                {
                    labelPause.Visibility = Visibility.Hidden;
                    game.Resume();
                }
                else
                {
                    labelPause.Visibility = Visibility.Visible;
                    game.Pause();
                }
            }

            game.Controller.InputKey = e.Key;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            //game.Controller.InputKey = null;
        }

        bool started = false;
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            if (!started)
                Start();
            else
                Stop();
        }

        private void InitGame()
        {
            labelPause.Visibility = Visibility.Hidden;
            labelGameover.Visibility = Visibility.Hidden;

            ClearCanvases();
            
            game = new Game();
            game.Board = new Board(10, 15);
            game.Controller = new Controller();

            DrawCanvas(null);
            DrawNext(null);
            DrawHold(null);

            game.DrawEvent += DrawCanvas;
            game.DrawNext += DrawNext;
            game.DrawHold += DrawHold;

            game.ScoreUpdated = (score) =>
            {
                tbScore.Text = score.ToString();
            };

            game.GameOver = () =>
            {
                labelGameover.Visibility = Visibility.Visible;
                Stop();
            };
        }

        private void ClearCanvases()
        {
            canvas.Children.Clear();
            canvasHold.Children.Clear();
            canvasNext.Children.Clear();
        }

        OutlinedTextBlock labelStart;
        private void AnimateStartLabel()
        {
            labelStart = new OutlinedTextBlock()
            {
                FontWeight = FontWeights.ExtraBold,
                FontSize = 36,
                TextWrapping = TextWrapping.Wrap,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.DeepSkyBlue),
                Fill = new SolidColorBrush(Colors.AliceBlue),
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, -30, 0, 0),
                Text = "게임 시작!"
            };

            rootGrid.Children.Add(labelStart);

            OutlinedTextBlock _labelStart = labelStart;

            Task.Run(() =>
            {
                _labelStart.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _labelStart.Visibility = Visibility.Visible;

                    ThicknessAnimation animTop = new ThicknessAnimation();
                    animTop.From = _labelStart.Margin;

                    Thickness toMargin = new Thickness()
                    {
                        Left = _labelStart.Margin.Left,
                        Top = _labelStart.Margin.Top + canvas.ActualHeight / 2,
                        Right = _labelStart.Margin.Right,
                        Bottom = _labelStart.Margin.Bottom
                    };

                    animTop.To = toMargin;
                    animTop.AccelerationRatio = 0.0;
                    animTop.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                    animTop.FillBehavior = FillBehavior.HoldEnd;

                    _labelStart.BeginAnimation(Grid.MarginProperty, animTop);
                }));
                
            });

            Task.Delay(1000).ContinueWith((dummy) =>
            {
                _labelStart.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ThicknessAnimation animLeft = new ThicknessAnimation();
                    animLeft.From = _labelStart.Margin;

                    Thickness toMargin = new Thickness()
                    {
                        Left = -500,
                        Top = _labelStart.Margin.Top,
                        Right = _labelStart.Margin.Right,
                        Bottom = _labelStart.Margin.Bottom
                    };

                    animLeft.To = toMargin;
                    animLeft.AccelerationRatio = 0.0;
                    animLeft.Duration = new Duration(TimeSpan.FromMilliseconds(500));
                    animLeft.FillBehavior = FillBehavior.HoldEnd;
                    _labelStart.BeginAnimation(Grid.MarginProperty, animLeft);
                }));
            });

            Task.Delay(2000).ContinueWith((dummy) =>
            {
                _labelStart.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _labelStart.BeginAnimation(Grid.MarginProperty, null);

                    _labelStart.Visibility = Visibility.Hidden;

                    //labelStart.Margin = new Thickness(0, -30, 0, 0);
                    _labelStart.HorizontalAlignment = HorizontalAlignment.Center;
                }));
            });

        }

        private void Start()
        {
            AnimateStartLabel();

            InitGame();
            game.Start();
            btnStart.Content = "게임 종료";
            started = true;
        }

        private void Stop()
        {
            //labelStart.Visibility = Visibility.Hidden;
            //labelStart.BeginAnimation(MarginProperty, null);
            if (labelStart != null)
            {
                rootGrid.Children.Remove(labelStart);
            }

            game.Stop();
            btnStart.Content = "게임 시작";
            started = false;
        }

        private void HelpClicked(object sender, RoutedEventArgs e)
        {
            HelpWindow window = new HelpWindow();
            window.ShowDialog();
                       
        }

        private void ThemeClicked(object sender, RoutedEventArgs e)
        {
            ThemeWindow window = new ThemeWindow();
            bool? result = window.ShowDialog();

            if (result != null && result == true)
            {
                var color = window.Result;
                rootGrid.Background = new SolidColorBrush(color);
            }
        }

        private void MultiClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
