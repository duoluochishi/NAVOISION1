//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Google.Protobuf.WellKnownTypes;
using NV.CT.Print.Events;
using NV.CT.Print.Models;

namespace NV.CT.Print.ViewModel
{
    public class SelectPagesViewModel : BaseViewModel
    {
        private readonly ILogger<SelectPagesViewModel>? _logger;
        private int _totalNumberOfPrintPages;

        private bool _selectAll;
        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                SetProperty(ref _selectAll, value);
            }
        }

        public bool IsSelectedChangedByAll { get; set; } = false;
        public bool IsSelectedChangedBySingle { get; set; } = false;

        private ObservableCollection<PageWithCheckModel>? _pageWithCheckModelList = new ObservableCollection<PageWithCheckModel>();
        public ObservableCollection<PageWithCheckModel> PageWithCheckModelList
        {
            get => _pageWithCheckModelList ?? new ObservableCollection<PageWithCheckModel>();
            set => SetProperty(ref _pageWithCheckModelList, value);
        }

        public SelectPagesViewModel(ILogger<SelectPagesViewModel>? logger)
        {
            _logger = logger;
            Commands.Add(PrintConstants.COMMAND_SELECT_ALL_CHANGED, new DelegateCommand<string>(OnSelectAllChanged));
            Initialize();
        }

        private void Initialize()
        {
            Global.Instance.ImageViewer.NotifyPrintPageChanged += OnPrintPageChanged;
        }

        public void ClearPageList()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                this.PageWithCheckModelList.Clear();
            });                
        }

        private void OnSelectAllChanged(string checkedState)
        {
            if (IsSelectedChangedBySingle)
                return;

            bool isSelectAllChecked = bool.Parse(checkedState);
            IsSelectedChangedByAll = true;
            PageWithCheckModelList.ForEach(p => p.IsChecked = isSelectAllChecked);
            IsSelectedChangedByAll = false;

            //Notify selected pages has changed
            EventAggregator.Instance.GetEvent<SelectedPagesChangedEvent>().Publish(isSelectAllChecked);
        }

        public void OnSelectPageCheckChanged()
        {
            if (IsSelectedChangedByAll)
                return;

            IsSelectedChangedBySingle = true;
            if (PageWithCheckModelList.Count == 0 || PageWithCheckModelList.Any(p => !p.IsChecked))
            {
                SelectAll = false;
            }
            else
            {
                SelectAll = true;
            }
            IsSelectedChangedBySingle = false;

            //Notify selected pages has changed
            bool hasSelectedPages = PageWithCheckModelList.Any(p => p.IsChecked);
            EventAggregator.Instance.GetEvent<SelectedPagesChangedEvent>().Publish(hasSelectedPages);

        }

        private void OnPrintPageChanged(object? sender, (int totalNumberOfPrintPages, int pageNumber) e)
        {
            if (_totalNumberOfPrintPages == e.totalNumberOfPrintPages) return;

            _totalNumberOfPrintPages = e.totalNumberOfPrintPages;
            this.LoadPageListSource(e.totalNumberOfPrintPages);

            SelectAll = false;
            EventAggregator.Instance.GetEvent<SelectedPagesChangedEvent>().Publish(false);
            EventAggregator.Instance.GetEvent<TotalNumberPageChangedEvent>().Publish(e.totalNumberOfPrintPages);
        }

        private void LoadPageListSource(int totalNumberOfPrintPages)
        {
            if (totalNumberOfPrintPages < 1)
            {
                _logger?.LogDebug($"Receive invalid totalNumberOfPrintPages:{totalNumberOfPrintPages}");
                return;
            }

            Application.Current?.Dispatcher?.Invoke(() =>
            {
                PageWithCheckModelList.Clear();
                for (int i = 1; i <= totalNumberOfPrintPages; i++)
                {
                    PageWithCheckModelList.Add(new PageWithCheckModel() { DisplayPageNumber = $"Page{i}", IsChecked = false, PageNumber = i });
                }
            });
        }

    }
}
