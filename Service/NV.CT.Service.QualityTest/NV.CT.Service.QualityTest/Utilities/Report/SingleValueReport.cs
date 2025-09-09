using System.Linq;
using System.Text;
using NV.CT.Service.QualityTest.Interface;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;

namespace NV.CT.Service.QualityTest.Utilities.Report
{
    internal class SingleValueReport : IReport
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
                if (entries[i].Param is not SingleValueParamModel param)
                {
                    continue;
                }

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{i + 1}</td>");
                sb.AppendLine($"<td>{param.ScanParam.BodyPart}</td>");
                sb.AppendLine($"<td>{param.ScanParam.Voltage.FirstOrDefault()} kV, {param.ScanParam.Current.FirstOrDefault()} mA</td>");
                sb.AppendLine($"<td>{param.Value.FirstValue:F2}</td>");
                sb.AppendLine($"<td>{param.Value.MediumValue:F2}</td>");
                sb.AppendLine($"<td>{param.Value.LastValue:F2}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MinValue)}</td>");
                sb.AppendLine($"<td>{ReportUtility.MaxMinMsg(param.ValidateParam.MaxValue)}</td>");
                sb.AppendLine($"<td>{param.ValidateParam.ReferenceValue}</td>");
                sb.AppendLine($"<td>{(entries[i].Result == true ? "<font color=\"#009900\">Passed" : "<font color=\"#FF0000\">Failed")}</font></td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
        }
    }
}