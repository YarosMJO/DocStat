using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DocStat
{
    public class ExcelFetcher
    {
        private readonly string excelPath = @"\LaplassTable.xlsx";
        public Dictionary<double, double> Fetch()
        {
            var xlsApp = new Application();
            var wb = xlsApp.Workbooks.Open(Directory.GetCurrentDirectory() + excelPath);
            var sheets = wb.Worksheets;
            var ws = (Worksheet)sheets.get_Item(1);

            Range firstCol = ws.UsedRange.Columns[1];
            Array keys = firstCol.Value as Array;

            Range secondCol = ws.UsedRange.Columns[2];
            Array values = secondCol.Value2 as Array;

            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "," };
            var stringKeys = new List<string>();
            foreach (var item in keys)
            {
                stringKeys.Add(item.ToString());
            }
            var doubleKeys = stringKeys.ConvertAll(x => double.Parse(x, formatter));

            var stringValues = new List<string>();
            foreach (var item in values)
            {
                stringValues.Add(item.ToString());
            }
            var doubleValues = stringValues.ConvertAll(x => double.Parse(x, formatter));

            Dictionary<double, double> dic = doubleKeys.Zip(doubleValues, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);

            return dic;
        }
    }
}
