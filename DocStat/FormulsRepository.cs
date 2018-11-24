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
            Xi = new List<double>();
            AXi = new List<double>();
            Ni = new List<int>();
            W = new List<double>();
            W_h = new List<double>();
        }
        public double minX { get; set; } // min of frequencies 
        public double maxX { get; set; } // max of frequencies
        public int h { get; set; } // step
        public double n { get; set; } // count of frequencies
        public double X1 { get; set; } // first X1
        public List<double> Xi { get; set; } // X1,X2,X3.... Xh where h - step
        public List<double> AXi { get; set; } // Average value of (Xi + (Xi+1))/2 for every range
        public List<int> Ni { get; set; }// values count in every range
        public List<double> W { get; set; }
        public List<double> W_h { get; set; }

        public void initValues(List<double> values)
        {
            Values = values;
            try
            {
                minX = Values.Min();
                maxX = Values.Max();
                n = Values.Count;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            
            
        }
        public int Calch()
        {
            try
            {
                h = (int)((maxX - minX) / (1 + (3.322 * Math.Log10(n))));
                //h = 3;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }

            return h;
        }

        public double CalcX()
        {
            X1 = minX - (h / 2);
            return X1;
        }

        public List<double> CalcXi(double Xn)
        {
            var tmp = Xn;
            for (int i = 0; i < h+1; i++)
            {
                Xi.Add(tmp);
                tmp += h;   
            }
            return Xi;
        }

        public List<double> CalcAXi()
        {
            for (int i = 0; i< Xi.Count-1; i++)
            {
                AXi.Add((Xi[i] + Xi[i+1])/2);
            }
            return AXi;
        }
        public List<int> CalcNi()
        {
            Values.Sort();// Here we sort, be careful
            for (int i = 0; i < Xi.Count - 1; i++)
            {
                Ni.Add(0);
                foreach (var v in Values)
                {
                    if (Xi[i] <=v && v <=Xi[i + 1] && v != 0)
                    {
                        Ni[i] += 1;
                    }
                }
            }
            return Ni;
        }
        public List<double> CalcW()
        {
            foreach (var ni in Ni)
            {
                W.Add(ni / n);
            }
            return W;
        }
        public List<double > CalcW_h()
        {
            foreach (var w in W)
            {
                W_h.Add(w / h);
            }
            return W_h;
        }
    }
}
