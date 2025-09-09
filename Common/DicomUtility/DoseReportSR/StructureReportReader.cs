//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using FellowOakDicom.StructuredReport;
using NV.CT.DicomUtility.DicomIOD;
using System.Diagnostics;

namespace NV.CT.DicomUtility.DoseReportSR
{
    public class StructureReportReader
    {
        private static StructureReportReader _structureReportReader;

        public static StructureReportReader Instance
        {
            get
            {
                if (null == _structureReportReader)
                {
                    _structureReportReader = new StructureReportReader();
                }
                return _structureReportReader;
            }
        }

        private StructureReportReader()
        {
        }

        public ReportItem GetStructuredReportItem(string path)
        {
            var df = DicomFile.Open(path);
            var ds = df.Dataset;

            var sr = new DicomStructuredReport(ds);
            try
            {
                return ReadContent(sr);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(@"Can't get Structure Report Item from {" + path + "},{0}", ex.Message);
                return null;
            }
        }

        public XRayRadiationDoseSRIOD GetXRayRadiationDoseSRIOD(string path)
        {
            var df = DicomFile.Open(path);
            var ds = df.Dataset;

            var result = new XRayRadiationDoseSRIOD();
            result.Read(ds);

            return result;
        }

        private ReportItem ReadContent(DicomContentItem sr)
        {
            object value = null;
            DicomValueType type = sr.Type;
            DicomDataset dataset = sr.Dataset;
            try
            {
                switch (type)
                {
                    case DicomValueType.Container:
                        value = null;
                        break;
                    case DicomValueType.Code:
                        {
                            DicomCodeItem codesr = sr.Dataset.GetCodeItem(DicomTag.ConceptCodeSequence);
                            value = ((!(codesr is null)) ? codesr.Meaning : string.Empty);
                            break;
                        }
                    case DicomValueType.Numeric:
                        {
                            DicomMeasuredValue measuredValue = sr.Dataset.GetMeasuredValue(DicomTag.MeasuredValueSequence);
                            value = ((measuredValue is not null) ? measuredValue.ToString() : string.Empty);
                            break;
                        }
                    case DicomValueType.Text:
                        value = sr.Dataset.GetValueOrDefault(DicomTag.TextValue, 0, string.Empty);
                        break;
                    case DicomValueType.PersonName:
                        value = sr.Dataset.GetValueOrDefault(DicomTag.PersonName, 0, string.Empty);
                        break;
                    case DicomValueType.Date:
                        value = dataset.GetValueOrDefault(DicomTag.Date, 0, string.Empty);
                        break;
                    case DicomValueType.Time:
                        value = dataset.GetValueOrDefault(DicomTag.Time, 0, string.Empty);
                        break;
                    case DicomValueType.DateTime:
                        value = dataset.GetValueOrDefault(DicomTag.DateTime, 0, string.Empty);
                        break;
                    case DicomValueType.UIDReference:
                        value = dataset.GetSingleValue<string>(DicomTag.UID);
                        break;
                }
            }
            catch (Exception ex)
            {
                value = ex.Message;
            }

            ReportItem reportItem = new ReportItem();
            reportItem.Key = sr.Code.Meaning;
            reportItem.Type = dataset.GetValueOrDefault(DicomTag.ValueType, 0, "UNKNOWN");
            reportItem.Value = value;
            if (sr.Children().Count() > 0)
            {
                List<ReportItem> list = new List<ReportItem>();
                foreach (DicomContentItem contentItem in sr.Children())
                {
                    list.Add(ReadContent(contentItem));
                }

                reportItem.Children = list;
            }

            return reportItem;
        }
    }

    //
    // 摘要:
    //     结构化报告元素
    public class ReportItem
    {
        //
        // 摘要:
        //     键名
        public string Key { get; internal set; }

        //
        // 摘要:
        //     类型
        public string Type { get; internal set; }

        //
        // 摘要:
        //     值
        public object Value { get; internal set; }

        //
        // 摘要:
        //     子元素
        public List<ReportItem> Children { get; internal set; }
    }
}
