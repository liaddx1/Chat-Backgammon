using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace tWpfMashUp_v0._0._1.Core
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void onProppertyChange([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
