using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppTest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            inkCanvas.AddHandler(InkCanvas.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Mouse_left_down), true);
            inkCanvas.Strokes.StrokesChanged += Strock_changed;
        }

        
        

        string data_name = "ink.data";


        private void Save_button_click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(data_name, XamlWriter.Save(inkCanvas.Strokes));
        }

        private void Load_button_click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Strokes = (StrokeCollection)XamlReader.Load(File.OpenRead(data_name));
        }

        private void Clear_button_click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Strokes.Clear();
        }
    






        enum Paint_Mode {Pencil, Line, Circle};

        Paint_Mode paint_mode=Paint_Mode.Pencil;

        private void Line_button_click(object sender, RoutedEventArgs e)
        {
            paint_mode = Paint_Mode.Line;

        }
        
        private void Mouse_left_down(object sender, MouseButtonEventArgs e)
        {
        }

        private void Mouse_left_up(object sender, MouseButtonEventArgs e)
        {
        }

        





        bool strock_save = true;
        Stack<StrokeCollection> history_added = new Stack<StrokeCollection>();
        Stack<StrokeCollection> history_remove = new Stack<StrokeCollection>();

        private void Strock_changed(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (strock_save)
            {
                history_added.Push(e.Added);
                history_remove.Push(e.Removed);
            }
        }

        private void Undo_button_click(object sender, RoutedEventArgs e)
        {
            if (history_added.Count != 0)
            {
                strock_save = false;
                inkCanvas.Strokes.Remove(history_added.Pop());
                inkCanvas.Strokes.Add(history_remove.Pop());
                strock_save = true;
            }
        }
    }
}
