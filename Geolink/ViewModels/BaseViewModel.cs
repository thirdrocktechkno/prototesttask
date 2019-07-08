using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Geolink
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region InotifyPropertychangeImplement
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public BaseViewModel()
        {
        }
    }
}
