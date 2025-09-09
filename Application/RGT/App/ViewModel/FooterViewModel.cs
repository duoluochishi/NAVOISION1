namespace NV.CT.RGT.ViewModel;

public class FooterViewModel : BaseViewModel
{
    private readonly IDataSync _dataSync;
    private PatientModel? _patientModel;
    public PatientModel? PatientModel
    {
        get => _patientModel;
        set => SetProperty(ref _patientModel, value);
    }

    private StudyModel? _studyModel;
    public StudyModel? StudyModel
    {
        get => _studyModel;
        set => SetProperty(ref _studyModel, value);
    }

    public FooterViewModel(IDataSync dataSync)
    {
        _dataSync = dataSync;

        _dataSync.ExamCloseFinished += DataSync_ExamCloseFinished;
        _dataSync.NormalExamFinished += DataSync_NormalExamFinished;
        _dataSync.EmergencyExamFinished += DataSync_EmergencyExamFinished;

        Commands.Add("SettingCommand", new DelegateCommand(SettingCommand, () => true));
    }

    /// <summary>
    /// 处理急诊病人
    /// </summary>
    private void DataSync_EmergencyExamFinished(object? sender, EventArgs e)
    {
        RefreshData();
    }

    /// <summary>
    /// 检查启动后，同步检查数据
    /// </summary>
    private void DataSync_NormalExamFinished(object? sender, EventArgs e)
    {
        RefreshData();
    }

    private void RefreshData()
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            var (studyInfo, patientInfo) = _dataSync.GetCurrentStudyInfo();

            StudyModel = studyInfo;
            PatientModel = patientInfo;
        });
    }

    /// <summary>
    /// 检查关闭
    /// </summary>
    private void DataSync_ExamCloseFinished(object? sender, EventArgs e)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            StudyModel = null;
            PatientModel = null;
        });
    }

    private void SettingCommand()
    {
        var settingWindow = CTS.Global.ServiceProvider.GetService<SettingWindow>();
        settingWindow?.ShowDialog();
    }
}