//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.MPS.Environment;

namespace NV.CT.UI.Exam.ViewModel;

public class VoiceAPIConfigViewModel : BaseViewModel
{
    private readonly IVoiceService _voiceService;
    private readonly IProtocolHostService _protocolHostService;
    private readonly ISelectionManager _selectionManager;
    List<VoiceModel> voiceModels = new List<VoiceModel>();

    private ScanModel CurrentScanModel = new ScanModel();

    private bool _isDefault = false;
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    private KeyValuePair<string, string> _selectedLanguage = new();
    public KeyValuePair<string, string> SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                InitComboxData(value.Key);
            }
        }
    }

    private ObservableCollection<KeyValuePair<string, string>> _languageList = new();
    public ObservableCollection<KeyValuePair<string, string>> LanguageList
    {
        get => _languageList;
        set => SetProperty(ref _languageList, value);
    }

    private ObservableCollection<KeyValuePair<ushort, string>> _preVoiceList = new();
    public ObservableCollection<KeyValuePair<ushort, string>> PreVoiceList
    {
        get => _preVoiceList;
        set => SetProperty(ref _preVoiceList, value);
    }

    private KeyValuePair<ushort, string> _selectedPreVoice = new KeyValuePair<ushort, string>();
    public KeyValuePair<ushort, string> SelectedPreVoice
    {
        get => _selectedPreVoice;
        set => SetProperty(ref _selectedPreVoice, value);
    }

    private ObservableCollection<KeyValuePair<ushort, string>> _postVoiceList = new();
    public ObservableCollection<KeyValuePair<ushort, string>> PostVoiceList
    {
        get => _postVoiceList;
        set => SetProperty(ref _postVoiceList, value);
    }

    private KeyValuePair<ushort, string> _selectedPostVoice = new KeyValuePair<ushort, string>();
    public KeyValuePair<ushort, string> SelectedPostVoice
    {
        get => _selectedPostVoice;
        set => SetProperty(ref _selectedPostVoice, value);
    }

    public VoiceAPIConfigViewModel(IVoiceService voiceService,
        IProtocolHostService protocolHostService,
        ISelectionManager selectionManager)
    {
        _voiceService = voiceService;
        _protocolHostService = protocolHostService;
        _selectionManager = selectionManager;

        Commands.Add(CommandParameters.COMMAND_LOAD, new DelegateCommand<object>(Loaded, _ => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
        InitData();

        _selectionManager.SelectionScanChanged -= SelectionManager_SelectionScanChanged;
        _selectionManager.SelectionScanChanged += SelectionManager_SelectionScanChanged;
        if (_selectionManager is not null && _selectionManager.CurrentSelection.Scan is not null)
        {
            SetInitData(_selectionManager.CurrentSelection.Scan as ScanModel);
        }
    }

    [UIRoute]
    private void SelectionManager_SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        if (e is null || e.Data is null)
        {
            return;
        }
        SetInitData(e.Data as ScanModel);
    }

    private void SetInitData(ScanModel scanModel)
    {
        CurrentScanModel = scanModel;
        if (scanModel.IsVoiceSupported)
        {
            if (scanModel.PreVoiceId > 0)
            {
                var voiceModel = _voiceService.GetVoiceInfo(scanModel.PreVoiceId.ToString());
                if (voiceModel is not null && !string.IsNullOrEmpty(PreVoiceList.FirstOrDefault(t => t.Key.Equals(voiceModel.InternalId)).Value))
                {
                    SelectedPreVoice = PreVoiceList.FirstOrDefault(t => t.Key.Equals(voiceModel.InternalId));
                }
                else
                {
                    SelectedPreVoice = PreVoiceList[0];
                }
            }
            else
            {
                SelectedPreVoice = PreVoiceList[0];
            }

            if (scanModel.PostVoiceId > 0)
            {
                var voiceModel = _voiceService.GetVoiceInfo(scanModel.PostVoiceId.ToString());
                if (voiceModel is not null && !string.IsNullOrEmpty(PostVoiceList.FirstOrDefault(t => t.Key.Equals(voiceModel.InternalId)).Value))
                {
                    SelectedPostVoice = PostVoiceList.FirstOrDefault(t => t.Key.Equals(voiceModel.InternalId));
                }
                else
                {
                    SelectedPostVoice = PostVoiceList[0];
                }
            }
            else
            {
                SelectedPostVoice = PostVoiceList[0];
            }
        }
    }

    private void InitData()
    {
        voiceModels = _voiceService.GetValidVoices();
        LanguageList = new ObservableCollection<KeyValuePair<string, string>>();

        LanguageList.Add(new KeyValuePair<string, string>("0", "None"));
        if (voiceModels is not null)
        {
            foreach (var item in voiceModels)
            {
                if (LanguageList.Count(t => t.Key == item.Language) == 0)
                {
                    LanguageList.Add(new KeyValuePair<string, string>(item.Language, item.Language));
                }
            }
        }
        if (LanguageList.Count > 0)
        {
            SelectedLanguage = LanguageList[0];
        }
    }

    private void InitComboxData(string language)
    {
        PreVoiceList = new ObservableCollection<KeyValuePair<ushort, string>>();
        PostVoiceList = new ObservableCollection<KeyValuePair<ushort, string>>();
        SelectedPreVoice = new KeyValuePair<ushort, string>();
        SelectedPostVoice = new KeyValuePair<ushort, string>();

        var no = new KeyValuePair<ushort, string>(0, "None");
        PreVoiceList.Add(no);
        PostVoiceList.Add(no);

        var list = voiceModels.ToList();
        if (!string.IsNullOrEmpty(language) && !language.Equals("0"))
        {
            list = voiceModels.FindAll(t => t.Language.Equals(language)).ToList();
        }
        foreach (var item in list.ToList())
        {
            if (item.IsFront)
            {
                if (PreVoiceList.Count(t => t.Key == item.InternalId) == 0)
                {
                    PreVoiceList.Add(new KeyValuePair<ushort, string>(item.InternalId, item.Name));
                }
            }
            else
            {
                if (PostVoiceList.Count(t => t.Key == item.InternalId) == 0)
                {
                    PostVoiceList.Add(new KeyValuePair<ushort, string>(item.InternalId, item.Name));
                }
            }
        }
        List<VoiceModel> dList = _voiceService.GetDefaultList();
        InitPreDefault(dList);
        InitPostDefault(dList);
    }

    private void InitPreDefault(List<VoiceModel> list)
    {
        //前语音的默认选中       
        var fd = list.FirstOrDefault(t => t.IsDefault && t.IsFront);
        if (fd is null)
        {
            if (PreVoiceList.Count > 0)
            {
                SelectedPreVoice = PreVoiceList[0];
            }
        }
        else
        {
            var d = PreVoiceList.FirstOrDefault(t => t.Key == fd.InternalId);
            if (string.IsNullOrEmpty(d.Value))
            {
                d = new KeyValuePair<ushort, string>(fd.InternalId, fd.Name);
                PreVoiceList.Add(d);
            }
            SelectedPreVoice = d;
        }
    }

    private void InitPostDefault(List<VoiceModel> list)
    {
        //后语音的默认选中
        var nfd = list.FirstOrDefault(t => t.IsDefault && !t.IsFront);
        if (nfd is null)
        {
            if (PostVoiceList.Count > 0)
            {
                SelectedPostVoice = PostVoiceList[0];
            }
        }
        else
        {
            var d = PostVoiceList.FirstOrDefault(t => t.Key == nfd.InternalId);
            if (string.IsNullOrEmpty(d.Value))
            {
                d = new KeyValuePair<ushort, string>(nfd.InternalId, nfd.Name);
                PostVoiceList.Add(d);
            }
            SelectedPostVoice = d;
        }
    }

    private void Loaded(object parameter)
    {
        if (CurrentScanModel is null || CurrentScanModel.Status != PerformStatus.Unperform)
        {
            if (parameter is Window colseWindow)
            {
                colseWindow.Hide();
            }
            return;
        }
        if (IsDefault)
        {
            SetDefaultVoice();
        }
        List<ParameterModel> parameterModels = new List<ParameterModel>();
        var model = _voiceService.GetVoiceInfo(SelectedPreVoice.Key.ToString());
        if (model is not null)
        {
            parameterModels.Add(new ParameterModel
            {
                Name = ProtocolParameterNames.SCAN_IS_VOICE_SUPPORTED,
                Value = true.ToString(CultureInfo.InvariantCulture)
            });
            parameterModels.Add(new ParameterModel
            {
                Name = ProtocolParameterNames.SCAN_PRE_VOICE_PLAY_TIME,
                Value = UnitConvert.Second2Microsecond((int)model.VoiceLength).ToString(CultureInfo.InvariantCulture)
            });
        }
        parameterModels.Add(new ParameterModel
        {
            Name = ProtocolParameterNames.SCAN_PRE_VOICE_ID,
            Value = SelectedPreVoice.Key.ToString(CultureInfo.InvariantCulture)
        });
        parameterModels.Add(new ParameterModel
        {
            Name = ProtocolParameterNames.SCAN_POST_VOICE_ID,
            Value = SelectedPostVoice.Key.ToString(CultureInfo.InvariantCulture)
        });
        if (CurrentScanModel is ScanModel scanModel && parameterModels.Count > 0)
        {
            _protocolHostService.SetParameters(scanModel, parameterModels);

            ExposureDelayTimeHelper.CorrectDelayTimeMeasurement(_protocolHostService, _voiceService, scanModel.Descriptor.Id);
        }
        IsDefault = false;
        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    private void SetDefaultVoice()
    {
        var preD = voiceModels.FirstOrDefault(t => t.InternalId == SelectedPreVoice.Key);
        var postD = voiceModels.FirstOrDefault(t => t.InternalId == SelectedPostVoice.Key);
        List<VoiceModel> list = new List<VoiceModel>();
        if (preD is null)
        {
            list.Add(new VoiceModel { InternalId = SelectedPreVoice.Key, IsDefault = true, IsValid = true, IsFront = true });
        }
        else
        {
            list.Add(preD);
        }

        if (postD is null)
        {
            list.Add(new VoiceModel { InternalId = SelectedPostVoice.Key, IsDefault = true, IsValid = true, IsFront = false });
        }
        else
        {
            list.Add(postD);
        }
        Task.Run(() =>
        {
            _voiceService.SetDefaultList(list);
        });
    }

    public void Closed(object parameter)
    {
        IsDefault = false;
        if (parameter is Window window)
        {
            window.Hide();
        }
    }
}