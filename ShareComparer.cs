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
    public class ShareComparer : IComparer
    {
        private readonly ListSortDirection direction;

        public ShareComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            NMDevice devx = x as NMDevice;
            NMDevice devy = y as NMDevice;
            string strX = devx.Share;
            string strY = devy.Share;

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

            string[] parts = str.Split('/');
            if (parts.Length < 2)
            {
                return 0;
            }

            return double.Parse(parts[1], CultureInfo.InvariantCulture);
        }
    }
}
