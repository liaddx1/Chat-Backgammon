using System.Threading.Tasks;
using tWpfMashUp_v0._0._1.MVVM.ViewModels;

namespace tWpfMashUp_v0._0._1.Assets.Components.CustomModal
{
    public static class Modal
    {
        private static string value;

        //caption and title optional
        public static void ShowModal(string caption, string title = " ")
        {
            var view = new ModalView();
            var loc = App.Current.Resources["Locator"] as ViewModelLocator;
            loc.Main.Modal = view;
            view.ModalClosing += (s, e) => loc.Main.Modal = null;
            view.Init(caption, title);
        }

        //caption title and two buttons
        public async static Task<string> ShowModal(string caption, string title, string Button1, string Button2)
        {
            var view = new ModalView();
            var loc = App.Current.Resources["Locator"] as ViewModelLocator;
            loc.Main.Modal = view;
            value = await view.InitWithButtons(caption, title, new string[] { Button1, Button2 });
            loc.Main.Modal = null;
            return value;
        }

    }
}
