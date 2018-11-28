using System;
using System.Collections.Generic;
using System.IO;
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
            ListLaplass = new List<double>();
            ListAxiF = new List<double>();
            ListPi = new List<double>();
            LaplassTable = new Dictionary<double, double>();
            N_piList = new List<double>();
            Ni_N_PiList = new List<double>();
            Ni_N_Pi_pow2List = new List<double>();
            Ni_N_Pi_pow2_devide_N_PiList = new List<double>();
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

        public double ExceptedValue { get; set; }
        public double Disperssion { get; set; }
        public double sigma { get; set; }
        public double FrequencyF { get; set; }
        public List<double> ListLaplass { get; set; }
        public List<double> ListAxiF { get; set; }

        public List<double> ListPi { get; set; }
        public Dictionary<double, double> LaplassTable { get; set; }
        public List<double> N_piList { get; set; }
        public List<double> Ni_N_PiList { get; set; }
        public List<double> Ni_N_Pi_pow2List { get; set; }
        public List<double> Ni_N_Pi_pow2_devide_N_PiList { get; set; }
        public double Rozrah { get; set; }

        public void initValues(List<double> values)
        {
            Values = values;
            try
            {
                ExcelFetcher excelFetcher = new ExcelFetcher();
                LaplassTable = excelFetcher.Fetch("C:/Users/ymykhailivskyi/Downloads/DocStat/DocStat/LaplassTable.xlsx");
                minX = Values.Min();
                maxX = Values.Max();
                n = Values.Count;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
        }

        #region First Second Third tables
        public int Calch()
        {
            try
            {
                h = (int)((maxX - minX) / (1 + (3.322 * Math.Log10(n))));
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
            while (tmp < maxX)
            {
                Xi.Add(tmp);
                tmp += h;
            }
            return Xi;
        }

        public List<double> CalcAXi()
        {
            for (int i = 0; i < Xi.Count - 1; i++)
            {
                AXi.Add((Xi[i] + Xi[i + 1]) / 2);
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
                    if (Xi[i] <= v && v <= Xi[i + 1] && v != 0)
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

        public List<double> CalcW_h()
        {
            foreach (var w in W)
            {
                W_h.Add(w / h);
            }
            return W_h;
        }
        #endregion

        #region Forth table
        public double CalcExceptedValue()
        {
            var sumList = AXi.Zip(Ni, (x, y) => x * y).Sum();
            ExceptedValue = sumList / n;

            return ExceptedValue;
        }

        public double CalcDisperssion()
        {
            var sumList = Xi.Sum(x => Math.Pow(x - ExceptedValue, 2));
            Disperssion = sumList / (n - 1);
            return Disperssion;
        }

        public double CalcFrequencyF()
        {
            var a = ExceptedValue;
            sigma = Math.Sqrt(Disperssion);
            //change x !!!!
            var power = Math.Pow(Math.E, (-1 * Math.Pow(X1 - a, 2)) / (2 * Math.Pow(sigma, 2)));
            var firstMultiplier = 1 / (sigma * Math.Sqrt(2 * Math.PI));
            FrequencyF = firstMultiplier * power;
            return FrequencyF;
        }

        public List<double> CalcListLaplass()
        {
            sigma = Math.Sqrt(Disperssion);
            foreach (var item in AXi)
            {
                ListLaplass.Add(CalcLaplass(item));
            }
            return ListLaplass;
        }

        private double CalcLaplass(double aXi)
        {
            var ti = (aXi - ExceptedValue) / sigma;
            var power = Math.Pow(Math.E, Math.Pow(ti, 2) / 2);
            var firstMultiplier = 1 / Math.Sqrt(2 * Math.PI);
            var laplasF = firstMultiplier * power;

            return laplasF;
        }

        public List<double> CalcListAxiF()
        {
            sigma = Math.Sqrt(Disperssion);
            foreach (var item in ListLaplass)
            {
                ListAxiF.Add(item / sigma);
            }
            return ListAxiF;
        }

        #endregion

        #region Fifth table
        public List<double> CalcPiList()
        {
            var TableKeys = LaplassTable.Keys;
            for (int i = 0; i < Xi.Count - 1; i++)
            {
                var firstArg = Math.Round(Math.Abs((Xi[i + 1] - ExceptedValue) / sigma), 2);
                var secondArg = Math.Round(Math.Abs((Xi[i] - ExceptedValue) / sigma), 2);
                if (TableKeys.Contains(firstArg) && TableKeys.Contains(secondArg))
                {
                    var firstValue = LaplassTable[firstArg];
                    var secondValue = LaplassTable[secondArg];
                    ListPi.Add(firstArg - secondArg);
                }   
            }

            return ListPi;
        }

        public List<double> CalcN_PiList()
        {
            foreach (var item in ListPi)
            {
                N_piList.Add(item * n);
            }
            return N_piList;
        }

        public List<double> CalcNi_N_PiList()
        {
            for(int i =0; i<Ni.Count;i++)
            {
                Ni_N_PiList.Add(Ni[i] - N_piList[i]);
            }
            return Ni_N_PiList;
        }

        public List<double> CalcNi_N_Pi_pow2List()
        {
            foreach(var item in Ni_N_PiList)
            {
                Ni_N_Pi_pow2List.Add(Math.Pow(item, 2));
            }
            return Ni_N_Pi_pow2List;
        }


        public List<double> CalcNi_N_Pi_pow2_devide_N_PiList()
        {
            for (int i = 0;i< Ni_N_Pi_pow2List.Count;i++)
            {
                Ni_N_Pi_pow2_devide_N_PiList.Add(Ni_N_Pi_pow2List[i]/ Ni_N_PiList[i]);
            }
            return Ni_N_Pi_pow2_devide_N_PiList;
        }

        public double CalcRozrah()
        {
            Rozrah = Ni_N_Pi_pow2_devide_N_PiList.Sum();
            return Rozrah;
        }
        #endregion
    }
}
