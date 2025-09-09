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
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="VStudy.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

using Module = Autofac.Module;

namespace NV.CT.PatientManagement.ViewModel;

public class ViewModelModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<StudyViewModel>().SingleInstance();
        builder.RegisterType<SeriesViewModel>().SingleInstance();
        builder.RegisterType<InstanceViewModel>().SingleInstance();
        builder.RegisterType<DoseReportDetailViewModel>().SingleInstance();

        builder.RegisterType<FilmPlayViewModel>().SingleInstance();
        builder.RegisterType<DataExportViewModel>().SingleInstance();
        builder.RegisterType<DataImportViewModel>().SingleInstance();
        builder.RegisterType<RawDataImportViewModel>().SingleInstance();
        builder.RegisterType<AddEditFolderViewModel>().SingleInstance();
        builder.RegisterType<CorrectViewModel>().SingleInstance();
        builder.RegisterType<RawDataManagementViewModel>().SingleInstance();
        builder.RegisterType<StudyListColumnsConfigViewModel>().SingleInstance();
        builder.RegisterType<StudyListFilterViewModel>().SingleInstance();

        builder.RegisterType<DoseReportDetailWindow>().SingleInstance();
        builder.RegisterType<DataExportWindow>().SingleInstance();
        builder.RegisterType<DataImportWindow>().SingleInstance();
        builder.RegisterType<FiltrationViewModel>().SingleInstance();
        builder.RegisterType<AddEditFolderWindow>().SingleInstance();
        builder.RegisterType<CorrectWindow>().SingleInstance();
        builder.RegisterType<RawDataManagementWindow>().SingleInstance();
        builder.RegisterType<StudyListColumnsConfigWindow>().SingleInstance();
        builder.RegisterType<StudyListFilterWindow>().SingleInstance();
        builder.RegisterType<RawDataImportWindow>().SingleInstance();

    }
}