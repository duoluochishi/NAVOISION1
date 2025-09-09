using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Interface;

namespace NV.CT.Service.QualityTest.Utilities.Report
{
    internal static class ReportContentFactory
    {
        public static IReport? CreateReportContent(QTType type)
        {
            return type switch
            {
                QTType.SliceThicknessAxial => new SingleValueReport(),
                QTType.SliceThicknessHelical => new SingleValueReport(),
                QTType.Homogeneity => new HomogeneityReport(),
                QTType.CTOfWater => new SingleValueReport(),
                QTType.NoiseOfWater => new SingleValueReport(),
                QTType.ContrastScale => new SingleValueReport(),
                QTType.MTF_XY => new MTFReport(),
                QTType.MTF_Z => new MTFReport(),
                _ => null,
            };
        }
    }
}
