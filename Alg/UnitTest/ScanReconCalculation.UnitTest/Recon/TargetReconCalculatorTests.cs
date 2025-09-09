using NUnit.Framework;
using NV.CT.Alg.ScanReconCalculation.Recon.Target;
using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ScanReconCalculation.UnitTest.Recon
{
    public class TargetReconCalculatorTests
    {
        [Test]
        public void TestNullResultForNotSupportedScanOption()
        {
            var input = new TargetReconInput();
            input.ScanOption = FacadeProxy.Common.Enums.ScanOption.None;
            var output = TargetReconCalculator.Instance.GetTargetReconParams(input);
            Assert.That(output, Is.Null);
        }


        [Test]
        public void TestNotNullResultForSupportedScanOption()
        {
            var input = new TargetReconInput();
            input.ScanOption = FacadeProxy.Common.Enums.ScanOption.Axial;
            var output = TargetReconCalculator.Instance.GetTargetReconParams(input);
            Assert.That(output, Is.Not.Null);
        }

        [TestCase(ScanOption.Axial, PatientPosition.HFS, -1000000, -1200000, 47520, 39930, 8000, 12000, -1184240, -1004170)]
        [TestCase(ScanOption.Helical, PatientPosition.HFS, -1000000, -1200000, 47520, 39930, 8000, 12000, -1224170, -964240)]

        public void GetROIReconParamForAxial(ScanOption scanOption, PatientPosition pp,
            int centerfirstz, int centerlastz, int fullsw, int collimatedsw, int predeletelength, int postdeletelength,
            int expectedMin,int expectedMax)
        {
            var input = new TargetReconInput(scanOption, pp,
                fullsw, collimatedsw, 0,-2000000,
                100,0, centerfirstz,
                100,0, centerlastz,
                506880,506880, predeletelength, postdeletelength);
            var result = TargetReconCalculator.Instance.GetTargetReconParams(input);
            Assert.That(result.TablePositionMax, Is.EqualTo(expectedMax));
            Assert.That(result.TablePositionMin, Is.EqualTo(expectedMin));
            Assert.That(result.roiFovCenterX, Is.EqualTo(100));
            Assert.That(result.roiFovCenterY, Is.EqualTo(0));
            Assert.That(result.IsTargetRecon, Is.True);
        }
    }
}
