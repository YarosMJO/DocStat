using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DocStat
{
    public static class ImageExt
    {
        public static void DrawGraph(this Image image, List<double> x, List<double> y)
        {
            if (x.Count != y.Count)
                throw new Exception("Size must be the same");

            int count = x.Count;
            double rangeX = Range(x), rangeY = Range(y);
            double xScale = image.Width / rangeX, yScale = image.Height / rangeY;

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                Point first = new Point(), second = first;
                Pen pen = new Pen(Brushes.Green, 1);
                double vOffset = rangeY / 2;
                double minY = Min(y);
                //dc.DrawLine(pen, new Point(0, vOffset * yScale), new Point(image.Width * xScale, vOffset * yScale));
                for (int i = 0; i < count - 1; i++)
                {
                    first.X = x[i] * xScale;
                    second.X = x[i + 1] * xScale;

                    first.Y = image.Height - (y[i] - minY) * yScale;
                    second.Y = image.Height - (y[i + 1] - minY) * yScale;
                    //first.X = x[i] * xScale;
                    //first.Y = (y[i] / 2 + vOffset) * yScale;
                    //second.X = x[i + 1] * xScale;
                    //second.Y = (y[i + 1] / 2 + vOffset) * yScale;
                    dc.DrawLine(pen, first, second);
                    //dc.DrawRectangle(Brushes.Green, null, new Rect(20, 20, 150, 100));
                }
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)image.Width, (int)image.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            image.Source = rtb;
        }

        private static double Min(List<double> values)
        {
            if (values.Count == 0)
                return 0;
            double min = values[0];
            foreach (var d in values)
            {
                if (d < min)
                    min = d;
            }
            return min;
        }

        private static double Range(List<double> values)
        {
            if (values.Count == 0)
                return 0;
            double max = values[0], min = max;
            foreach(var d in values)
            {
                if (d < min)
                    min = d;
                if (d > max)
                    max = d;
            }
            return max - min;
        }
    }
}
