using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.QualityTest.Enums;

namespace NV.CT.Service.QualityTest.Models
{
    /// <summary>
    /// 质量测试项目Model
    /// </summary>
    public class ItemModel : ViewModelBase
    {
        #region Field

        private string? _name;
        private bool _isAllChecked;
        private ItemEntryModel? _selectedEntry;
        private ObservableCollection<ItemEntryModel>? _entries;

        #endregion

        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value.GetLocalizationStr());
        }

        public QTType QTType { get; set; }
        public string? UCType { get; init; }
        public bool IsPhantom { get; init; }

        public bool IsAllChecked
        {
            get => _isAllChecked;
            set => SetProperty(ref _isAllChecked, value);
        }

        [JsonIgnore]
        public ItemEntryModel? SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty(ref _selectedEntry, value);
        }

        public ObservableCollection<ItemEntryModel>? Entries
        {
            get => _entries;
            set => SetProperty(ref _entries, value);
        }
    }
}