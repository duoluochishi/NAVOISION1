using System.ComponentModel;

namespace NV.CT.Service.UI.Util
{
    public class BindableBase : INotifyPropertyChanged
    {
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)//有改变  
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));//对{propertyName}进行监听  
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
