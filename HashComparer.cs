using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMController
{
    public class HashComparer : IComparer
    {

        private readonly ListSortDirection direction;
        private readonly string sortBy;

        public HashComparer(ListSortDirection direction, string sortBy)
        {
            this.direction = direction;
            this.sortBy = sortBy;
        }

        public int Compare(object? x, object? y)
        {
            NMDevice devx = x as NMDevice;
            NMDevice devy = y as NMDevice;

            string strX = "";
            string strY = "";

            if(sortBy == "LastDiff")
            {
                strX = devx.LastDiff;
                strY = devy.LastDiff;
            }
            else if(sortBy == "BestDiff")
            {
                strX = ExtractBestDiff(devx.BestDiff);
                strY = ExtractBestDiff(devy.BestDiff);
            }
            else
            {
                strX = devx.NetDiff;
                strY = devy.NetDiff;
            }

            if (strX == null || strY == null)
            {
                return 0;
            }

            double valueX = ParseValue(strX);
            double valueY = ParseValue(strY);

            int result = valueX.CompareTo(valueY);
            return direction == ListSortDirection.Ascending ? result : -result;
        }

        private string ExtractBestDiff(string str)
        {
            var parts = str.Split(new[] { "\r" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                return parts[1].Trim();
            }
            return "0.000";
        }

        private double ParseValue(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            char lastChar = str[str.Length - 1];
            string numberPart = str.Substring(0, str.Length - 1);
            double multiplier = 1;

            switch (lastChar)
            {
                case 'E':
                case 'e':
                    multiplier = 1e18;
                    break;
                case 'P':
                case 'p':
                    multiplier = 1e15;
                    break;
                case 'T':
                case 't':
                    multiplier = 1e12;
                    break;
                case 'G':
                case 'g':
                    multiplier = 1e9;
                    break;
                case 'M':
                case 'm':
                    multiplier = 1e6;
                    break;
                case 'K':
                case 'k':
                    multiplier = 1e3;
                    break;
                default:
                    return double.Parse(numberPart, CultureInfo.InvariantCulture);
            }

            return double.Parse(numberPart, CultureInfo.InvariantCulture) * multiplier;
        }
    }
}
