using NUnit.Framework;
using NV.CT.DicomUtility.Graphic;

namespace NV.CT.DicomUtility.UnitTest.Graphic
{
    public class ScanLengthCorrectionHelperTests
    {
        [TestCase(0,200,200,20000,200)]
        [TestCase(200, 200, 200, 20000, 200)]
        [TestCase(400, 200, 200, 20000, 400)]
        [TestCase(500, 200, 200, 20000, 600)]
        [TestCase(1500, 200, 200, 20000, 1600)]
        [TestCase(20000, 200, 200, 20000, 20000)]
        [TestCase(19999, 200, 200, 20000, 20000)]
        [TestCase(20001, 200, 200, 20000, 20000)]
        [TestCase(20000, 200, 200, 19999, 19800)]
        public void TestCorrectedSurviewScanLength(int length, int surviewActuralSize,int minLength,int maxLength,int expectedLength)
        {
            Assert.That(ScanLengthCorrectionHelper.GetCorrectedSurviewScanLength(length, surviewActuralSize, minLength, maxLength), Is.EqualTo(expectedLength));
        }

        [TestCase(0, 472, 400, 200, 20000,472)]
        [TestCase(250, 472, 400, 200, 20000, 472)]
        [TestCase(473, 472, 400, 200, 20000, 872)]
        [TestCase(900, 472, 400, 200, 20000, 1272)]
        [TestCase(20000, 472, 400, 200, 20000, 19672)]
        public void TestCorrectedAxialScanLength(int length, int ad_DetectorSize,int tableFeed, int minLength, int maxLength, int expectedLength)
        {
            Assert.That(ScanLengthCorrectionHelper.GetCorrectedAxialScanLength(length, tableFeed, ad_DetectorSize, minLength, maxLength), Is.EqualTo(expectedLength));
        }

        [TestCase(0, 3,  200, 20000, 210)]
        [TestCase(0, 3, 400, 20000, 420)]
        [TestCase(20000, 3, 200, 20000, 19980)]
        [TestCase(400, 0.165, 200, 20000, 400)]
        [TestCase(401, 1.5, 200, 20000, 405)]
        [TestCase(401, 1.5, 200, 20000, 405)]
        [TestCase(401, 0.165, 200, 20000, 402)]
        [TestCase(402, 0.165, 200, 20000, 402)]
        [TestCase(403, 0.165, 200, 20000, 404)]
        [TestCase(404, 0.165, 200, 20000, 404)]
        [TestCase(400, 0.33, 200, 20000, 402)]
        [TestCase(401, 0.66, 200, 20000, 402)]
        public void GetCorrectedHelicalScanLength(int length, double sliceThickness, int minLength, int maxLength, int expectedLength)
        {
            //Assert.That(ScanLengthCorrectionHelper.GetCorrectedHelicalScanLength(length, sliceThickness, minLength, maxLength), Is.EqualTo(expectedLength));

        }
    }
}
