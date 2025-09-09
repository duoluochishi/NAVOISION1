using NUnit.Framework;
using NV.CT.CTS.Enums;
using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.UnitTest.Graphic
{
    public class CoordinateConverterTests_ImageOrder
    {
        [TestCase(PatientPosition.HFS,ImageOrders.HeadFoot,TableDirection.In)]
        [TestCase(PatientPosition.HFP, ImageOrders.HeadFoot, TableDirection.In)]
        [TestCase(PatientPosition.HFDL, ImageOrders.HeadFoot, TableDirection.In)]
        [TestCase(PatientPosition.HFDR, ImageOrders.HeadFoot, TableDirection.In)]
        [TestCase(PatientPosition.HFS, ImageOrders.FootHead, TableDirection.Out)]
        [TestCase(PatientPosition.HFP, ImageOrders.FootHead, TableDirection.Out)]
        [TestCase(PatientPosition.HFDL, ImageOrders.FootHead, TableDirection.Out)]
        [TestCase(PatientPosition.HFDR, ImageOrders.FootHead, TableDirection.Out)]
        [TestCase(PatientPosition.FFS, ImageOrders.HeadFoot, TableDirection.Out)]
        [TestCase(PatientPosition.FFP, ImageOrders.HeadFoot, TableDirection.Out)]
        [TestCase(PatientPosition.FFDL, ImageOrders.HeadFoot, TableDirection.Out)]
        [TestCase(PatientPosition.FFDR, ImageOrders.HeadFoot, TableDirection.Out)]
        [TestCase(PatientPosition.FFS, ImageOrders.FootHead, TableDirection.In)]
        [TestCase(PatientPosition.FFP, ImageOrders.FootHead, TableDirection.In)]
        [TestCase(PatientPosition.FFDL, ImageOrders.FootHead, TableDirection.In)]
        [TestCase(PatientPosition.FFDR, ImageOrders.FootHead, TableDirection.In)]
        public void TestGetTableDirectionByImageOrder(PatientPosition pp, ImageOrders imageOrder, TableDirection expectedTableDirection)
        {
            Assert.That(CoordinateConverter.Instance.GetTableDirectionByImageOrder(pp, imageOrder),
                Is.EqualTo(expectedTableDirection));
        }

        [TestCase(PatientPosition.HFS, TableDirection.In,ImageOrders.HeadFoot )]
        [TestCase(PatientPosition.HFP, TableDirection.In, ImageOrders.HeadFoot)]
        [TestCase(PatientPosition.HFDL, TableDirection.In, ImageOrders.HeadFoot)]
        [TestCase(PatientPosition.HFDR, TableDirection.In, ImageOrders.HeadFoot)]
        [TestCase(PatientPosition.HFS, TableDirection.Out, ImageOrders.FootHead)]
        [TestCase(PatientPosition.HFP, TableDirection.Out, ImageOrders.FootHead)]
        [TestCase(PatientPosition.HFDL, TableDirection.Out, ImageOrders.FootHead)]
        [TestCase(PatientPosition.HFDR, TableDirection.Out, ImageOrders.FootHead)]

        [TestCase(PatientPosition.FFS, TableDirection.In, ImageOrders.FootHead)]
        [TestCase(PatientPosition.FFP, TableDirection.In, ImageOrders.FootHead)]
        [TestCase(PatientPosition.FFDL, TableDirection.In, ImageOrders.FootHead)]
        [TestCase(PatientPosition.FFDR, TableDirection.In, ImageOrders.FootHead)]
        [TestCase(PatientPosition.FFS, TableDirection.Out, ImageOrders.HeadFoot)]
        [TestCase(PatientPosition.FFP, TableDirection.Out, ImageOrders.HeadFoot)]
        [TestCase(PatientPosition.FFDL, TableDirection.Out, ImageOrders.HeadFoot)]
        [TestCase(PatientPosition.FFDR, TableDirection.Out, ImageOrders.HeadFoot)]
        public void TestGetImageOrderByTableDirection(PatientPosition pp, TableDirection tableDirection, ImageOrders expectedImageOrder )
        {
            Assert.That(CoordinateConverter.Instance.GetImageOrderByTableDirection(pp, tableDirection),
                Is.EqualTo(expectedImageOrder));
        }
    }
}
