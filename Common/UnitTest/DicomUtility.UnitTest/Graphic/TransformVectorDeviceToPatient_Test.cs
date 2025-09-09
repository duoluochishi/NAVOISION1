using MathNet.Numerics.Integration;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace NV.CT.DicomUtility.UnitTest.Graphic
{
    public class TransformVectorDeviceToPatient_Test
    {

        [TestCase(1, 0, 0, PatientPosition.HFS, 1, 0, 0)]
        [TestCase(1, 0, 0, PatientPosition.HFP, -1, 0, 0)]
        [TestCase(1, 0, 0, PatientPosition.HFDL, 0, -1, 0)]
        [TestCase(1, 0, 0, PatientPosition.HFDR, 0, 1, 0)]
        [TestCase(1, 0, 0, PatientPosition.FFS, -1, 0, 0)]
        [TestCase(1, 0, 0, PatientPosition.FFP, 1, 0, 0)]
        [TestCase(1, 0, 0, PatientPosition.FFDL, 0, 1, 0)]
        [TestCase(1, 0, 0, PatientPosition.FFDR, 0, -1, 0)]
        public void TransformVectorDeviceToPatientTest_TOPOOnTB(double vx,double vy,double vz,PatientPosition pp,double rx,double ry,double rz)
        {
            var result = CoordinateConverter.Instance.TransformVectorDeviceToPatient(pp, vx, vy, vz);
            Assert.That(result.vx, Is.EqualTo(rx));
            Assert.That(result.vy, Is.EqualTo(ry));
            Assert.That(result.vz, Is.EqualTo(rz));
        }


        [TestCase(0, -1, 0, PatientPosition.HFS, 0, -1, 0)]
        [TestCase(0, -1, 0, PatientPosition.HFP, 0, 1, 0)]
        [TestCase(0, -1, 0, PatientPosition.HFDL, -1, 0, 0)]
        [TestCase(0, -1, 0, PatientPosition.HFDR, 1, 0, 0)]
        [TestCase(0, -1, 0, PatientPosition.FFS, 0, -1, 0)]
        [TestCase(0, -1, 0, PatientPosition.FFP, 0, 1, 0)]
        [TestCase(0, -1, 0, PatientPosition.FFDL, -1, 0, 0)]
        [TestCase(0, -1, 0, PatientPosition.FFDR, 1, 0, 0)]
        public void TransformVectorDeviceToPatientTest_TOPOOnLR(double vx, double vy, double vz, PatientPosition pp, double rx, double ry, double rz)
        {
            var result = CoordinateConverter.Instance.TransformVectorDeviceToPatient(pp, vx, vy, vz);
            Assert.That(result.vx, Is.EqualTo(rx));
            Assert.That(result.vy, Is.EqualTo(ry));
            Assert.That(result.vz, Is.EqualTo(rz));
        }
    }
}
