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
        private Application application;
        private Document document;
        private Pages pages;

        private object what = WdGoToItem.wdGoToPage;
        private object which = WdGoToDirection.wdGoToFirst;
        private object missing = System.Reflection.Missing.Value;

        private Dictionary<char, double> dict;
        private static readonly List<char> letters = "abcdefghijklmnopqrstuvwxyz".ToList();
        private double targetCharCount = letters.Count;
        private const string defPath = @"C:\Users\myhaj\source\repos\DocStat\DocStat\example.docx";
        private string Path { get; set; }

        private OpenFileDialog OpenFileDialog;
        private FormulsRepository Formuls;

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
            document = application.Documents.Open(path, ReadOnly: false);
            pages = document.ActiveWindow.ActivePane.Pages;

            if (num > pages.Count || num <= 0)
            {
                Dispatcher.BeginInvoke(new Action(() => txtBlockError.Text = "Wrong number of page!"));
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
            Dispatcher.BeginInvoke(new Action(() => txtBlockError.Text = ""));

            //count letters entry
            int i;
            double tmp;
            dict = new Dictionary<char, double>();

            for (i = 0; i < targetCharCount; i++)
            {
                tmp = text.Count(chr => chr == letters[i]);
                dict.Add(letters[i], (tmp > 0) ? Math.Round(100.0 / (textLetterCount / tmp), 3) : 0);
            }
            document.Close();

            application.Quit();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtBoxNumber.Text, out int num))
            {
                txtBlockError.Text = "Wrong number of page!";
                return;
            }

            await Task.Run(() => OpenDoc(Path ?? defPath, num));

            var val = dict.Values.ToList();
            grid.SetData(val);

            List<double> x = new List<double>(), y = new List<double>();
            for (int i = 0; i < val.Count; i++)
            {
                x.Add(i);
                y.Add(val[i]);
            }
            image.DrawGraph(x, y);

            val.Sort();
            grid2.SetData(val);

            Formuls.initValues(val);
            var h = Formuls.Sturges();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }

}