using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NV.CT.Service.AutoCali.Logic
{
    public class ObservableCollectionWrapper<T> : ObservableCollection<T>, INotifyPropertyChanged
    {
        public int SelectedIndex { get; private set; } = -1;

        public T SelectedItem
        {
            get => _SelectedItem;
            set
            {
                if (object.ReferenceEquals(value, _SelectedItem))
                {
                    return;
                }

                _SelectedItem = value;
                OnPropertyChanged(Name_SelectedItem);

                //同步更新选中的位置
                int index = this.IndexOf(value);
                SelectedIndex = index;
            }
        }
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)//有改变
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));//对{propertyName}进行监听
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private T _SelectedItem;
        private static string Name_SelectedItem = nameof(SelectedItem);
    }
}
