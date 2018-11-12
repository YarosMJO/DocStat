using System;
using System.Collections.Generic;
using System.Linq;

namespace DocStat
{
    public class FormulsRepository
    {
        private List<double> Values;
        public FormulsRepository()
        {
            Values = new List<double>();
        }
        public double minX { get; set; }
        public double maxX { get; set; }
        public double h { get; set; }
        public double n { get; set; }

        public void initValues(List<double> values)
        {
            Values = values;
            minX = Values.Min();
            maxX = Values.Max();
            n = Values.Count;
        }
        public double Sturges()
        {
            try
            {
               h = (maxX - minX) / (1 + 3.322) * Math.Log10(n);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
            
            return h;
        }
        public double Method2()
        {
            return 0;
        }
        public double Method3()
        {
            return 0;
        }
    }
}
