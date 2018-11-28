using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Office.Interop.Excel;
namespace DocStat
{
    public class ExcelFetcher
    {
        public Dictionary<double, double> Fetch(string filename)
        {
            //C: \Users\ymykhailivskyi\Downloads\DocStat\DocStat\IntegratedLaplass.xlsx

            Application xlsApp = new Application();
            Workbook wb = xlsApp.Workbooks.Open(filename,
             0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true);
            Sheets sheets = wb.Worksheets;
            Worksheet ws = (Worksheet)sheets.get_Item(1);

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
