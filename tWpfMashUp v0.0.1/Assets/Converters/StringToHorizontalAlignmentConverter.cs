using System;
using System.Windows;
using System.Globalization;
using tWpfMashUp_v0._0._1.Sevices;
using tWpfMashUp_v0._0._1.MVVM.Models;
using Microsoft.Extensions.DependencyInjection;

namespace tWpfMashUp_v0._0._1.Assets.Converters
{
    public class StringToHorizontalAlignmentConverter : BaseConverter<StringToHorizontalAlignmentConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var store = App.ServiceProvider.GetRequiredService<StoreService>();
            var user = (string)store.Get(CommonKeys.LoggedUser.ToString()).UserName;
            return (string)value == user ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
