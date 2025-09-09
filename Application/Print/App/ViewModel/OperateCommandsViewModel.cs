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

using NV.CT.DicomUtility.DicomImage;
using NV.CT.Language;
using NV.CT.Print.Events;
using NV.CT.Print.Extensions;
using NV.CT.Print.View;
using NV.MPS.Configuration;
using NV.MPS.UI.Dialog.Service;
using System.Drawing;
using System.Windows.Media;
using Config = NV.MPS.Configuration;

namespace NV.CT.Print.ViewModel
{
    public class OperateCommandsViewModel : BaseViewModel
    {
        private const string OPERATION_TYPE_ZOOM = "zoom";
        private const string OPERATION_TYPE_MOVE = "move";
        private const string OPERATION_TYPE_ROTATE = "rotate";
        private const string OPERATION_TYPE_INVERT = "invert";
        private const string OPERATION_TYPE_WWWL = "wwwl";
        private const string OPERATION_TYPE_FLIP_HORIZONTAL = "flip_horizontal";
        private const string OPERATION_TYPE_FLIP_VERTICAL = "flip_vertical";
        private const string OPERATION_TYPE_FLIP_HIDE_TEXTS = "hide_texts";
        private const string OPERATION_TYPE_RESET_TO_INIT = "reset2init";
        private const string OPERATION_TYPE_REFERENCE_LINE = "reference_line";
        private const string OPERATION_TYPE_COPY = "copy";
        private const string OPERATION_TYPE_PASTE = "paste";
        private const string OPERATION_TYPE_CUT = "cut";
        private const string OPERATION_TYPE_DELETE = "delete";
        private const string OPERATION_TYPE_SELECT_ALL = "selectall";
        private const string OPERATION_TYPE_UNSELECT_ALL = "unselectall";
        private const string OPERATION_TYPE_FILTER = "filter";
        private const string OPERATION_TYPE_SPLIT = "split";
        private const string OPERATION_TYPE_MERGE = "merge";
        private const string OPERATION_TYPE_ROI_RECT = "roi_rect";
        private const string OPERATION_TYPE_ROI_CIRCLE = "roi_circle";
        private const string OPERATION_TYPE_ROI_FREEHAND = "roi_freehand";
        private const string OPERATION_TYPE_ROI_REMOVEALL = "roi_removeall";
        private const string OPERATION_CUSTOM = "Custom";

        private readonly ILogger<OperateCommandsViewModel>? _logger;
        private readonly IDialogService _dialogService;
        private CustomWWWLWindow? _customWwwlWindow;

        private ObservableCollection<WindowingInfo>? _wwwlItems;
        public ObservableCollection<WindowingInfo>? WWWLItems
        {
            get => _wwwlItems;
            set => SetProperty(ref _wwwlItems, value);
        }
        private ObservableCollection<Config.KernelType>? _FilterItems;
        public ObservableCollection<Config.KernelType>? FilterItems
        {
            get => _FilterItems;
            set => SetProperty(ref _FilterItems, value);
        }

