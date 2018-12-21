using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Application = Microsoft.Office.Interop.Word.Application;
using Task = System.Threading.Tasks.Task;

namespace DocStat
{
    public partial class MainWindow : System.Windows.Window
    {
        #region variables
        private Application application;
        private Document document;
        private Pages pages;

        private object what = WdGoToItem.wdGoToPage;
        private object which = WdGoToDirection.wdGoToFirst;
        private object missing = System.Reflection.Missing.Value;

        private Dictionary<char, double> dict;
        private static readonly List<char> letters = "abcdefghijklmnopqrstuvwxyz0123456789бвгґдєжзиїйклмнптфхцчшщьюя".ToList();
        private double targetCharCount = letters.Count;
        private string Path { get; set; }

        private OpenFileDialog OpenFileDialog;
        private FormulsRepository Formuls;

        private const string BaseText = "The hypothesis about the normal distribution of the general population, from which the formed sample";
        private const string SuccessText = BaseText + " does not contradict the experimental data";
        private const string FailText = BaseText + " contradict the experimental data";

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Formuls = new FormulsRepository();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog = new OpenFileDialog { Filter = "Word (*.docx)|*.docx" };

            if (OpenFileDialog.ShowDialog() == true)
            {
                Path = OpenFileDialog.FileName;
            }
        }

