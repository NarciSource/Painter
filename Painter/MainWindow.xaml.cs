﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Png Image|*.png";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName == "")
                saveFileDialog.FileName = "image.png";

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)canvas.RenderSize.Width,
                (int)canvas.RenderSize.Height,
                96d, 96d, System.Windows.Media.PixelFormats.Default);

            rtb.Render(canvas);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));


            using (var fs = System.IO.File.OpenWrite(saveFileDialog.FileName))
            {
                pngEncoder.Save(fs);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (canvas.Children.Count > 0)
            {
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
            }
            undo_times.Content = canvas.Children.Count.ToString();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Autor : Jeong Won-Cheol\rData : 2018-05-19","About");
        }






        enum Painting_Mode {
            Pencil,
            Line,
            Rectangle,
            Filed_Rectangle,
            Eraser,
            FullFill
        };
        Painting_Mode painting_mode = Painting_Mode.Pencil;

        Point start_point, current_point;
        bool is_painting = false;
        int history_pivot = 0;




        private void Mouse_Left_Down(object sender, MouseButtonEventArgs e)
        {
            start_point = e.GetPosition(canvas);
            current_point = e.GetPosition(canvas);

            is_painting = true;
            history_pivot = canvas.Children.Count;

            
            


            if (painting_mode == Painting_Mode.FullFill)
            {                
                Point targetPoint = e.GetPosition(canvas);
                Color targetColor = Get_Color(targetPoint);
                Color replaceColor = front_color;
                
                Stack<Point> pixels = new Stack<Point>();
                pixels.Push(targetPoint);

                PointCollection selected_pixels = new PointCollection();

                while (pixels.Count() > 0)
                {
                    Point point = pixels.Pop();

                    if (point.X < (int)canvas.ActualWidth && point.X > 0 &&
                        point.Y < (int)canvas.ActualHeight && point.Y > 0)//make sure we stay within bounds
                    {
                        if (Get_Color(point) == targetColor)
                        {
                            
                            pixels.Push(new Point(point.X - 1, point.Y));
                            pixels.Push(new Point(point.X + 1, point.Y));
                            pixels.Push(new Point(point.X, point.Y - 1));
                            pixels.Push(new Point(point.X, point.Y + 1));
                            Set_Color(point, replaceColor);
                            /*
                            Ellipse currentDot = new Ellipse();
                            currentDot.Stroke = new SolidColorBrush(Colors.Green);
                            currentDot.StrokeThickness = 1;
                            currentDot.Height = 1;
                            currentDot.Width = 1;
                            currentDot.Fill = new SolidColorBrush(Colors.Green);
                            currentDot.Margin = new Thickness(point.X, point.Y, 0, 0); // Sets the position.
                            canvas.Children.Add(currentDot);
                            */
                        }
                    }
                }
                
            }
        }

        private void Mouse_Left_Up(object sender, MouseButtonEventArgs e)
        {
            is_painting = false;
        }



        

        PointCollection pencil_line = new PointCollection();
        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            Refresh_Status(e.GetPosition(canvas), Get_Color(e.GetPosition(canvas)), canvas.Children.Count);


            Shape shape=null;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Capture_Canvas();
                switch (painting_mode)
                {
                    case Painting_Mode.Pencil:                        
                        pencil_line.Add(e.GetPosition(canvas));

                        shape = new Polyline()
                        {
                            Stroke = new SolidColorBrush(front_color),
                            StrokeThickness = thickness,
                            Points = pencil_line.Clone()
                        };
                        break;

                    case Painting_Mode.Eraser:
                        pencil_line.Add(e.GetPosition(canvas));

                        shape = new Polyline()
                        {
                            Stroke = new SolidColorBrush(back_color),
                            StrokeThickness = 30,
                            Points = pencil_line.Clone()
                        };
                        break;

                    case Painting_Mode.Line:
                        shape = new Line()
                        {
                            Stroke = new SolidColorBrush(front_color),
                            StrokeThickness = thickness,
                            X1 = start_point.X,
                            Y1 = start_point.Y,
                            X2 = e.GetPosition(canvas).X,
                            Y2 = e.GetPosition(canvas).Y
                        };
                        break;

                    case Painting_Mode.Filed_Rectangle:
                    case Painting_Mode.Rectangle:
                        shape = new Rectangle()
                        {
                            Stroke = new SolidColorBrush(front_color),
                            StrokeThickness = thickness,
                            Width = Math.Abs(start_point.X - e.GetPosition(canvas).X),
                            Height = Math.Abs(start_point.Y - e.GetPosition(canvas).Y),
                            Margin = new Thickness(Math.Min(start_point.X, e.GetPosition(canvas).X),
                                                    Math.Min(start_point.Y, e.GetPosition(canvas).Y), 0, 0)
                        };

                        if (painting_mode == Painting_Mode.Filed_Rectangle)
                            shape.Fill = new SolidColorBrush(back_color);
                        break;
                }

                try
                {
                    if (is_painting)
                        for (int i = history_pivot; i < canvas.Children.Count; i++)
                            canvas.Children.RemoveAt(i);
                
                    canvas.Children.Add(shape);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
                
            }
            else
            {
                pencil_line.Clear();
            }
        }





        bool color_toggle = true;
        Color front_color = Colors.Black;
        Color back_color = Colors.White;
        

        private void Front_Color_Button(object sender, RoutedEventArgs e)
        {
            frontColorCap.Background = new SolidColorBrush(Colors.LightSkyBlue);
            backColorCap.Background = new SolidColorBrush(Colors.White);
            color_toggle = true;
        }
        
        private void Back_Color_Button(object sender, RoutedEventArgs e)
        {
            backColorCap.Background = new SolidColorBrush(Colors.LightSkyBlue);
            frontColorCap.Background = new SolidColorBrush(Colors.White);
            color_toggle = false;
        }

        private void Black_Click(object sender, RoutedEventArgs e)
        {
            if (color_toggle)
            {
                frontColor.Background = new SolidColorBrush(Colors.Black);
                front_color = Colors.Black;
            }
            else
            {
                backColor.Background = new SolidColorBrush(Colors.Black);
                back_color = Colors.Black;
            }
        }

        private void Gray_Click(object sender, RoutedEventArgs e)
        {
            if (color_toggle)
            {
                frontColor.Background = new SolidColorBrush(Colors.Gray);
                front_color = Colors.Gray;
            }
            else
            {
                backColor.Background = new SolidColorBrush(Colors.Gray);
                back_color = Colors.Gray;
            }
        }

        private void Red_Click(object sender, RoutedEventArgs e)
        {
            if (color_toggle)
            {
                frontColor.Background = new SolidColorBrush(Colors.Red);
            front_color = Colors.Red;
            }
            else
            {
                backColor.Background = new SolidColorBrush(Colors.Red);
                back_color = Colors.Red;
            }
        }

        private void White_Click(object sender, RoutedEventArgs e)
        {
            if (color_toggle)
            {
                frontColor.Background = new SolidColorBrush(Colors.White);
                front_color = Colors.White;
            }
            else
            {
                backColor.Background = new SolidColorBrush(Colors.White);
                back_color = Colors.White;
            }
        }

        private void Blue_Click(object sender, RoutedEventArgs e)
        {
            if (color_toggle)
            {
                frontColor.Background = new SolidColorBrush(Colors.Blue);
            front_color = Colors.Blue;
            }
            else
            {
                backColor.Background = new SolidColorBrush(Colors.Blue);
                back_color = Colors.Blue;
            }
        }

        private void Yellow_Click(object sender, RoutedEventArgs e)
        {
            if (color_toggle)
            {
                frontColor.Background = new SolidColorBrush(Colors.Yellow);
                front_color = Colors.Yellow;
            }
            else
            {
                backColor.Background = new SolidColorBrush(Colors.Yellow);
                back_color = Colors.Yellow;
            }
        }







        private void Pencil_Check(object sender, RoutedEventArgs e)
        {
            painting_mode = Painting_Mode.Pencil;
        }

        private void Line_Check(object sender, RoutedEventArgs e)
        {
            painting_mode = Painting_Mode.Line;
        }

        private void Rectangle_Check(object sender, RoutedEventArgs e)
        {
            painting_mode = Painting_Mode.Rectangle;
        }

        private void Filed_Rectangle_Check(object sender, RoutedEventArgs e)
        {
            painting_mode = Painting_Mode.Filed_Rectangle;
        }

        private void Eraser_Check(object sender, RoutedEventArgs e)
        {
            painting_mode = Painting_Mode.Eraser;
        }

        private void FullFill_Check(object sender, RoutedEventArgs e)
        {
            painting_mode = Painting_Mode.FullFill;
        }



        double thickness;

        private void Slider_Value_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            thickness = e.NewValue;
        }




        private void Refresh_Status(Point position, Color color, int undos)
        {
            point_position.Content = position.ToString();
            point_color.Content = color.R + " " + color.G + " " + color.B;
            undo_times.Content = undos.ToString();
        }










        byte[] image_pixels = null;

        private Color Get_Color(Point point)
        {
            if (image_pixels == null) Capture_Canvas();

            int x = (int)point.X;
            int y = (int)point.Y;
            int width = (int)canvas.ActualWidth;
            Color color = new Color();

            color.B = image_pixels[y * width * 4 + x * 4];
            color.G = image_pixels[y * width * 4 + x * 4 + 1];
            color.R = image_pixels[y * width * 4 + x * 4 + 2];
            color.A = image_pixels[y * width * 4 + x * 4 + 3];

            return color;
        }

        private void Set_Color(Point point, Color color)
        {
            int x = (int)point.X;
            int y = (int)point.Y;
            int width = (int)canvas.ActualWidth;

            image_pixels[y * width * 4 + x * 4] = color.B;
            image_pixels[y * width * 4 + x * 4 + 1] = color.G;
            image_pixels[y * width * 4 + x * 4 + 2] = color.R;
            image_pixels[y * width * 4 + x * 4 + 3] = color.A;
        }

        private void Capture_Canvas()
        {
            var renderTargetBitmap = new RenderTargetBitmap((int)canvas.ActualWidth,
                                                        (int)canvas.ActualHeight,
                                                        96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(canvas);

            int width = (int)canvas.ActualWidth;
            int height = (int)canvas.ActualHeight;

            image_pixels = new byte[width * height * 4];
            int stride = (width * renderTargetBitmap.Format.BitsPerPixel + 7) / 8;

            renderTargetBitmap.CopyPixels(new Int32Rect(0, 0, width, height), image_pixels, stride, 0);
        }
    }
}
