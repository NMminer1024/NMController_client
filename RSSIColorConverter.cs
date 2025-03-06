using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NMController
{
    internal class RSSIColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double rssi)
            {
                if (rssi > -45)
                    return Brushes.Green;
                else if (rssi > -60 && rssi <= -45)
                    return Brushes.DarkOliveGreen;
                else if (rssi >-75 && rssi <=-60)
                    return Brushes.Orange;
                else
                    return Brushes.Red;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
