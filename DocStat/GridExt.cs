using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DocStat
{
    public static class GridExt
    {
        public static void SetData(this Grid grid, List<double> data)
        {
            int width = 40;
            int height = 40;
            int fontSize = 12;
            int horizontal = (int)grid.Width / width;
            int count = data.Count;
            int vertical = (int)Math.Ceiling((double)count / horizontal);

            for (int i = 0; i < horizontal; i++)
            {
                var c = new ColumnDefinition
                {
                    Width = new GridLength(width)
                };
                grid.ColumnDefinitions.Add(c);
            }
            for(int j = 0; j < vertical; j++)
            {
                var r = new RowDefinition
                {
                    Height = new GridLength(height)
                };
                grid.RowDefinitions.Add(r);
            }
            for(int i = 0; i < count; i++)
            {
                int x = i % horizontal, y = i / horizontal;
                var label = new Label() { Content = data[i], Width = width, Height = height, FontSize = fontSize };
                Grid.SetColumn(label, x);
                Grid.SetRow(label, y);
                grid.Children.Add(label);

                var border = new Border() { BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Colors.Black) };
                grid.Children.Add(border);
                Grid.SetColumn(border, x);
                Grid.SetColumnSpan(border, horizontal);
                Grid.SetRow(border, y);
                Grid.SetRowSpan(border, vertical);
            }
            grid.UpdateLayout();
        }
    }
}
