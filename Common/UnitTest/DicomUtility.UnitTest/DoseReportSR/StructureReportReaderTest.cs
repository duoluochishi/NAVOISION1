using NUnit.Framework;
using NV.CT.CTS;
using NV.CT.DicomUtility.DoseReportSR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomUtility.UnitTest.DoseReportSR
{
    public class StructureReportReaderTest
    {
        [Test]
        public void GetReportItemTest()
        {
            Global.Logger = Global.Logger;

            var path = @"d:\test_structurereport.dcm";
            var reportItem = StructureReportReader.Instance.GetStructuredReportItem(path);

            var iod = StructureReportReader.Instance.GetXRayRadiationDoseSRIOD(path);

            Assert.That(reportItem is not null);
            Assert.That(iod is not null);

            PrintReporItem(reportItem);
        }

        private void PrintReporItem(ReportItem ri,string prefix = "")
        {
            Trace.TraceInformation($"{prefix}/{ri.Key}:{ri.Value}");

            if (ri.Children is null)
                return;
            foreach(var subRi in ri.Children)
            {
                PrintReporItem(subRi,prefix + "--");
            }
        }
    }
}
