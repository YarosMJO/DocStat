﻿using System;
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
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

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

        public static void SetData(this Grid grid, List<(string formula, List<string> values)> data, bool turn = false)
        {
            if (data == null || data.Count == 0)
                throw new Exception("data can't be null");

            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            int horizontal, vertical;

            if (turn)
            {
                horizontal = data[0].values.Count;
                foreach (var l in data)
                {
                    if (l.values.Count < horizontal)
                        horizontal = l.values.Count;
                }
                ++horizontal;
                vertical = data.Count;
            }
            else
            {
                horizontal = data.Count;
                vertical = data[0].values.Count;
                foreach (var l in data)
                {
                    if (l.values.Count < vertical)
                        vertical = l.values.Count;
                }
                ++vertical;
            }

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
            
            if (turn)
            {
                for(int j = 0; j < vertical; j++)
                {
                    var (formula, values) = data[j];
                    //formula
                    var parser = new TexFormulaParser();
                    var f = parser.Parse(formula);
                    var renderer = f.GetRenderer(TexStyle.Display, 12.0, "Arial");
                    var bitmapSource = renderer.RenderToBitmap(0.0, 0.0);

                    var image = new Image();
                    image.Source = bitmapSource;

                    Grid.SetColumn(image, 0);
                    Grid.SetRow(image, j);
                    grid.Children.Add(image);

                    if (bitmapSource.Width < width && bitmapSource.Height < height)
                        image.Stretch = Stretch.None;
                    else
                        image.Stretch = Stretch.Uniform;
                    grid.InsertBorder(0, j);
                    grid.InsertBorder(0, j);
                    for (int i = 1; i < horizontal; i++)
                    {
                        var label = new Label() { Content = data[j].values[i - 1], Width = width, Height = height, FontSize = fontSize };
                        label.HorizontalContentAlignment = HorizontalAlignment.Center;
                        Grid.SetColumn(label, i);
                        Grid.SetRow(label, j);
                        grid.Children.Add(label);
                        grid.InsertBorder(i, j);
                    }
                }
            }
            else
            {
                for (int i = 0; i < horizontal; i++)
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
            }

            grid.UpdateLayout();
        }

        public static void InsertSumRow(this Grid grid, params List<double>[] data)
        {
            int j = grid.RowDefinitions.Count;

            int horizontal = data.Length + 1;
            int vertical = j + 1;
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
            foreach(var rd in grid.RowDefinitions)
            {
                rd.Height = new GridLength(height);
            }
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height) });

            #endregion

            #region Formula

            //formula
            var parser = new TexFormulaParser();
            var f = parser.Parse(@"\sum");
            var renderer = f.GetRenderer(TexStyle.Display, 12.0, "Arial");
            var bitmapSource = renderer.RenderToBitmap(0.0, 0.0);

            var image = new Image();
            image.Source = bitmapSource;

            Grid.SetColumn(image, 0);
            Grid.SetRow(image, j);
            grid.Children.Add(image);

            if (bitmapSource.Width < width && bitmapSource.Height < height)
                image.Stretch = Stretch.None;
            else
                image.Stretch = Stretch.Uniform;

            grid.InsertBorder(0, j);

            #endregion

            for(int i = 0; i < horizontal - 1; i++)
            {
                var l = data[i];
                var sum = 0.0;
                foreach (var d in l)
                    sum += d;

                var label = new Label() { Content = sum, Width = width, Height = height, FontSize = fontSize };
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetColumn(label, i + 1);
                Grid.SetRow(label, j);
                grid.Children.Add(label);
                grid.InsertBorder(i + 1, j);
            }
        }

        public static void MakeResult(this Grid grid, double xit, double xrozrah, bool result, int r, string sucT, string faiT)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            for (int i = 1; i < 5; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var first = FormulaToImage(@"\chi^2=\sum_{i=1}^k\frac{(n_i-n*p_i)^2}{n*p_i}");
            first.Stretch = Stretch.None;
            Grid.SetColumn(first, 0);
            Grid.SetRow(first, 0);
            grid.Children.Add(first);

            var second = FormulaToImage(@"\chi^2 = " + xrozrah);
            second.Stretch = Stretch.None;
            Grid.SetColumn(second, 0);
            Grid.SetRow(second, 1);
            grid.Children.Add(second);

            var third = FormulaToImage(@"r = " + r);
            third.Stretch = Stretch.None;
            Grid.SetColumn(third, 0);
            Grid.SetRow(third, 2);
            grid.Children.Add(third);

            var fourth = FormulaToImage(@"\chi_{t}^2 = " + xit + @" => \chi ^ 2 " + (result ? "<" : ">") + @" \chi_{t}^2");
            fourth.Stretch = Stretch.None;
            Grid.SetColumn(fourth, 0);
            Grid.SetRow(fourth, 3);
            grid.Children.Add(fourth);

            var label = new Label
            {
                Content = result ? sucT : faiT
            };
            Grid.SetColumn(label, 0);
            Grid.SetRow(label, 4);
            grid.Children.Add(label);

            grid.UpdateLayout();
        }

        private static Image FormulaToImage(string formula)
        {
            var parser = new TexFormulaParser();
            var f = parser.Parse(formula);
            var renderer = f.GetRenderer(TexStyle.Display, 14.0, "Arial");
            var bitmapSource = renderer.RenderToBitmap(0.0, 0.0);

            var image = new Image
            {
                Source = bitmapSource
            };

            return image;
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
