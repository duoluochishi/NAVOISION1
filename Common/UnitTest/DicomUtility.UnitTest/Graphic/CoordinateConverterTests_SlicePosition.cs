using NUnit.Framework;
using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.UnitTest.Graphic
{
    public class CoordinateConverterTests_SlicePosition
    {
        [TestCase(-22000,-22000)]
        [TestCase(0,0)]
        [TestCase(-11000,-11000)]
        public void TestSlicePositionOnHF(double deviceSP, double patientSP)
        {
            //Device To Patient
            Assert.That(patientSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.HFS, deviceSP))); 
            Assert.That(patientSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.HFP, deviceSP)));
            Assert.That(patientSP,  
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.HFDL, deviceSP)));
            Assert.That(patientSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.HFDR, deviceSP)));

            //Patient To Device
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.HFS, patientSP)));
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.HFP, patientSP)));
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.HFDL, patientSP)));
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.HFDR, patientSP)));
        }

        [TestCase(-22000, 22000)]
        [TestCase(0, 0)]
        [TestCase(-11000, 11000)]
        public void TestSlicePositionOnFF(double deviceSP, double patientSP)
        {
            //Device To Patient
            Assert.That(patientSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.FFS, deviceSP)));
            Assert.That(patientSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.FFP, deviceSP)));
            Assert.That(patientSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.FFDL, deviceSP)));
            Assert.That(patientSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(PatientPosition.FFDR, deviceSP)));

            //Patient To Device
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.FFS, patientSP)));
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.FFP, patientSP)));
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.FFDL, patientSP)));
            Assert.That(deviceSP,
                Is.EqualTo(CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(PatientPosition.FFDR, patientSP)));
        }
    }
}
