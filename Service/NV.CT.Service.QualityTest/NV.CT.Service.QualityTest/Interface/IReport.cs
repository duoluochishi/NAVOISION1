using System.Text;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models;

namespace NV.CT.Service.QualityTest.Interface
{
    interface IReport
    {
        void ReportContent(StringBuilder sb, ItemModel item, ReportAllowType allowType);
    }
}