using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Markup;

namespace tWpfMashUp_v0._0._1.Assets.Converters
{
    /// <summary>
    /// A base value converter that allows direct XAML usage
    /// </summary>
    /// <typeparam name="T">The type of this value converter</typeparam>
    public abstract class BaseConverter<T> : MarkupExtension, IValueConverter
        where T : class, new()
    {
        private static T Converter;

        public override object ProvideValue(IServiceProvider serviceProvider) => Converter ?? (Converter = new T());

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}
