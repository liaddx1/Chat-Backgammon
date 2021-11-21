using System;
using System.Globalization;
using tWpfMashUp_v0._0._1.Sevices;
using tWpfMashUp_v0._0._1.MVVM.Models;
using Microsoft.Extensions.DependencyInjection;

namespace tWpfMashUp_v0._0._1.Assets.Converters
{
    public class SentByMeNameConverter : BaseConverter<SentByMeNameConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var store = App.ServiceProvider.GetRequiredService<StoreService>();
            var user = store.Get(CommonKeys.LoggedUser.ToString()).UserName;

            return (string)value == user ? "Me" : value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
