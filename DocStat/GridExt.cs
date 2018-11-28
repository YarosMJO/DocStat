using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfMath;
using WpfMath.Controls;

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
            for (int j = 0; j < vertical; j++)
            {
                var r = new RowDefinition
                {
                    Height = new GridLength(height)
                };
                grid.RowDefinitions.Add(r);
            }

            var array = Make2DArray(data.ToArray(), vertical, horizontal);

            int n = 0;
            for(int j = 0; j < vertical; j++)
            {
                for(int i = 0; i < horizontal; i++)
                {
                    if (n < count)
                    {
                        var label = new Label() { Content = array[j, i], Width = width, Height = height, FontSize = fontSize };
                        Grid.SetColumn(label, i);
                        Grid.SetRow(label, j);
                        grid.Children.Add(label);
                        ++n;
                    }
                    else
                    {
                        var label = new Label() { Content = "", Width = width, Height = height, FontSize = fontSize };
                        Grid.SetColumn(label, i);
                        Grid.SetRow(label, j);
                        grid.Children.Add(label);
                    }
                    grid.InsertBorder(i, j);
                }
            }
            grid.UpdateLayout();
        }

        public static void SetData(this Grid grid, List<(string formula, List<string> values)> data)
        {
            if (data == null || data.Count == 0)
                throw new Exception("data can't be null");

            int horizontal = data.Count;
            int vertical = data[0].values.Count + 1;
            int width = (int)(grid.Width / horizontal);
            int height = (int)(grid.Height / vertical);
            int fontSize = 12;

            #region Cells

            for (int i = 0; i < horizontal; i++)
            {
                var c = new ColumnDefinition
                {
                    Width = new GridLength(width)
                };
                grid.ColumnDefinitions.Add(c);
            }
            for (int j = 0; j < vertical; j++)
            {
                var r = new RowDefinition
                {
                    Height = new GridLength(height)
                };
                grid.RowDefinitions.Add(r);
            }

            #endregion

            for(int i = 0; i < horizontal; i++)
            {
                var (formula, values) = data[i];
                //formula
                var parser = new TexFormulaParser();
                var f = parser.Parse(formula);
                var renderer = f.GetRenderer(TexStyle.Display, 12.0, "Arial");
                var bitmapSource = renderer.RenderToBitmap(0.0, 0.0);

                var image = new Image();
                image.Source = bitmapSource;

                Grid.SetColumn(image, i);
                Grid.SetRow(image, 0);
                grid.Children.Add(image);
                
                if (bitmapSource.Width < width && bitmapSource.Height < height)
                    image.Stretch = Stretch.None;
                else
                    image.Stretch = Stretch.Uniform;

                grid.InsertBorder(i, 0);

                for (int j = 1; j < vertical; j++)
                {
                    var label = new Label() { Content = data[i].values[j - 1], Width = width, Height = height, FontSize = fontSize };
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Grid.SetColumn(label, i);
                    Grid.SetRow(label, j);
                    grid.Children.Add(label);
                    grid.InsertBorder(i, j);
                }
            }
            grid.UpdateLayout();
        }

        public static void InsertBorder(this Grid grid, int i, int j)
        {
            var border = new Border() { BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Colors.Black) };
            grid.Children.Add(border);
            Grid.SetColumn(border, i);
            Grid.SetRow(border, j);
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private static T[,] Make2DArray<T>(T[] input, int height, int width)
        {
            T[,] output = new T[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (j + i * width >= input.Length)
                        return output;
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }
    }
}
