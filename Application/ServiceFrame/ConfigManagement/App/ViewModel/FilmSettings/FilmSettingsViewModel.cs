//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/07/02 16:35:51    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.View.FilmSettings;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.Language;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace NV.CT.ConfigManagement.ViewModel
{
    public class FilmSettingsViewModel : BaseViewModel
    {
        #region private members
        private readonly ILogger<FilmSettingsViewModel> _logger;
        private readonly IFilmSettingsApplicationService _filmSettingsApplicationService;
        private readonly IDialogService _dialogService;

        private readonly int _workAreaWidth = (int)SystemParameters.WorkArea.Width - 320; //屏幕总宽-左区
        private readonly int _workAreaHeight = (int)SystemParameters.WorkArea.Height - 170; //屏幕总高-Header区

        private const int PAGE_MARGIN = 10; //胶片上下左右侧各有10像素的间距
        private const int HEADER_MIN_HEIGHT = 70;
        private const int HEADER_MAX_HEIGHT = 180;
        private const int HEADER_DEFAULT_HEIGHT = 90;
        private const int FOOTER_MIN_HEIGHT = 50;
        private const int FOOTER_MAX_HEIGHT = 120;
        private const int FOOTER_DEFAULT_HEIGHT = 60;

        private const int LOGO_DEFAULT_WIDTH = 60;
        private const int LOGO_DEFAULT_HEIGHT = 30;

        private const string DIALOG_DEFAULT_PATH = "D:\\";
        private const string DIALOG_FILTER = "Images|*.gif;*.jpg;*.jpeg;*.bmp;*.png";
        private const string SECTION_TYPE_HEADER = "header";
        private const string UNSELECTED = "";
        private const int DEFAULT_FONTSIZE = 8;
        private const int MIN_FONTSZIE = 5;
        private const int MAX_FONTSZIE = 15;
        private const string COMMAND_SAVE = "SaveCommand";
        private const string COMMAND_INCREASE = "IncreaseCommand";
        private const string COMMAND_DECREASE = "DecreaseCommand";
        private const string COMMAND_CLEAR_LOGO = "ClearLogoCommand";
        private const string COMMAND_SET_LOGO = "SetLogoCommand";
        #endregion

        #region public proprities

        public int HeaderMinHeight
        {
            get => HEADER_MIN_HEIGHT;
        }

        public int HeaderMaxHeight
        {
            get => HEADER_MAX_HEIGHT;
        }

        private GridLength _headerHeight = new GridLength(HEADER_DEFAULT_HEIGHT);
        public GridLength HeaderHeight
        {
            get => _headerHeight;
            set => SetProperty(ref _headerHeight, value);
        }

        public int FooterMinHeight
        {
            get => FOOTER_MIN_HEIGHT;
        }

        public int FooterMaxHeight
        {
            get => FOOTER_MAX_HEIGHT;
        }

        private GridLength _footerHeight = new GridLength(FOOTER_DEFAULT_HEIGHT);
        public GridLength FooterHeight
        {
            get => _footerHeight;
            set => SetProperty(ref _footerHeight, value);
        }

        private bool _isPortraitChecked = true;
        public bool IsPortraitChecked
        {
            get => _isPortraitChecked;
            set 
            {

                SetProperty(ref _isPortraitChecked, value);
                this.OrientationChanged();                
            }
        }

        private bool _isLandscapeChecked = false;
        public bool IsLandscapeChecked
        {
            get => _isLandscapeChecked;
            set 
            {
                SetProperty(ref _isLandscapeChecked, value);
            } 
        }

        private bool _isEditingTextChecked = true;
        public bool IsEditingTextChecked
        {
            get => _isEditingTextChecked;
            set
            { 
                SetProperty(ref _isEditingTextChecked, value);
                EditingTypeChanged();
            } 
        }

        private bool _isEditingLogoChecked = false;
        public bool IsEditingLogoChecked
        {
            get => _isEditingLogoChecked;
            set => SetProperty(ref _isEditingLogoChecked, value);
        }

        private int _filmWidth;
        public int FilmWidth
        {
            get => _filmWidth;
            set => SetProperty(ref _filmWidth, value);
        }

        private int _filmHeight;
        public int FilmHeight
        {
            get => _filmHeight;
            set => SetProperty(ref _filmHeight, value);
        }

        private string? _headerLogoPath = null;
        public string? HeaderLogoPath
        {
            get => _headerLogoPath;
            set
            {
                SetProperty(ref _headerLogoPath, value);
            }
        }

        private float _headerLogoWidth = LOGO_DEFAULT_WIDTH;
        public float HeaderLogoWidth
        {
            get => _headerLogoWidth;
            set
            {
                SetProperty(ref _headerLogoWidth, value);
            }
        }

        private float _headerLogoHeight = LOGO_DEFAULT_HEIGHT;
        public float HeaderLogoHeight
        {
            get => _headerLogoHeight;
            set
            {
                SetProperty(ref _headerLogoHeight, value);
            }
        }

        private bool _isHeaderLogoVisible = false;
        public bool IsHeaderLogoVisible
        {
            get => _isHeaderLogoVisible;
            set
            {
                SetProperty(ref _isHeaderLogoVisible, value);
            }
        }

        private bool _isFooterLogoVisible = false;
        public bool IsFooterLogoVisible
        {
            get => _isFooterLogoVisible;
            set
            {
                SetProperty(ref _isFooterLogoVisible, value);
            }
        }

        private bool _isSetHeaderButtonVisible = true;
        public bool IsSetHeaderButtonVisible
        {
            get => _isSetHeaderButtonVisible;
            set
            {
                SetProperty(ref _isSetHeaderButtonVisible, value);
            }
        }

        private bool _isSetFooterButtonVisible = true;
        public bool IsSetFooterButtonVisible
        {
            get => _isSetFooterButtonVisible;
            set
            {
                SetProperty(ref _isSetFooterButtonVisible, value);
            }
        }

        private string? _footerLogoPath = null;
        public string? FooterLogoPath
        {
            get => _footerLogoPath;
            set
            {
                SetProperty(ref _footerLogoPath, value);
            }
        }

        private float _footerLogoWidth = LOGO_DEFAULT_WIDTH;
        public float FooterLogoWidth
        {
            get => _footerLogoWidth;
            set
            {
                SetProperty(ref _footerLogoWidth, value);
            }
        }

        private float _footerLogoHeight = LOGO_DEFAULT_HEIGHT;
        public float FooterLogoHeight
        {
            get => _footerLogoHeight;
            set
            {
                SetProperty(ref _footerLogoHeight, value);
            }
        }

        private ObservableCollection<string> _filmHeaderFooterOptionList;
        public ObservableCollection<string> FilmHeaderFooterOptionList
        {
            get => _filmHeaderFooterOptionList;
            set => SetProperty(ref _filmHeaderFooterOptionList, value);
        }

        private List<FilmOptionModel> _selectedHeaderOptionList;
        public List<FilmOptionModel> SelectedHeaderOptionList
        {
            get => _selectedHeaderOptionList;
            set => SetProperty(ref _selectedHeaderOptionList, value);
        }
        
        private List<FilmOptionModel> _selectedFooterOptionList;
        public List<FilmOptionModel> SelectedFooterOptionList
        {
            get => _selectedFooterOptionList;
            set => SetProperty(ref _selectedFooterOptionList, value);
        }

        private Thickness _headerLogoMargin = default;
        public Thickness HeaderLogoMargin
        {
            get => _headerLogoMargin;
            set => SetProperty(ref _headerLogoMargin, value);
        }

        private Thickness _footerLogoMargin = default;
        public Thickness FooterLogoMargin
        {
            get => _footerLogoMargin;
            set => SetProperty(ref _footerLogoMargin, value);
        }

        public float ActualHeaderHeight { get; set; }
        public float ActualFooterHeight { get; set; }

        #endregion

        #region constructor
        public FilmSettingsViewModel(ILogger<FilmSettingsViewModel> logger,
                                     IDialogService dialogService,
                                     IFilmSettingsApplicationService filmSettingsApplicationService)
        {
            this._logger = logger;
            this._filmSettingsApplicationService = filmSettingsApplicationService;
            this._dialogService = dialogService;

            Commands.Add(COMMAND_SAVE, new DelegateCommand(OnSaved));
            Commands.Add(COMMAND_INCREASE, new DelegateCommand<object>(OnIncrease));
            Commands.Add(COMMAND_DECREASE, new DelegateCommand<object>(OnDecrease));
            Commands.Add(COMMAND_CLEAR_LOGO, new DelegateCommand<object>(OnClearLogo));
            Commands.Add(COMMAND_SET_LOGO, new DelegateCommand<object>(OnSetLogo));

            this.Initialize();
        }

        #endregion

        #region private methods

        private void Initialize()
        {
            this.InitializeFilmHeaderFooterOptionsList();
            this.CalculateFilmSize();
            this.RestoreLocations();
        }

        private void InitializeFilmHeaderFooterOptionsList()
        {
            this.FilmHeaderFooterOptionList = Enum.GetNames<FilmHeaderFooterOption>().ToObservableCollection();
            this.FilmHeaderFooterOptionList.Insert(0, UNSELECTED);

            //initialize header binding view models
            this.SelectedHeaderOptionList = new List<FilmOptionModel>();
            for (int row = 0; row <= 4; row++)
            {
                for (int column = 0; column <= 2; column++)
                {
                    this.SelectedHeaderOptionList.Add(new FilmOptionModel() { RowIndex = row, ColumnIndex = column, OptionName = UNSELECTED, FontSize = DEFAULT_FONTSIZE });
                }
            }

            //initialize footer binding view models
            this.SelectedFooterOptionList = new List<FilmOptionModel>();
            for (int row = 0; row <= 2; row++)
            {
                for (int column = 0; column <= 2; column++)
                {
                    this.SelectedFooterOptionList.Add(new FilmOptionModel() { RowIndex = row, ColumnIndex = column, OptionName = UNSELECTED, FontSize = DEFAULT_FONTSIZE });
                }
            }
        }

        private void ResetFilmHeaderFooterOptionsList()
        {
            //reset header binding view models
            for (int row = 0; row <= 4; row++)
            {
                for (int column = 0; column <= 2; column++)
                {
                    var option = this.SelectedHeaderOptionList.Single(o => o.RowIndex == row && o.ColumnIndex == column);
                    option.OptionName = UNSELECTED;
                    option.FontSize = DEFAULT_FONTSIZE;
                }
            }
            //reset footer binding view models
            for (int row = 0; row <= 2; row++)
            {
                for (int column = 0; column <= 2; column++)
                {
                    var option = this.SelectedFooterOptionList.Single(o => o.RowIndex == row && o.ColumnIndex == column);
                    option.OptionName = UNSELECTED;
                    option.FontSize = DEFAULT_FONTSIZE;
                }
            }
        }

        private void CalculateFilmSize()
        {
            if (this.IsPortraitChecked)
            {
                this.FilmHeight = this._workAreaHeight;
                this.FilmWidth = (int)Math.Floor((float)(this.FilmHeight * 14) / 17);
            }
            else
            {
                this.FilmHeight = this._workAreaHeight;
                this.FilmWidth = (int)Math.Ceiling((float)(this.FilmHeight * 17) / 14);
            }        
        }

        private void OnIncrease(object arg)
        {
            FilmOptionModel currentModel = this.ParseCommandArgument(arg);
            if (currentModel.FontSize < MAX_FONTSZIE)
            {
                currentModel.FontSize += 1;
            }

            this.RefreshButtonStatus(currentModel);
        }

        private void OnDecrease(object arg)
        {
            FilmOptionModel currentModel = this.ParseCommandArgument(arg);
            if (currentModel.FontSize > MIN_FONTSZIE)
            {
                currentModel.FontSize -= 1;
            }

            this.RefreshButtonStatus(currentModel);
        }

        private void RefreshButtonStatus(FilmOptionModel currentModel)
        {
            currentModel.IsIncreaseEnabled = currentModel.FontSize >= MAX_FONTSZIE ? false : true;
            currentModel.IsDecreaseEnabled = currentModel.FontSize <= MIN_FONTSZIE ? false : true;
        }

        private void OnSaved()
        {
            this.SaveCurrentOrientation();

            this._dialogService?.ShowDialog(false, MessageLeveles.Info,
                                LanguageResource.Message_Info_Title, LanguageResource.Message_Saved_Successfully,
                                null, ConsoleSystemHelper.WindowHwnd);

        }
        private bool SaveCurrentOrientation() 
        {
            var source = this._filmSettingsApplicationService.Get();
            FilmSettings filmSettings;
            if (this.IsPortraitChecked)
            {
                filmSettings = source.FirstOrDefault(s => s.IsPortrait == true);
            }
            else
            {
                filmSettings = source.FirstOrDefault(s => s.IsPortrait == false);
            }

            if (filmSettings is null) 
            {
                this._logger.LogWarning($"No filmSettings found!");
                return false;
            }

            //保存页眉高度和页脚高度
            filmSettings.NormalizedHeaderHeight = (this.ActualHeaderHeight + PAGE_MARGIN) / (float)this.FilmHeight;
            filmSettings.NormalizedFooterHeight = (this.ActualFooterHeight + PAGE_MARGIN) / (float)this.FilmHeight;

            //保存页眉LOGO
            if (string.IsNullOrWhiteSpace(this.HeaderLogoPath))
            {
                filmSettings.HeaderLogo.IsVisible = false;
                filmSettings.HeaderLogo.PicturePath = string.Empty;
                filmSettings.HeaderLogo.NormalizedWidth = LOGO_DEFAULT_WIDTH / (float)this.FilmWidth;
                filmSettings.HeaderLogo.NormalizedHeight = LOGO_DEFAULT_HEIGHT / (float)this.FilmHeight;

                var locationX = ((float)this.FilmWidth - LOGO_DEFAULT_WIDTH) / 2f;
                var locationY = ((float)this.ActualHeaderHeight - LOGO_DEFAULT_HEIGHT) / 2f;
                filmSettings.HeaderLogo.NormalizedLocationX = locationX / (float)this.FilmWidth;
                filmSettings.HeaderLogo.NormalizedLocationY = locationY / (float)this.FilmHeight;
            }
            else
            {
                filmSettings.HeaderLogo.IsVisible = true;
                filmSettings.HeaderLogo.PicturePath = this.HeaderLogoPath;
                filmSettings.HeaderLogo.NormalizedWidth = this.HeaderLogoWidth / (float)this.FilmWidth;
                filmSettings.HeaderLogo.NormalizedHeight = this.HeaderLogoHeight / (float)this.FilmHeight;

                var locationX = ((float)this.FilmWidth / 2) - (this.HeaderLogoWidth / 2) + (float)this.HeaderLogoMargin.Left;
                var locationY = ((float)this.ActualHeaderHeight / 2) - (this.HeaderLogoHeight / 2) + PAGE_MARGIN + (float)this.HeaderLogoMargin.Top;
                filmSettings.HeaderLogo.NormalizedLocationX = locationX / (float)this.FilmWidth;
                filmSettings.HeaderLogo.NormalizedLocationY = locationY / (float)this.FilmHeight;
            }

            //保存页脚LOGO
            if (string.IsNullOrWhiteSpace(this.FooterLogoPath))
            {   
                filmSettings.FooterLogo.IsVisible = false;
                filmSettings.FooterLogo.PicturePath = string.Empty;
                filmSettings.FooterLogo.NormalizedWidth = LOGO_DEFAULT_WIDTH / (float)this.FilmWidth;
                filmSettings.FooterLogo.NormalizedHeight = LOGO_DEFAULT_HEIGHT / (float)this.FilmHeight; 

                var locationX = ((float)this.FilmWidth - LOGO_DEFAULT_WIDTH) / 2f;
                var locationY = this.FilmHeight - ((float)this.ActualFooterHeight + LOGO_DEFAULT_HEIGHT) / 2f;
                filmSettings.FooterLogo.NormalizedLocationX = locationX / (float)this.FilmWidth;
                filmSettings.FooterLogo.NormalizedLocationY = locationY / (float)this.FilmHeight;
            }
            else
            {
                filmSettings.FooterLogo.IsVisible = true;
                filmSettings.FooterLogo.PicturePath = this.FooterLogoPath;
                filmSettings.FooterLogo.NormalizedWidth = this.FooterLogoWidth / (float)this.FilmWidth;
                filmSettings.FooterLogo.NormalizedHeight = this.FooterLogoHeight / (float)this.FilmHeight;

                var locationX = ((float)this.FilmWidth / 2) - (this.FooterLogoWidth / 2) + (float)this.FooterLogoMargin.Left;
                var locationY = this.FilmHeight - PAGE_MARGIN - ((float)this.ActualFooterHeight + this.FooterLogoHeight)/2f + (float)this.FooterLogoMargin.Top;
                filmSettings.FooterLogo.NormalizedLocationX = locationX / (float)this.FilmWidth;
                filmSettings.FooterLogo.NormalizedLocationY = locationY / (float)this.FilmHeight;
            }

            //保存页眉区设置
            var headerOptions = this.SelectedHeaderOptionList.Where(h => !string.IsNullOrWhiteSpace(h.OptionName));
            float headerCellWidth = (float)(this.FilmWidth - 2 * PAGE_MARGIN) / 3f; //共3列
            float headerCellHeight = this.ActualHeaderHeight / 5f;  //共5行

            float normalizedHeaderCellWidth = headerCellWidth / this.FilmWidth;
            float normalizedHeaderCellHeight = headerCellHeight / this.FilmHeight;

            filmSettings.HeaderCellsList.CellList.Clear();
            foreach(var option in headerOptions)
            {
                float normalizedLocationX = (PAGE_MARGIN + option.ColumnIndex * headerCellWidth) / (float)this.FilmWidth;
                float normalizedLocationY = (PAGE_MARGIN + option.RowIndex * headerCellHeight) / (float)this.FilmHeight;

                filmSettings.HeaderCellsList.CellList.Add(new Cell() {
                                                          RowIndex = option.RowIndex,
                                                          ColumnIndex = option.ColumnIndex,
                                                          Text = option.OptionName,
                                                          NormalizedLocationX = normalizedLocationX,
                                                          NormalizedLocationY = normalizedLocationY,
                                                          NormalizedWidth = normalizedHeaderCellWidth,
                                                          NormalizedHeight = normalizedHeaderCellHeight,
                                                          FontSize = option.FontSize,
                });
            }

            //保存页脚区设置
            var footerOptions = this.SelectedFooterOptionList.Where(h => !string.IsNullOrWhiteSpace(h.OptionName));
            filmSettings.FooterCellsList.CellList.Clear();

            float footerCellWidth = (float)(this.FilmWidth - 2 * PAGE_MARGIN) / 3f;  //共3列
            float footerCellHeight = this.ActualFooterHeight / 3f;  //共3行
            float normalizedFooterCellWidth = footerCellWidth / this.FilmWidth;
            float normalizedFooterCellHeight = footerCellHeight / this.FilmHeight;

            foreach (var option in footerOptions)
            {
                float normalizedLocationX = (PAGE_MARGIN + option.ColumnIndex * footerCellWidth) / (float)this.FilmWidth;
                float normalizedLocationY = (this.FilmHeight - PAGE_MARGIN - (3 - option.RowIndex) * footerCellHeight) / (float)this.FilmHeight;

                filmSettings.FooterCellsList.CellList.Add(new Cell()
                {
                    RowIndex = option.RowIndex,
                    ColumnIndex = option.ColumnIndex,
                    Text = option.OptionName,
                    NormalizedLocationX = normalizedLocationX,
                    NormalizedLocationY = normalizedLocationY,
                    NormalizedWidth = normalizedFooterCellWidth,
                    NormalizedHeight = normalizedFooterCellHeight,
                    FontSize = option.FontSize,
                });
            }

            try
            {
                //持久化到配置文件           
                this._filmSettingsApplicationService.Update(filmSettings);
            }
            catch(Exception ex)
            {
                this._logger.LogError($"Failed to save film settings with exception:{ex.Message}");
                return false;
            }

            return true;
        }

        private void OnSetLogo(object sectionType)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.InitialDirectory = Path.Combine(DIALOG_DEFAULT_PATH);
            openFileDialog.Filter = DIALOG_FILTER;
            string selectedImagePath = string.Empty;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.BindLogo(sectionType.ToString(), openFileDialog.FileName);
            }
        }

        private void BindLogo(string sectionType, string selectedImagePath)
        {
            if (sectionType == SECTION_TYPE_HEADER)
            {
                HeaderLogoPath = selectedImagePath;
                IsSetHeaderButtonVisible = string.IsNullOrWhiteSpace(HeaderLogoPath);
            }
            else
            {
                FooterLogoPath = selectedImagePath;
                IsSetFooterButtonVisible = string.IsNullOrWhiteSpace(FooterLogoPath);
            }
        }

        private void OnClearLogo(object sectionType)
        {
            if (sectionType.ToString() == SECTION_TYPE_HEADER)
            {
                HeaderLogoPath = null;
                IsSetHeaderButtonVisible = true;
            }
            else
            {
                FooterLogoPath = null;
                IsSetFooterButtonVisible = true;
            }
        }

        private void OrientationChanged()
        {
            this.CalculateFilmSize();
            this.RestoreLocations();
        }

        private void EditingTypeChanged()
        {
            IsHeaderLogoVisible = !(IsEditingTextChecked && string.IsNullOrEmpty(this.HeaderLogoPath));
            IsFooterLogoVisible = !(IsEditingTextChecked && string.IsNullOrEmpty(this.FooterLogoPath));
        }

        private void RestoreLocations()
        {     
            var source = this._filmSettingsApplicationService.Get();
            FilmSettings filmSettings;
            if (this.IsPortraitChecked)
            {
                 filmSettings = source.FirstOrDefault(s => s.IsPortrait == true);
            }
            else
            {
                 filmSettings = source.FirstOrDefault(s => s.IsPortrait == false);
            }

            //恢复页眉高度和页脚高度
            this.HeaderHeight = new GridLength(Math.Floor(filmSettings.NormalizedHeaderHeight * this.FilmHeight));
            this.FooterHeight = new GridLength(Math.Floor(filmSettings.NormalizedFooterHeight * this.FilmHeight));

            //恢复页眉Logo
            if (filmSettings.HeaderLogo.IsVisible)
            {
                this.HeaderLogoWidth = filmSettings.HeaderLogo.NormalizedWidth * this.FilmWidth;
                this.HeaderLogoHeight = filmSettings.HeaderLogo.NormalizedHeight * this.FilmHeight;
                if (!string.IsNullOrEmpty(filmSettings.HeaderLogo.PicturePath) && File.Exists(filmSettings.HeaderLogo.PicturePath))
                {
                    this.HeaderLogoPath = filmSettings.HeaderLogo.PicturePath;
                }
            }
            else
            {
                this.HeaderLogoWidth = LOGO_DEFAULT_WIDTH;
                this.HeaderLogoHeight = LOGO_DEFAULT_HEIGHT;
                this.HeaderLogoPath = null;
            }

            IsSetHeaderButtonVisible = string.IsNullOrWhiteSpace(HeaderLogoPath);
            //恢复页眉Logo位置
            if (!string.IsNullOrWhiteSpace(this.HeaderLogoPath))
            {
                float left = (float)this.FilmWidth * filmSettings.HeaderLogo.NormalizedLocationX + (this.HeaderLogoWidth / 2) - ((float)this.FilmWidth / 2);
                float right = -1 * left;
                var actualHeaderHeight = HeaderHeight.Value - PAGE_MARGIN;
                float top = (float)this.FilmHeight * filmSettings.HeaderLogo.NormalizedLocationY + (this.HeaderLogoHeight / 2) - ((float)actualHeaderHeight / 2) - PAGE_MARGIN;
                float bottom = -1 * top;

                this.HeaderLogoMargin = new Thickness(left, top, right, bottom);
            }
            else
            {
                this.HeaderLogoMargin = new Thickness(0, 0, 0, 0);
            }

            //恢复页脚Logo
            if (filmSettings.FooterLogo.IsVisible)
            {
                this.FooterLogoWidth = filmSettings.FooterLogo.NormalizedWidth * this.FilmWidth;
                this.FooterLogoHeight = filmSettings.FooterLogo.NormalizedHeight * this.FilmHeight;

                if (!string.IsNullOrEmpty(filmSettings.FooterLogo.PicturePath) && File.Exists(filmSettings.FooterLogo.PicturePath))
                {
                    this.FooterLogoPath = filmSettings.FooterLogo.PicturePath;
                }
            }
            else
            {
                this.FooterLogoWidth = LOGO_DEFAULT_WIDTH;
                this.FooterLogoHeight = LOGO_DEFAULT_HEIGHT;
                this.FooterLogoPath = null;
            }
            IsSetFooterButtonVisible = string.IsNullOrWhiteSpace(FooterLogoPath);

            //恢复页脚Logo位置
            if (!string.IsNullOrWhiteSpace(this.FooterLogoPath))
            {
                float left = (float)this.FilmWidth * filmSettings.FooterLogo.NormalizedLocationX + (this.FooterLogoWidth / 2) - ((float)this.FilmWidth / 2);
                float right = -1 * left;
                var actualfooterHeight = FooterHeight.Value - PAGE_MARGIN;
                float top = (float)this.FilmHeight * filmSettings.FooterLogo.NormalizedLocationY + PAGE_MARGIN + ((float)actualfooterHeight + this.FooterLogoHeight)/2f - this.FilmHeight;
                float bottom = -1 * top;
                this.FooterLogoMargin = new Thickness(left, top, right, bottom);
            }
            else
            {
                this.FooterLogoMargin = new Thickness(0, 0, 0, 0);
            }

            //恢复Logo可见状态
            this.EditingTypeChanged();

            //重置页眉区和页脚区表格栏
            this.ResetFilmHeaderFooterOptionsList();

            //恢复页眉区设置
            if (filmSettings.HeaderCellsList is not null && filmSettings.HeaderCellsList.CellList is not null)
            {
                foreach (var option in filmSettings.HeaderCellsList.CellList)
                {
                    var selectedOption = this.SelectedHeaderOptionList.First(h => h.RowIndex == option.RowIndex && h.ColumnIndex == option.ColumnIndex);
                    selectedOption.OptionName = option.Text;
                    selectedOption.FontSize = option.FontSize;
                }
            }

            //恢复页脚区设置
            foreach (var option in filmSettings.FooterCellsList.CellList)
            {
                var selectedOption = this.SelectedFooterOptionList.First(h => h.RowIndex == option.RowIndex && h.ColumnIndex == option.ColumnIndex);
                selectedOption.OptionName = option.Text;
                selectedOption.FontSize = option.FontSize;
            }
        }
        private FilmOptionModel ParseCommandArgument(object arg)
        {
            var arguments = arg.ToString().Split("#");
            int index = int.Parse(arguments[1]);
            if (arguments[0] == SECTION_TYPE_HEADER)
            {
                return SelectedHeaderOptionList[index];
            }
            else
            {
                return SelectedFooterOptionList[index];
            }
        }

        #endregion

        #region public methods

        public void ClearLogo(string sectionType)
        {
            this.OnClearLogo(sectionType);
        }

        public void SetLogo(string sectionType)
        {
            this.OnSetLogo(sectionType);
        }
        #endregion
    }
}
