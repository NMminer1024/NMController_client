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
    internal class TempColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double temperature)
            {
                if(temperature == 0)
                {
                    return Brushes.Transparent;
                }
                if (temperature < 50)
                    return Brushes.Green;
                else if (temperature >= 50 && temperature <= 65)
                    return Brushes.Gold;
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
