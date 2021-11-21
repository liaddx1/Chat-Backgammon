using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace tWpfMashUp_v0._0._1.Extensions
{
    public static class GameUiExtensions
    {
        public static Grid Clear(this Grid grid)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            return grid;
        }

        public static Grid BuildGridDefenitions(this Grid grid, int cols, int rows)
        {
            for (int i = 0; i < cols + 1; i++)
            {
                if (i == cols / 2) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.6, GridUnitType.Star) });
                else grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            //setRows
            for (int i = 0; i < rows + 1; i++)
            {
                if (i == rows / 2)
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                else
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4, GridUnitType.Star) });
            }
            return grid;
        }

        public static void AddToGrid(this Grid grid, FrameworkElement elem, int col = 0, int row = 0)
        {
            Grid.SetRow(elem, row);
            Grid.SetColumn(elem, col);
            grid.Children.Add(elem);
        }

        public static Grid BuildGridPolygons(this Grid grid)
        {
            for (int i = 0; i < grid.ColumnDefinitions.Count + 1; i++) //cols
            {
                for (int j = 0; j < grid.RowDefinitions.Count + 1; j += 2) //rows
                {
                    if (i == grid.ColumnDefinitions.Count / 2) continue;
                    var triangle = new Polygon().BuildPoligon(i, j);
                    var playersStack = new StackPanel().BuildStackPanel(j);
                    grid.AddToGrid(triangle, j, i);
                    grid.AddToGrid(playersStack, j, i);
                    Panel.SetZIndex(triangle, 1);
                    Panel.SetZIndex(playersStack, 2);
                }
            }
            return grid;
        }

        public static Polygon BuildPoligon(this Polygon polygon, int i, int j) => new()
        {
            Stretch = Stretch.Fill,
            Points = j < 1 ? new PointCollection { new Point(0, 0), new Point(30, 100), new Point(60, 0) }
                           : new PointCollection { new Point(0, 100), new Point(30, 0), new Point(60, 100) },
            HorizontalAlignment = HorizontalAlignment.Center,
            Fill = j == 0 && ((i % 2 == 0 && i < 6) || (i % 2 == 1 && i > 6)) || // first row zig zag
                   j == 2 && ((i % 2 == 1 && i < 6) || (i % 2 == 0 && i > 6))    //second row zag zig
                    ? (SolidColorBrush)Application.Current.FindResource("DimBrush")
                    : (SolidColorBrush)Application.Current.FindResource("ComplimentaryBrush"),
            Margin = new Thickness(5, 0, 5, 0),
            IsHitTestVisible = false
        };

        public static StackPanel BuildStackPanel(this StackPanel stack, int j) => new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = j < 1 ? VerticalAlignment.Top : VerticalAlignment.Bottom,
            IsHitTestVisible = false
        };

        public static Border BuildElipses(this Border border, int number)
        {
            var grid = new Grid();
            AddEllipsesToGrid(number, grid);
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;

            Binding binding2 = new Binding("ActualHeight");
            binding2.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1);
            border.SetBinding(Border.MaxWidthProperty, binding2);

            border.Width = Double.NaN;
            Binding binding = new Binding("ActualWidth");
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
            border.SetBinding(Border.HeightProperty, binding);

            border.Background = new SolidColorBrush(Colors.Gray);
            border.Padding = new Thickness(3);
            border.CornerRadius = new CornerRadius(4);
            border.Child = grid;
            return border;
        }

        private static void AddEllipsesToGrid(int number, Grid grid)
        {
            for (int i = 0; i < 3; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            if (number % 2 == 1) grid.AddToGrid(new Ellipse { Fill = new SolidColorBrush(Colors.White) }, 1, 1);
            if (number > 1)
            {
                grid.AddToGrid(new Ellipse { Fill = new SolidColorBrush(Colors.White) }, 0, 0);
                grid.AddToGrid(new Ellipse { Fill = new SolidColorBrush(Colors.White) }, 2, 2);
            }
            if (number > 3)
            {
                grid.AddToGrid(new Ellipse { Fill = new SolidColorBrush(Colors.White) }, 0, 2);
                grid.AddToGrid(new Ellipse { Fill = new SolidColorBrush(Colors.White) }, 2, 0);
            }
            if (number == 6)
            {
                grid.AddToGrid(new Ellipse { Fill = new SolidColorBrush(Colors.White) }, 1, 0);
                grid.AddToGrid(new Ellipse { Fill = new SolidColorBrush(Colors.White) }, 1, 2);
            }
        }
    }
}
