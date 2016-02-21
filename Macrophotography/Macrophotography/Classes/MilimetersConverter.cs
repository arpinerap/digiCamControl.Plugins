using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace Macrophotography
{
    public class MilimetersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) * StepperManager.Instance.RailAccuracy;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) / StepperManager.Instance.RailAccuracy;
        }
    }
}
