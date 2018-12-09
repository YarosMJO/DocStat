using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocStat
{
    public class FormulsRepository
    {
        private const int S = 3;
        private readonly double[] PEARSON_CRITERIONS =
        {   3.8, 6.0, 7.8, 9.5, 11.1, 12.6, 14.1, 15.5, 16.9, 18.3, 19.7,
            21.0, 22.4, 23.7, 25.0, 26.3, 27.6, 28.9, 30.1, 31.4, 32.7,
            33.9, 35.2, 36.4, 37.7, 38.9, 40.1, 41.3, 42.6, 43.8
        };

        private List<double> Values;

        public FormulsRepository()
        {
            Values = new List<double>();
            Xi = new List<double>();
            AXi = new List<double>();
            Ni = new List<int>();
            W = new List<double>();
            W_h = new List<double>();
            FrequencyF = new List<double>();
            ListLaplass = new List<double>();
            ListAxiF = new List<double>();
            ListPi = new List<double>();
            LaplassTable = new Dictionary<double, double>();
            N_piList = new List<double>();
            Ni_N_PiList = new List<double>();
            Ni_N_Pi_pow2List = new List<double>();
            Ni_N_Pi_pow2_devide_N_PiList = new List<double>();
            RightRangeNi = new List<double>();
            mList = new List<double>();
        }

        public double minX { get; private set; } // min of frequencies 
        public double maxX { get; private set; } // max of frequencies
        public double h { get; private set; } // step
        public double n { get; private set; } // count of frequencies
        public double X1 { get; private set; } // first X1
        public List<double> Xi { get; private set; } // X1,X2,X3.... Xh where h - step
        public List<double> AXi { get; private set; } // Average value of (Xi + (Xi+1))/2 for every range
        public List<int> Ni { get; private set; }// values count in every range
        public List<double> W { get; private set; }
        public List<double> W_h { get; private set; }

        public double ExceptedValue { get; private set; }
        public double Disperssion { get; private set; }
        public double sigma { get; private set; }
        public List<double> FrequencyF { get; private set; }
        public List<double> ListLaplass { get; private set; }
        public List<double> ListAxiF { get; private set; }

        public List<double> ListPi { get; private set; }
        public Dictionary<double, double> LaplassTable { get; private set; }
        public List<double> N_piList { get; private set; }
        public List<double> Ni_N_PiList { get; private set; }
        public List<double> Ni_N_Pi_pow2List { get; private set; }
        public List<double> Ni_N_Pi_pow2_devide_N_PiList { get; private set; }
        public double Rozrah { get; private set; }
        public List<double> RightRangeNi { get; private set; }
        public List<double> mList { get; private set; }
        public double Xit { get; set; }
        public int R { get; set; }
        public void initValues(List<double> values)
        {
            try
            {
                ExcelFetcher excelFetcher = new ExcelFetcher();
                LaplassTable = excelFetcher.Fetch();
                Values = values;
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
        public double Calch()
        {
            try
            {
                h = (maxX - minX) / (1 + (3.322 * Math.Log10(n)));
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
            Xi.Clear();
            var tmp = Xn;
            while (tmp < maxX + h)
            {
                Xi.Add(tmp);
                tmp += h;
            }
            return Xi;
        }

        public List<double> CalcAXi()
        {
            AXi.Clear();
            for (int i = 0; i < Xi.Count - 1; i++)
            {
                AXi.Add((Xi[i] + Xi[i + 1]) / 2);
            }
            return AXi;
        }

        public List<int> CalcNi()
        {
            Ni.Clear();
            Values.Sort();// Here we sort, be careful
            for (int i = 0; i < Xi.Count - 1; i++)
            {
                Ni.Add(0);
                foreach (var v in Values)
                {
                    if (Xi[i] <= v && v < Xi[i + 1] && v != 0)
                    {
                        Ni[i] += 1;
                    }
                }
            }
            return Ni;
        }

        public List<double> CalcW()
        {
            W.Clear();
            foreach (var ni in Ni)
            {
                W.Add(ni / n);
            }
            return W;
        }

        public List<double> CalcW_h()
        {
            W_h.Clear();
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
            var sumList = AXi.Select((value, index) => Math.Pow(Math.Abs(value - ExceptedValue), 2) * Ni[index]).Sum();
            Disperssion = sumList / (n - 1);
            return Disperssion;
        }

        public List<double> CalcFrequencyF()
        {
            var a = ExceptedValue;
            sigma = Math.Sqrt(Disperssion);
            for (int i = 0; i < AXi.Count(); i++)
            {
                var power = Math.Pow(Math.E, (-1 * Math.Pow(AXi[i] - a, 2)) / (2 * Math.Pow(sigma, 2)));
                var firstMultiplier = 1 / (sigma * Math.Sqrt(2 * Math.PI));
                FrequencyF.Add(firstMultiplier * power);
            }

            return FrequencyF;
        }

        public List<double> CalcListLaplass()
        {
            ListLaplass.Clear();
            sigma = Math.Sqrt(Disperssion);
            foreach (var item in AXi)
            {
                ListLaplass.Add(CalcLaplass(item));
            }
            return ListLaplass;
        }

        private double CalcLaplass(double aXi)
        {
            var ti = Math.Abs((aXi - ExceptedValue) / sigma);
            var power = Math.Pow(Math.E, -1 * Math.Pow(ti, 2) / 2);
            var firstMultiplier = 1 / Math.Sqrt(2 * Math.PI);
            var laplasF = Math.Round(firstMultiplier * power, 4);

            return laplasF;
        }

        public List<double> CalcListAxiF()
        {
            ListAxiF.Clear();
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
            ListPi.Clear();
            var TableKeys = LaplassTable.Keys;

            var mXi = CalcListWithRightRange(Xi);

            for (int i = 0; i < mXi.Count - 1; i++)
            {
                var firstBracket = (mXi[i + 1] - ExceptedValue) / sigma;
                var secondBracket = (mXi[i] - ExceptedValue) / sigma;

                var firstArgSignCoef = firstBracket > 0 ? 1 : -1;
                var secondArgSignCoef = secondBracket > 0 ? 1 : -1;

                var firstArg = Math.Round(Math.Abs(firstBracket), 2);
                var secondArg = Math.Round(Math.Abs(secondBracket), 2);

                if (TableKeys.Contains(firstArg) && TableKeys.Contains(secondArg))
                {
                    var firstValue = firstArgSignCoef * LaplassTable[firstArg];
                    var secondValue = secondArgSignCoef * LaplassTable[secondArg];
                    ListPi.Add(Math.Abs(firstValue - secondValue));
                }
            }
            return ListPi;
        }

        public List<double> CalcListWithRightRange(List<double> list)
        {
            if (list.Count < 4)
            {
                return list;
            }

            mList = new List<double>(list.Capacity);
            for (int j = 0; j < list.Count - 1; j++)
            {
                if (j == 1)
                {
                    continue;
                }
                else if (j == list.Count - 2)
                {
                    mList.Add(list.Last());
                    break;
                }
                mList.Add(list[j]);
            }

            return mList;
        }

        public List<double> CalcRightRangeNiList()
        {
            RightRangeNi.Clear();
            RightRangeNi = Ni.ToDoubleList();
            if (Ni.Count > 4)
            {
                var zeroAndfirstSum = Ni[0] + Ni[1];
                var prelastAndlastSum = Ni[Ni.Count - 2] + Ni.Last();

                //Remove bounds that not include in right range and
                RightRangeNi.RemoveRange(0, 2);
                //Insert missed value from new right range
                RightRangeNi.Insert(0, zeroAndfirstSum);

                RightRangeNi.RemoveRange(Ni.Count - 3, 2);
                RightRangeNi.Add(prelastAndlastSum);
            }
            return RightRangeNi;
        }

        public List<double> CalcN_PiList()
        {
            N_piList.Clear();
            foreach (var item in ListPi)
            {
                N_piList.Add(item * n);
            }
            return N_piList;
        }

        public List<double> CalcNi_N_PiList()
        {
            Ni_N_PiList.Clear();

            var mNi = CalcRightRangeNiList();

            for (int i = 0; i < mNi.Count; i++)
            {
                Ni_N_PiList.Add(mNi[i] - N_piList[i]);
            }
            return Ni_N_PiList;
        }

        public List<double> CalcNi_N_Pi_pow2List()
        {
            Ni_N_Pi_pow2List.Clear();
            foreach (var item in Ni_N_PiList)
            {
                Ni_N_Pi_pow2List.Add(Math.Pow(item, 2));
            }
            return Ni_N_Pi_pow2List;
        }

        public List<double> CalcNi_N_Pi_pow2_devide_N_PiList()
        {
            Ni_N_Pi_pow2_devide_N_PiList.Clear();
            for (int i = 0; i < Ni_N_Pi_pow2List.Count; i++)
            {
                Ni_N_Pi_pow2_devide_N_PiList.Add(Ni_N_Pi_pow2List[i] / N_piList[i]);
            }
            return Ni_N_Pi_pow2_devide_N_PiList;
        }
        #endregion

        #region final calculations
        public double CalcRozrah()
        {
            Rozrah = Ni_N_Pi_pow2_devide_N_PiList.Sum();
            return Rozrah;
        }

        //calculate stages of reedom
        public int CalcR()
        {
            var k = mList.Count - 1;
            R = k - S;
            return R;
        }
        public double CalcXit()
        {
            Xit = (R - 1 >= 0 && R - 1 < PEARSON_CRITERIONS.Length - 1) ? PEARSON_CRITERIONS[R - 1] : 0;
            return Xit;
        }
        public bool VerifyDistribution()
        {
            CalcR();
            CalcXit();
            return Rozrah < Xit;
        }
        #endregion
    }
}