        private bool _isEditable = true;
        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                this.SetProperty(ref _isEditable, value);
            }
        }

        private System.Windows.Media.Brush _wwwlFillColor = new SolidColorBrush(Colors.Gray);
        public System.Windows.Media.Brush WWWLFillColor
        {
            get
            {
                return _wwwlFillColor;
            }
            set
            {
                this.SetProperty(ref _wwwlFillColor, value);
            }
        }


        private System.Windows.Media.Brush _FilterFillColor = new SolidColorBrush(Colors.Gray);
        public System.Windows.Media.Brush FilterFillColor
        {
            get
            {
                return _FilterFillColor;
            }
            set
            {
                this.SetProperty(ref _FilterFillColor, value);
            }
        }

        private bool _isZoomChecked = false;
        public bool IsZoomChecked
        {
            get
            {
                return _isZoomChecked;
            }
            set
            {
                this.SetProperty(ref _isZoomChecked, value);
                this.IsZoomHighlighted = value;
            }
        }

        private bool _isMoveChecked = false;
        public bool IsMoveChecked
        {
            get
            {
                return _isMoveChecked;
            }
            set
            {
                this.SetProperty(ref _isMoveChecked, value);
                this.IsMoveHighlighted = value;
            }
        }

        private bool _isRotateChecked = false;
        public bool IsRotateChecked
        {
            get
            {
                return _isRotateChecked;
            }
            set
            {
                this.SetProperty(ref _isRotateChecked, value);
                this.IsRotateHighlighted = value;
            }
        }

        private bool _isInvertChecked = false;
        public bool IsInvertChecked
        {
            get
            {
                return _isInvertChecked;
            }
            set
            {
                this.SetProperty(ref _isInvertChecked, value);
                this.IsInvertHighlighted = value;
            }
        }

        private bool _isWWWLChecked = false;
        public bool IsWWWLChecked
        {
            get
            {
                return _isWWWLChecked;
            }
            set
            {
                this.SetProperty(ref _isWWWLChecked, value);
                //WWWLFillColor = value ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Gray);
                this.IsWWWLHighlighted = value;
            }
        }

        private bool _isFilterChecked = false;
        public bool IsFilterChecked
        {
            get
            {
                return _isFilterChecked;
            }
            set
            {
                this.SetProperty(ref _isFilterChecked, value);
                //FilterFillColor = value ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Gray);
                this.IsFilterHighlighted = value;
            }
        }

        private bool _isResetChecked = false;
        public bool IsResetChecked
        {
            get
            {
                return _isResetChecked;
            }
            set
            {
                this.SetProperty(ref _isResetChecked, value);
                this.IsResetHighlighted = value;
            }
        }

        private bool _isFlipHorizontalChecked = false;
        public bool IsFlipHorizontalChecked
        {
            get
            {
                return _isFlipHorizontalChecked;
            }
            set
            {
                this.SetProperty(ref _isFlipHorizontalChecked, value);
                this.IsFlipHorizontalHighlighted = value;
            }
        }

        private bool _isFlipVerticalChecked = false;
        public bool IsFlipVerticalChecked
        {
            get
            {
                return _isFlipVerticalChecked;
            }
            set
            {
                this.SetProperty(ref _isFlipVerticalChecked, value);
                this.IsFlipVerticalHighlighted = value;
            }
        }

        private bool _isHideTextsChecked = false;
        public bool IsHideTextsChecked
        {
            get
            {
                return _isHideTextsChecked;
            }
            set
            {
                this.SetProperty(ref _isHideTextsChecked, value);
                this.IsHideTextsHighlighted = value;
            }
        }

        private bool _isReferenceLineChecked = false;
        public bool IsReferenceLineChecked
        {
            get
            {
                return _isReferenceLineChecked;
            }
            set
            {
                this.SetProperty(ref _isReferenceLineChecked, value);
                this.IsReferenceLineHighlighted = value;
            }
        }

        private bool _isDeleteChecked = false;
        public bool IsDeleteChecked
        {
            get
            {
                return _isDeleteChecked;
            }
            set
            {
                this.SetProperty(ref _isDeleteChecked, value);
                this.IsDeleteHighlighted = value;
            }
        }

        private bool _isCopyChecked = false;
        public bool IsCopyChecked
        {
            get
            {
                return _isCopyChecked;
            }
            set
            {
                this.SetProperty(ref _isCopyChecked, value);
                this.IsCopyHighlighted = value;
            }
        }

        private bool _isPasteChecked = false;
        public bool IsPasteChecked
        {
            get
            {
                return _isPasteChecked;
            }
            set
            {
                this.SetProperty(ref _isPasteChecked, value);
                this.IsPasteHighlighted = value;
            }
        }

        private bool _isCutChecked = false;
        public bool IsCutChecked
        {
            get
            {
                return _isCutChecked;
            }
            set
            {
                this.SetProperty(ref _isCutChecked, value);
                this.IsCutHighlighted = value;
            }
        }

        private bool _isSelectAllChecked = false;
        public bool IsSelectAllChecked
        {
            get
            {
                return _isSelectAllChecked;
            }
            set
            {
                this.SetProperty(ref _isSelectAllChecked, value);
                this.IsSelectAllHighlighted = value;
            }
        }

        private bool _isUnSelectAllChecked = false;
        public bool IsUnSelectAllChecked
        {
            get
            {
                return _isUnSelectAllChecked;
            }
            set
            {
                this.SetProperty(ref _isUnSelectAllChecked, value);
                this.IsUnSelectAllHighlighted = value;
            }
        }

        private bool _isSplitChecked = false;
        public bool IsSplitChecked
        {
            get
            {
                return _isSplitChecked;
            }
            set
            {
                this.SetProperty(ref _isSplitChecked, value);
                this.IsSplitHighlighted = value;
            }
        }

        private bool _isMergeChecked = false;
        public bool IsMergeChecked
        {
            get
            {
                return _isMergeChecked;
            }
            set
            {
                this.SetProperty(ref _isMergeChecked, value);
                this.IsMergeHighlighted = value;
            }
        }

        private bool _isCreateROIRectChecked = false;
        public bool IsCreateROIRectChecked
        {
            get
            {
                return _isCreateROIRectChecked;
            }
            set
            {
                this.SetProperty(ref _isCreateROIRectChecked, value);
                this.IsCreateROIRectHighlighted = value;
            }
        }

        private bool _isCreateROICircleChecked = false;
        public bool IsCreateROICircleChecked
        {
            get
            {
                return _isCreateROICircleChecked;
            }
            set
            {
                this.SetProperty(ref _isCreateROICircleChecked, value);
                this.IsCreateROICircleHighlighted = value;
            }
        }

        private bool _isCreateROIFreehandChecked = false;
        public bool IsCreateROIFreehandChecked
        {
            get
            {
                return _isCreateROIFreehandChecked;
            }
            set
            {
                this.SetProperty(ref _isCreateROIFreehandChecked, value);
                this.IsCreateROIFreehandHighlighted = value;
            }
        }

        //HightLighted Properties

        private bool _isZoomHighlighted = false;
        public bool IsZoomHighlighted
        {
            get
            {
                return _isZoomHighlighted;
            }
            set
            {
                this.SetProperty(ref _isZoomHighlighted, value);
            }
        }

        private bool _isMoveHighlighted = false;
        public bool IsMoveHighlighted
        {
            get
            {
                return _isMoveHighlighted;
            }
            set
            {
                this.SetProperty(ref _isMoveHighlighted, value);
            }
        }

        private bool _isRotateHighlighted = false;
        public bool IsRotateHighlighted
        {
            get
            {
                return _isRotateHighlighted;
            }
            set
            {
                this.SetProperty(ref _isRotateHighlighted, value);
            }
        }

        private bool _isInvertHighlighted = false;
        public bool IsInvertHighlighted
        {
            get
            {
                return _isInvertHighlighted;
            }
            set
            {
                this.SetProperty(ref _isInvertHighlighted, value);
            }
        }

        private bool _isWWWLHighlighted = false;
        public bool IsWWWLHighlighted
        {
            get
            {
                return _isWWWLHighlighted;
            }
            set
            {
                this.SetProperty(ref _isWWWLHighlighted, value);
                WWWLFillColor = value ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Gray);
            }
        }

        private bool _isFilterHighlighted = false;
        public bool IsFilterHighlighted
        {
            get
            {
                return _isFilterHighlighted;
            }
            set
            {
                this.SetProperty(ref _isFilterHighlighted, value);
                FilterFillColor = value ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Gray);
            }
        }

        private bool _isResetHighlighted = false;
        public bool IsResetHighlighted
        {
            get
            {
                return _isResetHighlighted;
            }
            set
            {
                this.SetProperty(ref _isResetHighlighted, value);
            }
        }

        private bool _isFlipHorizontalHighlighted = false;
        public bool IsFlipHorizontalHighlighted
        {
            get
            {
                return _isFlipHorizontalHighlighted;
            }
            set
            {
                this.SetProperty(ref _isFlipHorizontalHighlighted, value);
            }
        }

        private bool _isFlipVerticalHighlighted = false;
        public bool IsFlipVerticalHighlighted
        {
            get
            {
                return _isFlipVerticalHighlighted;
            }
            set
            {
                this.SetProperty(ref _isFlipVerticalHighlighted, value);
            }
        }

        private bool _isHideTextsHighlighted = false;
        public bool IsHideTextsHighlighted
        {
            get
            {
                return _isHideTextsHighlighted;
            }
            set
            {
                this.SetProperty(ref _isHideTextsHighlighted, value);
            }
        }

        private bool _isReferenceLineHighlighted = false;
        public bool IsReferenceLineHighlighted
        {
            get
            {
                return _isReferenceLineHighlighted;
            }
            set
            {
                this.SetProperty(ref _isReferenceLineHighlighted, value);
            }
        }

        private bool _isDeleteHighlighted = false;
        public bool IsDeleteHighlighted
        {
            get
            {
                return _isDeleteHighlighted;
            }
            set
            {
                this.SetProperty(ref _isDeleteHighlighted, value);
            }
        }

        private bool _IsCopyHighlighted = false;
        public bool IsCopyHighlighted
        {
            get
            {
                return _IsCopyHighlighted;
            }
            set
            {
                this.SetProperty(ref _IsCopyHighlighted, value);
            }
        }



        private bool _IsCutHighlighted = false;
        public bool IsCutHighlighted
        {
            get
            {
                return _IsCutHighlighted;
            }
            set
            {
                this.SetProperty(ref _IsCutHighlighted, value);
            }
        }


        private bool _IsPasteHighlighted = false;
        public bool IsPasteHighlighted
        {
            get
            {
                return _IsPasteHighlighted;
            }
            set
            {
                this.SetProperty(ref _IsPasteHighlighted, value);
            }
        }


        private bool _IsSelectAllHighlighted = false;
        public bool IsSelectAllHighlighted
        {
            get
            {
                return _IsSelectAllHighlighted;
            }
            set
            {
                this.SetProperty(ref _IsSelectAllHighlighted, value);
            }
        }

        private bool _isSplitHighlighted = false;
        public bool IsSplitHighlighted
        {
            get
            {
                return _isSplitHighlighted;
            }
            set
            {
                this.SetProperty(ref _isSplitHighlighted, value);
            }
        }

        private bool _isMergeHighlighted = false;
        public bool IsMergeHighlighted
        {
            get
            {
                return _isMergeHighlighted;
            }
            set
            {
                this.SetProperty(ref _isMergeHighlighted, value);
            }
        }

        private bool _IsUnSelectAllHighlighted = false;
        public bool IsUnSelectAllHighlighted
        {
            get
            {
                return _IsUnSelectAllHighlighted;
            }
            set
            {
                this.SetProperty(ref _IsUnSelectAllHighlighted, value);
            }
        }

        private bool _isCreateROIRectHighlighted = false;
        public bool IsCreateROIRectHighlighted
        {
            get
            {
                return _isCreateROIRectHighlighted;
            }
            set
            {
                this.SetProperty(ref _isCreateROIRectHighlighted, value);
            }
        }

        private bool _isCreateROICircleHighlighted = false;
        public bool IsCreateROICircleHighlighted
        {
            get
            {
                return _isCreateROICircleHighlighted;
            }
            set
            {
                this.SetProperty(ref _isCreateROICircleHighlighted, value);
            }
        }

        private bool _isCreateROIFreehandHighlighted = false;
        public bool IsCreateROIFreehandHighlighted
        {
            get
            {
                return _isCreateROIFreehandHighlighted;
            }
            set
            {
                this.SetProperty(ref _isCreateROIFreehandHighlighted, value);
            }
        }

        private bool _isRemoveAllROIHighlighted = false;
        public bool IsRemoveAllROIHighlighted
        {
            get
            {
                return _isRemoveAllROIHighlighted;
            }
            set
            {
                this.SetProperty(ref _isRemoveAllROIHighlighted, value);
            }
        }

        public OperateCommandsViewModel(ILogger<OperateCommandsViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Commands.Add(PrintConstants.COMMAND_OPERATE_IMAGE, new DelegateCommand<object>(OnOperateImage));

            Commands.Add("MouseEnter", new DelegateCommand<object>(OnMouseEnter));
            Commands.Add("MouseLeave", new DelegateCommand<object>(OnMouseLeave));

            EventAggregator.Instance.GetEvent<DisplayModeChangedEvent>().Subscribe(OnDisplayModeChanged);

            Initialize();
        }

        public void OnMouseEnter(object operationType)
        {
            switch (operationType.ToString())
            {
                case OPERATION_TYPE_ZOOM:
                    this.IsZoomHighlighted = true;
                    break;
                case OPERATION_TYPE_MOVE:
                    this.IsMoveHighlighted = true;
                    break;
                case OPERATION_TYPE_WWWL:
                    this.IsWWWLHighlighted = true;
                    break;
                case OPERATION_TYPE_ROTATE:
                    this.IsRotateHighlighted = true;
                    break;
                case OPERATION_TYPE_INVERT:
                    this.IsInvertHighlighted = true;
                    break;
                case OPERATION_TYPE_RESET_TO_INIT:
                    this.IsResetHighlighted = true;
                    break;
                case OPERATION_TYPE_FLIP_HORIZONTAL:
                    this.IsFlipHorizontalHighlighted = true;
                    break;
                case OPERATION_TYPE_FLIP_VERTICAL:
                    this.IsFlipVerticalHighlighted = true;
                    break;
                case OPERATION_TYPE_FLIP_HIDE_TEXTS:
                    this.IsHideTextsHighlighted = true;
                    break;
                case OPERATION_TYPE_REFERENCE_LINE:
                    this.IsReferenceLineHighlighted = true;
                    break;
                case OPERATION_TYPE_DELETE:
                    this.IsDeleteHighlighted = true;
                    break;
                case OPERATION_TYPE_PASTE:
                    this.IsPasteHighlighted = true;
                    break;
                case OPERATION_TYPE_COPY:
                    this.IsCopyHighlighted = true;
                    break;
                case OPERATION_TYPE_CUT:
                    this.IsCutHighlighted = true;
                    break;
                case OPERATION_TYPE_SELECT_ALL:
                    this.IsSelectAllHighlighted = true;
                    break;
                case OPERATION_TYPE_UNSELECT_ALL:
                    this.IsUnSelectAllHighlighted = true;
                    break;
                case OPERATION_TYPE_FILTER:
                    this.IsFilterHighlighted = true;
                    break;
                case OPERATION_TYPE_SPLIT:
                    this.IsSplitHighlighted = true;
                    break;
                case OPERATION_TYPE_MERGE:
                    this.IsMergeHighlighted = true;
                    break;
                case OPERATION_TYPE_ROI_RECT:
                    this.IsCreateROIRectHighlighted = true;
                    break;
                case OPERATION_TYPE_ROI_CIRCLE:
                    this.IsCreateROICircleHighlighted = true;
                    break;
                case OPERATION_TYPE_ROI_FREEHAND:
                    this.IsCreateROIFreehandHighlighted = true;
                    break;
                case OPERATION_TYPE_ROI_REMOVEALL:
                    this.IsRemoveAllROIHighlighted = true;
                    break;
                default:
                    break;
            }
        }
        public void OnMouseLeave(object operationType)
        {
            switch (operationType.ToString())
            {
                case OPERATION_TYPE_ZOOM:
                    this.IsZoomHighlighted = this.IsZoomChecked ? true : false;
                    break;
                case OPERATION_TYPE_MOVE:
                    this.IsMoveHighlighted = this.IsMoveChecked ? true : false;
                    break;
                case OPERATION_TYPE_WWWL:
                    this.IsWWWLHighlighted = this.IsWWWLChecked ? true : false;
                    break;
                case OPERATION_TYPE_ROTATE:
                    this.IsRotateHighlighted = false;
                    break;
                case OPERATION_TYPE_INVERT:
                    this.IsInvertHighlighted = false;
                    break;
                case OPERATION_TYPE_RESET_TO_INIT:
                    this.IsResetHighlighted = false;
                    break;
                case OPERATION_TYPE_FLIP_HORIZONTAL:
                    this.IsFlipHorizontalHighlighted = false;
                    break;
                case OPERATION_TYPE_FLIP_VERTICAL:
                    this.IsFlipVerticalHighlighted = false;
                    break;
                case OPERATION_TYPE_FLIP_HIDE_TEXTS:
                    this.IsHideTextsHighlighted = false;
                    break;
                case OPERATION_TYPE_REFERENCE_LINE:
                    this.IsReferenceLineHighlighted = false;
                    break;
                case OPERATION_TYPE_DELETE:
                    this.IsDeleteHighlighted = false;
                    break;
                case OPERATION_TYPE_PASTE:
                    this.IsPasteHighlighted = false;
                    break;
                case OPERATION_TYPE_COPY:
                    this.IsCopyHighlighted = false;
                    break;
                case OPERATION_TYPE_CUT:
                    this.IsCutHighlighted = false;
                    break;
                case OPERATION_TYPE_SELECT_ALL:
                    this.IsSelectAllHighlighted = false;
                    break;
                case OPERATION_TYPE_UNSELECT_ALL:
                    this.IsUnSelectAllHighlighted = false;
                    break;
                case OPERATION_TYPE_FILTER:
                    this.IsFilterHighlighted = false;
                    break;
                case OPERATION_TYPE_SPLIT:
                    this.IsSplitHighlighted = false;
                    break;
                case OPERATION_TYPE_MERGE:
                    this.IsMergeHighlighted = false;
                    break;
                case OPERATION_TYPE_ROI_RECT:
                    this.IsCreateROIRectHighlighted = this.IsCreateROIRectChecked;
                    break;
                case OPERATION_TYPE_ROI_CIRCLE:
                    this.IsCreateROICircleHighlighted = this.IsCreateROICircleChecked;
                    break;
                case OPERATION_TYPE_ROI_FREEHAND:
                    this.IsCreateROIFreehandHighlighted = this.IsCreateROIFreehandChecked;
                    break;
                case OPERATION_TYPE_ROI_REMOVEALL:
                    this.IsRemoveAllROIHighlighted = false;
                    break;
                default:
                    break;
            }
        }

        private void Initialize()
        {
            // Load wwwl mennItems
            var windowTypes = UserConfig.WindowingConfig.Windowings;
            if (windowTypes is null) windowTypes = new System.Collections.Generic.List<WindowingInfo>();

            //添加自定义ww/wl
            windowTypes.Add(new WindowingInfo()
            {
                Width = new ItemField<int> { Value = 350, Default = 350 },
                Level = new ItemField<int> { Value = 20, Default = 20 },
                BodyPart = "Custom",
                Shortcut = "F12",
                Description = "Custom"
            });
            foreach (var windowType in windowTypes)
            {
                windowType.Description = $"{windowType.BodyPart} ({windowType.Shortcut})";
            }

            WWWLItems = windowTypes.ToObservableCollection();

            // Load Kernel mennItems
            var kernelTypes = UserConfig.KernelConfig.Kernels;
            if (kernelTypes is null) kernelTypes = new System.Collections.Generic.List<Config.KernelType>();

            foreach (var kernelType in kernelTypes)
            {
                kernelType.Description = $"{kernelType.Kernel}"; // ({kernelType.Shortcut})
            }

            FilterItems = kernelTypes.ToObservableCollection();
        }

        public void OnClickWWWLMenuItem(WindowingInfo windowType)
        {
            var imageViewer = Global.Instance.ImageViewer;
            try
            {
                if (windowType.BodyPart == OPERATION_CUSTOM)
                {
                    //Custom情况下，自定义输入值
                    ShowCustomWWWLWindow();
                }
                else
                {
                    imageViewer.SetWWWL((double)windowType.Width.Value, (double)windowType.Level.Value);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"parse ww/wl error {ex.Message}");
            }
        }
        public void OnClickFilterMenuItem(Config.KernelType filterType)
        {
            var imageViewer = Global.Instance.ImageViewer;
            imageViewer.Kernel(filterType.Kernel.ToString());
        }
        public void OnOperateImage(object operationType)
        {
            var imageViewer = Global.Instance.ImageViewer;
            switch (operationType.ToString())
            {
                case OPERATION_TYPE_ZOOM:
                    var isZoomChecked = this.IsZoomChecked;
                    this.ResetExclusiveButtonsState();
                    this.IsZoomChecked = isZoomChecked;
                    if (this.IsZoomChecked)
                        imageViewer.Zoom();
                    else
                        imageViewer.ResetMouseSelector();
                    break;
                case OPERATION_TYPE_MOVE:
                    var isMoveChecked = this.IsMoveChecked;
                    this.ResetExclusiveButtonsState();
                    this.IsMoveChecked = isMoveChecked;
                    if (this.IsMoveChecked)
                        imageViewer.Move();
                    else
                        imageViewer.ResetMouseSelector();
                    break;
                case OPERATION_TYPE_WWWL:
                    this.ResetExclusiveButtonsState();
                    imageViewer.SetWWWL();
                    break;
                case OPERATION_TYPE_ROTATE:
                    this.IsRotateChecked = false;
                    imageViewer.Rotate(90);
                    this.IsRotateHighlighted = true;
                    break;
                case OPERATION_TYPE_INVERT:
                    this.IsInvertChecked = false;
                    imageViewer.Invert();
                    this.IsInvertHighlighted = true;
                    break;
                case OPERATION_TYPE_RESET_TO_INIT:
                    this.IsResetChecked = false; 
                    imageViewer.Reset();
                    this.IsResetHighlighted = true;
                    break;
                case OPERATION_TYPE_FLIP_HORIZONTAL:
                    this.IsFlipHorizontalChecked = false;
                    imageViewer.FlipHorizontal();
                    this.IsFlipHorizontalHighlighted = true;
                    break;
                case OPERATION_TYPE_FLIP_VERTICAL:
                    this.IsFlipVerticalChecked = false;
                    imageViewer.FlipVertical();
                    this.IsFlipVerticalHighlighted = true;
                    break;
                case OPERATION_TYPE_FLIP_HIDE_TEXTS:
                    this.IsHideTextsChecked = false;
                    imageViewer.ToggleOverlay();
                    this.IsHideTextsHighlighted = true;
                    break;
                case OPERATION_TYPE_REFERENCE_LINE:
                    this.IsReferenceLineChecked = false;
                    if (IsTomoCheck())
                    {
                        var seriesListViewModel = CTS.Global.ServiceProvider.GetService<SeriesListViewModel>();
                        var topo = seriesListViewModel.CurrentPrintingStudy?.PrintingSeriesModelList.FirstOrDefault(s => s.ImageType.ToLower() == "topo");
                        if (topo is not null)
                        {
                            string filePath = "";
                            if (Directory.Exists(topo.SeriesPath))
                            {
                                filePath = Directory.GetFiles(topo.SeriesPath).FirstOrDefault();
                            }
                            else if (File.Exists(topo.SeriesPath))
                            {
                                filePath = topo.SeriesPath;
                            }
                            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                            {
                                imageViewer.InsertLocationImage(filePath);
                            }
                            else
                            {
                                _dialogService.ShowDialog(false,
                                    MPS.UI.Dialog.Enum.MessageLeveles.Warning,
                                    LanguageResource.Message_Warning_Title,
                                    LanguageResource.Message_Warning_Topo_Missing,
                                    null, ConsoleSystemHelper.WindowHwnd);
                            }
                        }
                    }
                    else
                    {
                        _dialogService.ShowDialog(false,
                            MPS.UI.Dialog.Enum.MessageLeveles.Warning,
                            LanguageResource.Message_Warning_Title,
                            LanguageResource.Message_Warning_Is_Not_Tomo,
                            null, ConsoleSystemHelper.WindowHwnd);
                    }
                    this.IsReferenceLineHighlighted = true;
                    break;
                case OPERATION_TYPE_COPY:
                    this.IsCopyChecked = false;
                    imageViewer.Copy();
                    this.IsCopyHighlighted = true;
                    break;
                case OPERATION_TYPE_PASTE:
                    this.IsPasteChecked = false;
                    imageViewer.Paste();
                    this.IsPasteHighlighted = true;
                    break;
                case OPERATION_TYPE_CUT:
                    this.IsCutChecked = false;
                    imageViewer.Cut();
                    this.IsCutHighlighted = true;
                    break;

                case OPERATION_TYPE_DELETE:
                    this.IsDeleteChecked = false;
                    imageViewer.Delete();
                    this.IsDeleteHighlighted = true;
                    break;
                case OPERATION_TYPE_SELECT_ALL:
                    this.IsSelectAllChecked = false;
                    imageViewer.SelectAll();
                    this.IsSelectAllChecked = true;
                    break;
                case OPERATION_TYPE_UNSELECT_ALL:
                    this.IsUnSelectAllChecked = false;
                    imageViewer.UnSelectAll();
                    this.IsUnSelectAllChecked = true;
                    break;
                case OPERATION_TYPE_FILTER:
                    this.ResetExclusiveButtonsState();
                    break;
                case OPERATION_TYPE_SPLIT:
                    this.IsSplitChecked= false;
                    imageViewer.SplitCell(2, 2);
                    this.IsSplitHighlighted = true;
                    break;
                case OPERATION_TYPE_MERGE:
                    this.IsMergeChecked = false;
                    imageViewer.MergeCell();
                    this.IsMergeHighlighted = true;
                    break;
                case OPERATION_TYPE_ROI_RECT:
                    var isCreateROIRectChecked = this.IsCreateROIRectChecked;
                    ResetExclusiveButtonsState();
                    this.IsCreateROIRectChecked = isCreateROIRectChecked;
                    if (this.IsCreateROIRectChecked)
                    {
                        imageViewer.CreateROI(NVCTImageViewerInterop.ROIType.ROI_Rect);
                    }
                    else
                    {
                        imageViewer.ResetMouseSelector();
                    }
                    break;
                case OPERATION_TYPE_ROI_CIRCLE:
                    var isCreateROICircleChecked = this.IsCreateROICircleChecked;
                    ResetExclusiveButtonsState();
                    this.IsCreateROICircleChecked = isCreateROICircleChecked;
                    if (this.IsCreateROICircleChecked)
                    {
                        imageViewer.CreateROI(NVCTImageViewerInterop.ROIType.ROI_Circle);
                    }
                    else
                    {
                        imageViewer.ResetMouseSelector();
                    }
                    break;
                case OPERATION_TYPE_ROI_FREEHAND:
                    var isCreateROIFreehandChecked = this.IsCreateROIFreehandChecked;
                    ResetExclusiveButtonsState();
                    this.IsCreateROIFreehandChecked = isCreateROIFreehandChecked;
                    if (this.IsCreateROIFreehandChecked)
                    {
                        imageViewer.CreateROI(NVCTImageViewerInterop.ROIType.ROI_PolygonClosed);
                    }
                    else
                    {
                        imageViewer.ResetMouseSelector();
                    }
                    break;
                case OPERATION_TYPE_ROI_REMOVEALL:
                    ResetExclusiveButtonsState();
                    imageViewer.RemoveAllROI();
                    break;
                default:
                    break;
            }
        }

        private void ShowCustomWWWLWindow()
        {
            _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();

            if (_customWwwlWindow is null)
            {
                _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();
            }

            if (_customWwwlWindow != null)
            {
                WindowDialogShow.DialogShow(_customWwwlWindow);
            }
        }

        private void OnDisplayModeChanged(PrintDisplayMode printDisplayMode)
        {
            IsEditable = printDisplayMode == PrintDisplayMode.Browser;
        }

        private void ResetExclusiveButtonsState()
        {
            this.IsZoomChecked = false;
            this.IsMoveChecked = false;
            this.IsWWWLChecked = false;
            this.IsFilterChecked = false;
            this.IsCreateROIRectChecked = false;
            this.IsCreateROICircleChecked = false;
            this.IsCreateROIFreehandChecked = false;
        }

        private bool IsTomoCheck()
        {
            string path = Global.Instance.ImageViewer.GetSelectedRenderViewDataPath();
            if (!string.IsNullOrEmpty(path) && Path.Exists(path))
            {
                var tags = DicomImageHelper.Instance.GetDicomDetails(path);
                var tag = tags.FirstOrDefault(t => t.TagID.ToLower() == "(0008,0008)"); // Image Type
                var imageType = tag?.TagValue.ToLower();
                if (imageType == "original\\primary\\axial" || imageType == "original\\primary\\helical")
                    return true;
            }

            return false;
        }
    }
}
