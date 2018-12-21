using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocStat
{
    public static class ListExt
    {
        public static List<string> ToStringList<T>(this List<T> list)
        {
            var result = new List<string>();
            foreach(var i in list)
            {
                result.Add(i.ToString());
            }
            return result;
        }

        public static List<double> ToDoubleList(this List<int> list)
        {
            var result = new List<double>();
            foreach (var i in list)
            {
                result.Add(i);
            }
            return result;
        }
    }
}
