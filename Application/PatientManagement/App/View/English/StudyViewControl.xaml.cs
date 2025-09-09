//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.Language;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace NV.CT.PatientManagement.View.English;

public partial class StudyViewControl
{
    private IStudyListColumnsConfigService _studyListColumnsConfigService;
    public StudyViewControl()
    {
        InitializeComponent();
        DataContext = Global.Instance.ServiceProvider.GetRequiredService<StudyViewModel>();
        _studyListColumnsConfigService = Global.Instance.ServiceProvider.GetRequiredService<IStudyListColumnsConfigService>();
        _studyListColumnsConfigService.ConfigRefreshed += OnColumnsConfigRefreshed;
        Loaded += PageLoaded;
    }

    private void PageLoaded(object sender, RoutedEventArgs e)
    {
        this.ApplyColumnsConfig();
    }

    private void OnColumnsConfigRefreshed(object? sender, EventArgs e)
    {
        this.ApplyColumnsConfig();
    }

    private void ApplyColumnsConfig()
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            var columnsConfigs = _studyListColumnsConfigService.GetConfigs().ColumnItems.Items.OrderBy(c => { return c.SortNumber; });
            foreach (var columnConfig in columnsConfigs)
            {
                ResetColumnConfig(columnConfig);
            }
        });
    }

    private void ResetColumnConfig(ColumnItem columnConfig)
    {
        DataGridColumn? datagridColumn = null;
        switch (columnConfig.ItemName)
        {
            case StudyListColumn.PatientID:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_PatientId);
                break;
            case StudyListColumn.PatientName:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Name);
                break;
            case StudyListColumn.Birthday:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Birthday);
                break;
            case StudyListColumn.Sex:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Sex);
                break;
            case StudyListColumn.BodyPartExam:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_BodyPart);
                break;
            case StudyListColumn.StudyStatus:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Study_Status);
                break;
            case StudyListColumn.ArchiveStatus:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Archive);
                break;
            case StudyListColumn.PrintStatus:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Print);
                break;
            case StudyListColumn.CorrectionStatus:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Correct);
                break;
            case StudyListColumn.StudyTime:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Study_Time);
                break;
            case StudyListColumn.Technician:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Technician);
                break;
            case StudyListColumn.Ward:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Ward);
                break;
            case StudyListColumn.ReferringPhysician:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_ReferringPhysician);
                break;
            case StudyListColumn.CreateTime:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_CreatedTime);
                break;
            case StudyListColumn.IsLocked:
                datagridColumn = dgStudy.Columns.FirstOrDefault(c => c.Header.ToString() == LanguageResource.Header_Lock);
                break;
            default:
                break;

        }

        if (datagridColumn is not null)
        {
            datagridColumn.DisplayIndex = columnConfig.SortNumber;
            datagridColumn.Visibility = columnConfig.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}