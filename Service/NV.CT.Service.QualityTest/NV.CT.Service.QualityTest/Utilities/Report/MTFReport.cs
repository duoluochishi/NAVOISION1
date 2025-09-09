using System.Linq;
using System.Text;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Interface;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;

namespace NV.CT.Service.QualityTest.Utilities.Report
{
    internal class MTFReport : IReport
    {
        public void ReportContent(StringBuilder sb, ItemModel item, ReportAllowType allowType)
        {
            var entries = ReportUtility.GetAllowItemEntries(allowType, item.Entries);

            if (entries == null || entries.Count == 0)
            {
                return;
            }

            sb.AppendLine("<table border=\"1\" align=\"center\" width=\"95%\" cellSpacing=\"0\" style=\"border-collapse: collapse; table-layout: auto; border-color:#000000; color: black;\">");
            sb.AppendLine($"<th colspan=\"100\">{item.Name}</th>");
            sb.AppendLine("<tr bgcolor=\"#cccccc\" align=\"center\">");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_ScanNo}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_BodyPart}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_Parameters}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_Items}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_First}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_Middle}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_Last}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_Min}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_Max}</td>");
            sb.AppendLine($"<td>{Quality_Lang.Quality_Report_Reference}</td>");
            sb.AppendLine($"<td>{Common_Lang.Common_Status}</td>");
            sb.AppendLine("</tr>");

            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Param is not MTFParamModel param)
                {
                    continue;
                }

                // 基本信息 and MTF 0%
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td rowspan=\"4\">{i + 1}</td>");
                sb.AppendLine($"<td rowspan=\"4\">{param.ScanParam.BodyPart}</td>");
                sb.AppendLine($"<td rowspan=\"4\">{param.ScanParam.Voltage.FirstOrDefault()} kV, {param.ScanParam.Current.FirstOrDefault()} mA</td>");
                sb.AppendLine("<td>MTF 0%</td>");
                sb.AppendLine($"<td>{param.Value.FirstMTF0Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.MediumMTF0Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.LastMTF0Value:F2}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MinMTF0Value)}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MaxMTF0Value)}</td>");
                sb.AppendLine($"<td>{param.ValidateParam.ReferenceMTF0Value}</td>");
                sb.AppendLine($"<td rowspan=\"4\">{(entries[i].Result == true ? "<font color=\"#009900\">Passed" : "<font color=\"#FF0000\">Failed")}</font></td>");
                sb.AppendLine("</tr>");

                // MTF 2%
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>MTF 2%</td>");
                sb.AppendLine($"<td>{param.Value.FirstMTF2Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.MediumMTF2Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.LastMTF2Value:F2}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MinMTF2Value)}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MaxMTF2Value)}</td>");
                sb.AppendLine($"<td>{param.ValidateParam.ReferenceMTF2Value}</td>");
                sb.AppendLine("</tr>");

                // MTF 10%
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>MTF 10%</td>");
                sb.AppendLine($"<td>{param.Value.FirstMTF10Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.MediumMTF10Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.LastMTF10Value:F2}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MinMTF10Value)}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MaxMTF10Value)}</td>");
                sb.AppendLine($"<td>{param.ValidateParam.ReferenceMTF10Value}</td>");
                sb.AppendLine("</tr>");

                // MTF 50%
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>MTF 50%</td>");
                sb.AppendLine($"<td>{param.Value.FirstMTF50Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.MediumMTF50Value:F2}</td>");
                sb.AppendLine($"<td>{param.Value.LastMTF50Value:F2}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MinMTF50Value)}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MaxMTF50Value)}</td>");
                sb.AppendLine($"<td>{param.ValidateParam.ReferenceMTF50Value}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
        }
    }
}