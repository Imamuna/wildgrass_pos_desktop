using System;
using System.Collections.Generic;
using System.Linq;
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
using WildGrass_Desktop_f8.Activities.components.graph;

namespace WildGrass_Desktop_f8.Tests
{
    /// <summary>
    /// Interaction logic for TestingWindow.xaml
    /// </summary>
    public partial class TestingWindow : Window
    {
        public TestingWindow()
        {
            InitializeComponent();

            render();
            this.StateChanged += (sender, e) => render();
            this.SizeChanged += (sender, e) => render();
        }

        private void render()
        {
            double heightUnit = graphMe.ActualHeight /100;
            double widthUnit = graphMe.ActualWidth /100;

            graphMe.Children.Clear();
            Polyline graph = new()
            {
                Stroke = new SolidColorBrush(Color.FromRgb(90, 254, 203)),
                StrokeThickness = 2,
            };
            Polyline branch1 = new()
            {
                Stroke = Brushes.Orange,
                StrokeThickness = 2,
            };
            Polyline branch2 = new()
            {
                Stroke = Brushes.Purple,
                StrokeThickness = 2,
            };
            Polyline branch3 = new()
            {
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 2,
            };

            Line line = new()
            {
                Stroke = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                StrokeThickness = 1,
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = graphMe.ActualHeight,
            };
            Line line1 = new()
            {
                Stroke = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                StrokeThickness = 1,
                X1 = 0,
                Y1 = graphMe.ActualHeight,
                X2 = graphMe.ActualWidth,
                Y2 = graphMe.ActualHeight,
            };

            graphMe.Children.Add(line);
            graphMe.Children.Add(line1);

            //line properties:: x, y, height, tooltip values
            double widthInt = graphMe.ActualWidth/50;
            double count = Math.Round(widthInt);
            double y0 = 100;
            double y1 = 60;
            double y2 = 30;
            double y3 = 10;
            for (int i = 1; i < count; i++)
            {
                double value = 50*Convert.ToDouble(i);
                y0 +=20;
                y1 +=20;
                y2 +=20;
                y3 +=20;


                Point point = new()
                {
                    X = value,
                    Y = y0,
                };
                graph.Points.Add(point);

                Point point1 = new()
                {
                    X = value,
                    Y = y1,
                };
                branch1.Points.Add(point1);
                Point point2 = new()
                {
                    X = value,
                    Y = y2,
                };
                branch2.Points.Add(point2);
                Point point3 = new()
                {
                    X = value,
                    Y = y3,
                };
                branch3.Points.Add(point3);


                IntervalLine intervalLine = new();
                intervalLine.Height = graphMe.ActualHeight;

                //for each of the branches or lines
                ToolTipCard toolTipCard = new()
                {
                    Value = y0.ToString("N2"),
                    Change = "+20%",
                    cardColor = new SolidColorBrush(Color.FromRgb(90, 254, 203)),
                    Title = "Total",
                };
                ToolTipCard toolTipCard1 = new()
                {
                    Value = y1.ToString("N2"),
                    Change = "+20%",
                    cardColor = Brushes.Orange,
                    Title = "Branch 1",
                };
                ToolTipCard toolTipCard2 = new()
                {
                    Value = y2.ToString("N2"),
                    Change = "+0%",
                    cardColor = Brushes.Purple,
                    Title = "Branch 2",
                };
                ToolTipCard toolTipCard3 = new()
                {
                    Value = y3.ToString("N2"),
                    Change = "+120%",
                    cardColor = Brushes.DodgerBlue,
                    Title = "Branch 3",
                };

                Border circlePoint = new()
                {
                    Width = 7,
                    Height = 7,
                    CornerRadius = new CornerRadius(15.0),
                    Background = new SolidColorBrush(Color.FromRgb(90, 254, 203)),
                };
                Border circlePoint1 = new()
                {
                    Width = 7,
                    Height = 7,
                    CornerRadius = new CornerRadius(15.0),
                    Background = Brushes.Orange,
                };
                Border circlePoint2 = new()
                {
                    Width = 7,
                    Height = 7,
                    CornerRadius = new CornerRadius(15.0),
                    Background = Brushes.Purple,
                };
                Border circlePoint3 = new()
                {
                    Width = 7,
                    Height = 7,
                    CornerRadius = new CornerRadius(15.0),
                    Background = Brushes.DodgerBlue,
                };

                intervalLine.ClearCanvas();
                intervalLine.AddPoint(circlePoint, y0);
                intervalLine.AddPoint(circlePoint1, y1);
                intervalLine.AddPoint(circlePoint2, y2);
                intervalLine.AddPoint(circlePoint3, y3);

                intervalLine.ClearToolTip();
                intervalLine.AddToolTip(toolTipCard);
                intervalLine.AddToolTip(toolTipCard1);
                intervalLine.AddToolTip(toolTipCard2);
                intervalLine.AddToolTip(toolTipCard3);
                Canvas.SetTop(intervalLine, 0);
                Canvas.SetLeft(intervalLine, value);
                graphMe.Children.Add(intervalLine);
            }

            //Point point2 = new()
            //{
            //    X = 40,
            //    Y = 100,
            //};
            //Point point3 = new()
            //{
            //    X = 200,
            //    Y = 40,
            //};
            //Point point4 = new()
            //{
            //    X = 400,
            //    Y = 40,
            //};
            //Point point5 = new()
            //{
            //    X = 500,
            //    Y = 200,
            //};
            //graph.Points.Add(point2);
            //graph.Points.Add(point3);
            //graph.Points.Add(point4);
            //graph.Points.Add(point5);

            graphMe.Children.Add(graph);
            graphMe.Children.Add(branch1);
            graphMe.Children.Add(branch2);
            graphMe.Children.Add(branch3);
        }
    }
}
