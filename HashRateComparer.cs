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
    public class HashRateComparer : IComparer
    {
        private readonly ListSortDirection direction;

        public HashRateComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            NMDevice devx = x as NMDevice;
            NMDevice devy = y as NMDevice;

            string strX = devx.HashRate;
            string strY = devy.HashRate;

            if (strX == null || strY == null)
            {
                return 0;
            }

            double valueX = ParseValue(strX);
            double valueY = ParseValue(strY);

            int result = valueX.CompareTo(valueY);
            return direction == ListSortDirection.Ascending ? result : -result;
        }

        private double ParseValue(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            char lastChar = str[str.Length - 4];
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
                    return double.Parse(str, CultureInfo.InvariantCulture);
            }

            string numberPart = str.Substring(0, str.Length - 4);
            return double.Parse(numberPart, CultureInfo.InvariantCulture) * multiplier;
        }
    }
}
