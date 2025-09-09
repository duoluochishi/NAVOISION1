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

using NV.CT.DicomUtility.DicomIOD;
using NV.CT.DicomUtility.DoseReportSR;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using System.Windows.Documents;
using System.Windows.Media;

namespace NV.CT.PatientManagement.ViewModel;

public class DoseReportDetailViewModel : BaseViewModel
{
    private string _detailTitle = "Dose SR";
    public string DetailTitle
    {
        get => _detailTitle;
        set => SetProperty(ref _detailTitle, value);
    }

    private ReportItem? _reportItem;

    public ReportItem? ReportItem
    {
        get => _reportItem;
        set => SetProperty(ref _reportItem, value);
    }

    private XRayRadiationDoseSRIOD? _radiationInfo;
    public XRayRadiationDoseSRIOD? RadiationInfo
    {
        get => _radiationInfo;
        set => SetProperty(ref _radiationInfo, value);
    }

    private FlowDocument? _doseReportDocument = new();

    public FlowDocument? DoseReportDocument
    {
        get => _doseReportDocument;
        set => SetProperty(ref _doseReportDocument, value);
    }

    private readonly ISeriesApplicationService _seriesApplicationService;
    public DoseReportDetailViewModel(ISeriesApplicationService iSeriesApplicationService)
    {
        Commands.Add("CloseCommand", new DelegateCommand<object>(Close, _ => true));
        Commands.Add("DragMoveCommand", new DelegateCommand<object>(DragMove, _ => true));

        _seriesApplicationService = iSeriesApplicationService;
        _seriesApplicationService.ReportItemChanged += SeriesApplicationService_ReportItemChanged;
    }

    private void SeriesApplicationService_ReportItemChanged(object? sender, EventArgs e)
    {
        if (ReportItem is not null && ReportItem.Children is not null && ReportItem.Children.Any())
        {
            ConstructHeaderPart();

            AddChildrenItems(ReportItem.Children, 0);
        }
    }

    private void ConstructHeaderPart()
    {
        if (ReportItem is null)
            return;
        if (DoseReportDocument?.Blocks.Count >0)
            DoseReportDocument?.Blocks.Clear();
        //标题
        var titleParagraph = new Paragraph(new Run($"{ReportItem?.Key}"));
        titleParagraph.FontWeight = FontWeights.Bold;
        titleParagraph.FontSize = 28;
        DoseReportDocument?.Blocks.Add(titleParagraph);

        //时间
        var studyDate = $"{RadiationInfo?.GeneralStudyModule.StudyDate:yyyy-MM-dd HH:mm:ss}";
        var dateParagraph = new Paragraph(new Run($"{studyDate}"));
        DoseReportDocument?.Blocks.Add(dateParagraph);

        //表格
        var table = new Table();
        var column01 = new TableColumn();
        var column02 = new TableColumn();
        var column03 = new TableColumn();
        table.Columns.Add(column01);
        table.Columns.Add(column02);
        table.Columns.Add(column03);

        //第一行
        var titleRow = new TableRow();
        titleRow.Cells.Add(ConstructCell("Patient"));
        titleRow.Cells.Add(ConstructCell("Study"));
        titleRow.Cells.Add(ConstructCell("Report Status"));

        //第二行
        var row2 = new TableRow();
        row2.Cells.Add(ConstructCell($"Patient's Name: {RadiationInfo?.PatientModule.PatientName}"));
        row2.Cells.Add(ConstructCell($"Study Date: {RadiationInfo?.GeneralStudyModule.StudyDate}"));
        row2.Cells.Add(ConstructCell($"Completion Flag: {RadiationInfo?.SRDocumentGeneralModule.CompletionFlag}"));

        //第三行
        var row3 = new TableRow();
        row3.Cells.Add(ConstructCell($"Patient ID: {RadiationInfo?.PatientModule.PatientID}"));
        row3.Cells.Add(ConstructCell($"Study ID: {RadiationInfo?.GeneralStudyModule.StudyID}"));
        row3.Cells.Add(ConstructCell($"Verification Flag: {RadiationInfo?.SRDocumentGeneralModule.VerificationFlag}"));

        //第四行
        var row4 = new TableRow();
        row4.Cells.Add(ConstructCell($"Patient's Birth Date: {RadiationInfo?.PatientModule.PatientBirthDate}"));
        row4.Cells.Add(ConstructCell($"Accession Number: {RadiationInfo?.GeneralStudyModule.AccessionNumber}"));

        //第五行
        var row5 = new TableRow();
        row5.Cells.Add(ConstructCell($"Patient's Sex: {RadiationInfo?.PatientModule.PatientSex}"));
        row5.Cells.Add(ConstructCell($"Referring Physician's Name: {RadiationInfo?.GeneralStudyModule.ReferringPhysicianName}"));

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(titleRow);
        rowGroup.Rows.Add(row2);
        rowGroup.Rows.Add(row3);
        rowGroup.Rows.Add(row4);
        rowGroup.Rows.Add(row5);

        table.RowGroups.Add(rowGroup);
        DoseReportDocument?.Blocks.Add(table);

        table.BorderThickness = new Thickness(0, 0, 0, 3);
        table.BorderBrush = new SolidColorBrush(Colors.White);
        table.Padding = new Thickness(5, 5, 5, 30);

    }

    private TableCell ConstructCell(string content)
    {
        var cell = new TableCell();
        cell.Blocks.Add(new Paragraph(new Run(content)));
        return cell;
    }

    private void AddChildrenItems(List<ReportItem> items, int depth)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var eachItem = items[i];

            string tag;
            if (depth == 0)
            {
                tag = $"1.{i + 1} ";
            }
            else
            {
                tag = $"{new string('\t', depth)}{i + 1}.";
            }

            var paragraph = new Paragraph(new Run($"{tag} {eachItem.Key} : {eachItem.Value}"));
            DoseReportDocument?.Blocks.Add(paragraph);

            if (eachItem.Children is not null && eachItem.Children.Any())
            {
                AddChildrenItems(eachItem.Children, depth + 1);
            }
        }
    }

    public void Close(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    public void DragMove(object parameter)
    {
        if (parameter is Window window)
        {
            window.DragMove();
        }
    }
}