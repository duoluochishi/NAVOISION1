using System;

namespace NV.CT.ConfigManagement.View.FilmSettings
{
    public class FilmOptionModel : BaseViewModel
    {
        private int _rowIndex;
        public int RowIndex
        {
            get => _rowIndex;
            set => SetProperty(ref _rowIndex, value);
        }

        private int _columnIndex;
        public int ColumnIndex
        {
            get => _columnIndex;
            set => SetProperty(ref _columnIndex, value);
        }

        private string _optionName;
        public string OptionName
        {
            get => _optionName;
            set => SetProperty(ref _optionName, value);
        }

        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        private bool _isDecreaseEnabled = true;
        public bool IsDecreaseEnabled
        {
            get => _isDecreaseEnabled;
            set => SetProperty(ref _isDecreaseEnabled, value);
        }

        private bool _isIncreaseEnabled = true;
        public bool IsIncreaseEnabled
        {
            get => _isIncreaseEnabled;
            set => SetProperty(ref _isIncreaseEnabled, value);
        }

    }
}