        private void OpenDoc(string path, int num)
        {
            application = new Application();
            try
            {
                document = application.Documents.Open(path, ReadOnly: false);
            }
            catch (Exception)
            {
                throw;
            }
            pages = document.ActiveWindow.ActivePane.Pages;

            if (num > pages.Count || num <= 0)
            {
                ShowMessage("Wrong number of page!");
                return;
            }

            //make range to analyze
            object countx = num;
            var x = application.Selection.GoTo(ref what, ref which, ref countx, ref missing);
            object county = num + 1;
            var y = application.Selection.GoTo(ref what, ref which, ref county, ref missing);

            // if last page make specific setRange
            int allCharCount = document.Characters.Count;
            if (num >= pages.Count)
            {
                x.SetRange(x.Start, allCharCount);
            }
            else
            {
                x.SetRange(x.Start, y.Start);
            }

            x.Select();

            var text = x.Text.ToLower();

            //calculate only letters in file
            //double allLettersCount = 0.0;
            //foreach (char item in document.Characters)
            //    if (char.IsLetter(item)) allLettersCount++;

            // calculate only letters in text
            double textLetterCount = 0.0;
            foreach (char item in text)
            {
                if (char.IsLetter(item))
                {
                    textLetterCount++;
                }
            }

            //clean error and result boxes
            ShowMessage("");

            //count letters entry
            int i;
            double tmp;
            dict = new Dictionary<char, double>();

            for (i = 0; i < targetCharCount; i++)
            {
                tmp = text.Count(chr => chr == letters[i]);
                try
                {
                    dict.Add(letters[i], (tmp > 0) ? Math.Round(100.0 / (textLetterCount / tmp), 3) : 0);
                }
                catch (ArgumentException)
                {
                    throw;
                }
            }
            document.Close();

            application.Quit();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ShowMessage(""); //clean screen

            if (!int.TryParse(txtBoxNumber.Text, out int num))
            {
                ShowMessage("Wrong number of page!");
                return;
            }
            List<double> val = new List<double>();
           
            try
            {
                await Task.Run(() => OpenDoc(Path, num));

                if (dict != null)
                {
                    val = dict.Values.ToList();
                    val = val.FindAll(item => item > 0);
                    Formuls.initValues(val);
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                ShowMessage("Internal error. Bad input data");
                ShowMessage(ex.Message);
                return;
            }

            FirstTableGrid.SetData(val);

            var x = new List<double>();
            var y = new List<double>();
            for (int i = 0; i < val.Count; i++)
            {
                x.Add(i);
                y.Add(val[i]);
            }

            image.DrawGistogram(y);

            val.Sort();
            SecondTableGrid.SetData(val);

            FillThirdTable();

            FillForthTable();

            FillFifthTable();

            FinalCalculations();

        }

        public void FillThirdTable()
        {
            var h = Formuls.Calch();
            var x1 = Formuls.CalcX();
            var Xi = Formuls.CalcXi(x1);
            List<double> Axi = Formuls.CalcAXi();
            List<int> Ni = Formuls.CalcNi();
            var sumNi = Ni.Sum();
            List<double> W = Formuls.CalcW();
            List<double> W_h = Formuls.CalcW_h();

            #region Grid

            #region Formulas

            var first = @"[X_{i};X_{i+1})";
            var second = @"X_{i}=\frac{X_{i}+X_{i+1}}{2}";
            var third = @"n_{i}";
            var fourth = @"\omega_{i}=\frac{n_{i}}{n}";
            var fifth = @"\frac{\omega_{i}}{h}";

            #endregion

            var thirdTableData = new List<(string, List<string>)>();

            #region Convert Xi to formatted

            var firstList = new List<string>();
            for (int i = 0; i < Xi.Count - 1; i++)
            {
                firstList.Add("[" + Xi[i] + "; " + Xi[i + 1] + ")");
            }

            #endregion

            thirdTableData.Add((first, firstList));
            thirdTableData.Add((second, Axi.ToStringList()));
            thirdTableData.Add((third, Ni.ToStringList()));
            thirdTableData.Add((fourth, W.ToStringList()));
            thirdTableData.Add((fifth, W_h.ToStringList()));

            ThirdTableGrid.SetData(thirdTableData);

            ThirdTableGrid.InsertSumRow(Axi, Ni.ToDoubleList(), W, W_h);

            #endregion
        }

        public void FillForthTable()
        {
            var expValue = Formuls.CalcExceptedValue();
            var disperssion = Formuls.CalcDisperssion();
            var frequencyF = Formuls.CalcFrequencyF();
            List<double> Axi = Formuls.CalcAXi();
            List<double> laplassList = Formuls.CalcListLaplass();
            List<double> FlaplassList = Formuls.CalcListAxiF();

            #region Grid

            #region Formulas

            var first = @"X_{i}";
            var second = @"f(x)";

            #endregion

            var fourthTableData = new List<(string, List<string>)>
            {
                (first, Axi.ToStringList()),
                (second, FlaplassList.ToStringList())
            };

            FourthTableGrid.SetData(fourthTableData);

            #endregion
        }

        public void FillFifthTable()
        {
            var PisList = Formuls.CalcPiList();
            var RightRangeNi = Formuls.CalcRightRangeNiList();
            var N_PiList = Formuls.CalcN_PiList();
            var Ni_N_PiList = Formuls.CalcNi_N_PiList();
            var Ni_N_Pi_pow2List = Formuls.CalcNi_N_Pi_pow2List();
            var Ni_N_Pi_pow2_devide_N_PiList = Formuls.CalcNi_N_Pi_pow2_devide_N_PiList();

            #region Grid

            #region Formulas

            var third = @"p_{i}";
            var fourth = @"n*p_{i}";
            var fifth = @"(n_{i}-n*p_{i})";
            var sixth = @"(n_{i}-n*p_{i})^2";
            var seventh = @"\frac{(n_{i}-n*p_{i})^2}{n*p_{i}}";

            #endregion

            var fifthTableData = new List<(string, List<string>)>
            {
                (third, PisList.ToStringList()),
                (fourth, N_PiList.ToStringList()),
                (fifth, Ni_N_PiList.ToStringList()),
                (sixth, Ni_N_Pi_pow2List.ToStringList()),
                (seventh, Ni_N_Pi_pow2_devide_N_PiList.ToStringList())
            };

            FifthTableGrid.SetData(fifthTableData, true);

            #endregion
        }

        public void FinalCalculations()
        {
            var Rozrah = Formuls.CalcRozrah();
            var R = Formuls.CalcR();
            var Xit = Formuls.CalcXit();
            var rezult = Formuls.VerifyDistribution();

            ResultGrid.MakeResult(Xit, Rozrah, rezult, R, SuccessText, FailText);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ShowMessage(string message = "", bool error = true)
        {
            if (message != null)
            {
                if (error)
                {
                    Dispatcher.BeginInvoke(new Action(() => txtBlockError.Text = message));
                }

                // here we clean all previous results
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    FirstTableGrid.Children.Clear();
                    FirstTableGrid.RowDefinitions.Clear();
                    FirstTableGrid.ColumnDefinitions.Clear();

                    SecondTableGrid.Children.Clear();
                    SecondTableGrid.RowDefinitions.Clear();
                    SecondTableGrid.ColumnDefinitions.Clear();

                    ThirdTableGrid.Children.Clear();
                    ThirdTableGrid.RowDefinitions.Clear();
                    ThirdTableGrid.ColumnDefinitions.Clear();

                    FourthTableGrid.Children.Clear();
                    FourthTableGrid.RowDefinitions.Clear();
                    FourthTableGrid.ColumnDefinitions.Clear();

                    FifthTableGrid.Children.Clear();
                    FifthTableGrid.RowDefinitions.Clear();
                    FifthTableGrid.ColumnDefinitions.Clear();

                    ResultGrid.Children.Clear();
                    ResultGrid.RowDefinitions.Clear();
                    ResultGrid.ColumnDefinitions.Clear();
                }));
            }
        }

    }
}