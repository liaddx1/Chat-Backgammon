using System;
using System.Globalization;

namespace tWpfMashUp_v0._0._1.Assets.Converters
{
    public class TimeToDisplayConverter : BaseConverter<TimeToDisplayConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the time passed in
            var time = (DateTime)value;

            // If it is today
            if (time.Date == DateTime.UtcNow.Date)
                // Return just time
                return time.ToLocalTime().ToString("hh:mm tt");

            // Otherwise, return a full date
            return time.ToLocalTime().ToString("HH:mm, dd MMM yyyy");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
