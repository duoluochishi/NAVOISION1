using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Utilities.Report;

namespace NV.CT.Service.QualityTest.Utilities
{
    internal static class ReportUtility
    {
        public static void CreateReport(string filePath, IList<ItemModel> items, ReportHeadInfoModel headInfo, ReportAllowType allowType)
        {
            var sb = Report(items, headInfo, allowType);
            var folder = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder!);
            }

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            using var sw = new StreamWriter(fs);
            sw.Write(sb);
        }

        private static StringBuilder Report(IList<ItemModel> items, ReportHeadInfoModel headInfo, ReportAllowType allowType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html xmlns:msxsl=\"urn:schemas-microsoft-com:xslt\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            sb.AppendLine($"<Title>{Quality_Lang.Quality_Report_Title}</Title>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine($"<H1 style=\"text-align: center;\">{Quality_Lang.Quality_Report_Title}</H1><br>");
            sb.AppendLine("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"table-layout: fixed;\">");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_GenerationData} {headInfo.GenerationDate:dd/MM/yyyy HH:mm:ss}</H4></td>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_LastData} {headInfo.LastSessionDate:dd/MM/yyyy HH:mm:ss}</H4></td>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_Reason} {headInfo.Reason}</H4></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_SystemSN} {headInfo.SystemSN}</H4></td>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_Name} {headInfo.CustomerName}</H4></td>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_Address} {headInfo.CustomerAddress}</H4></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_PhantomSN} {headInfo.PhantomSN}</H4></td>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_Version} {headInfo.SoftwareVersion}</H4></td>");
            sb.AppendLine($"<td><H4>{Quality_Lang.Quality_Report_Executed} {headInfo.ExecutedBy}</H4></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<hr color=\"Black\"><br>");

            foreach (var item in items)
            {
                var reportContent = ReportContentFactory.CreateReportContent(item.QTType);
                reportContent?.ReportContent(sb, item, allowType);
            }

            sb.AppendLine("<br><hr color=\"Black\">");
            sb.AppendLine($"<H4 style=\"text-align: right;\">{Quality_Lang.Quality_Report_Signature}____________</H4>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb;
        }

        public static string MaxMinMsg(double value)
        {
            return value switch
            {
                double.MaxValue or double.MinValue => Quality_Lang.Quality_Report_Unlimited,
                _ => value.ToString(CultureInfo.CurrentCulture)
            };
        }

        public static List<ItemEntryModel>? GetAllowItemEntries(ReportAllowType allowType, IList<ItemEntryModel>? items)
        {
            return (allowType switch
                       {
                           ReportAllowType.All => items,
                           ReportAllowType.OfflineReconSucceed => items?.Where(i => i.IsOfflineReconSucceed),
                           ReportAllowType.Check => items?.Where(i => i.IsChecked),
                           ReportAllowType.CheckAndOffLineReconSucceed => items?.Where(i => i is { IsOfflineReconSucceed: true, IsChecked: true }),
                           _ => null
                       })?.ToList();
        }
    }
}