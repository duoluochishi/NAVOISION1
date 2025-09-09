using NUnit.Framework;
using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.UnitTest.Graphic
{
    public class CoordinateConverterTests_ImageOrientation
    {
        private readonly Vector3D DeviceTopoOritationForHF_X = new Vector3D(1,0,0);
        private readonly Vector3D DeviceTopoOritationForHF_Y = new Vector3D(0,-1,0);
        private readonly Vector3D DeviceTopoOritationForHF_Z = new Vector3D(0,0,-1);

        private readonly Vector3D DeviceTopoOritationForFF_X = new Vector3D(1, 0, 0);
        private readonly Vector3D DeviceTopoOritationForFF_Y = new Vector3D(0, -1, 0);
        private readonly Vector3D DeviceTopoOritationForFF_Z = new Vector3D(0, 0, 1);

        private readonly Vector3D DeviceAxialOritation_X = new Vector3D(1,0,0);
        private readonly Vector3D DeviceAxialOritation_Y = new Vector3D(0, 1, 0);

        //HFS_TB(1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFS_TBX = new Vector3D(1, 0, 0);
        private readonly Vector3D TopoOritationForHFS_TBY = new Vector3D(0,0,-1);

        //HFS_LR(0,-1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFS_LRX = new Vector3D(0, -1, 0);
        private readonly Vector3D TopoOritationForHFS_LRY = new Vector3D(0, 0, -1);

        //HFS_Axial(1,0,0)/(0,1,0)
        private readonly Vector3D AxialOritationForHFS_X = new Vector3D(1, 0, 0);
        private readonly Vector3D AxialOritationForHFS_Y = new Vector3D(0, 1, 0);

        //HFP_TB(-1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFP_TBX = new Vector3D(-1, 0, 0);
        private readonly Vector3D TopoOritationForHFP_TBY = new Vector3D(0, 0, -1);

        //HFP_LR(0,1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFP_LRX = new Vector3D(0, 1, 0);
        private readonly Vector3D TopoOritationForHFP_LRY = new Vector3D(0, 0, -1);

        //HFP_Axial(-1,0,0)/(0,-1,0)
        private readonly Vector3D AxialOritationForHFP_X = new Vector3D(-1, 0, 0);
        private readonly Vector3D AxialOritationForHFP_Y = new Vector3D(0, -1, 0);

        //HFL_TB(0,-1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFL_TBX = new Vector3D(0, -1, 0);
        private readonly Vector3D TopoOritationForHFL_TBY = new Vector3D(0, 0, -1);

        //HFL_LR(-1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFL_LRX = new Vector3D(-1, 0, 0);
        private readonly Vector3D TopoOritationForHFL_LRY = new Vector3D(0, 0, -1);

        //HFL_Axial(0,-1,0)/(1,0,0)
        private readonly Vector3D AxialOritationForHFL_X = new Vector3D(0, -1, 0);
        private readonly Vector3D AxialOritationForHFL_Y = new Vector3D(1, 0, 0);

        //HFR_TB(0,1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFR_TBX = new Vector3D(0, 1, 0);
        private readonly Vector3D TopoOritationForHFR_TBY = new Vector3D(0, 0, -1);

        //HFR_LR(1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForHFR_LRX = new Vector3D(1, 0, 0);
        private readonly Vector3D TopoOritationForHFR_LRY = new Vector3D(0, 0, -1);

        //HFR_Axial(0,1,0)/(-1,0,0)
        private readonly Vector3D AxialOritationForHFR_X = new Vector3D(0, 1, 0);
        private readonly Vector3D AxialOritationForHFR_Y = new Vector3D(-1, 0, 0);

        //FFS_TB(-1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFS_TBX = new Vector3D(-1, 0, 0);
        private readonly Vector3D TopoOritationForFFS_TBY = new Vector3D(0, 0, -1);

        //FFS_LR(0,-1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFS_LRX = new Vector3D(0, -1, 0);
        private readonly Vector3D TopoOritationForFFS_LRY = new Vector3D(0, 0, -1);

        //FFS_Axial(-1,0,0)/(0,1,0)
        private readonly Vector3D AxialOritationForFFS_X = new Vector3D(-1, 0, 0);
        private readonly Vector3D AxialOritationForFFS_Y = new Vector3D(0, 1, 0);

        //FFP_TB(1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFP_TBX = new Vector3D(1, 0, 0);
        private readonly Vector3D TopoOritationForFFP_TBY = new Vector3D(0, 0, -1);

        //FFP_LR(0,1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFP_LRX = new Vector3D(0, 1, 0);
        private readonly Vector3D TopoOritationForFFP_LRY = new Vector3D(0, 0, -1);

        //FFP_Axial(1,0,0)/(0,-1,0)
        private readonly Vector3D AxialOritationForFFP_X = new Vector3D(1, 0, 0);
        private readonly Vector3D AxialOritationForFFP_Y = new Vector3D(0, -1, 0);

        //FFL_TB(0,1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFL_TBX = new Vector3D(0, 1, 0);
        private readonly Vector3D TopoOritationForFFL_TBY = new Vector3D(0, 0, -1);

        //FFL_LR(-1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFL_LRX = new Vector3D(-1, 0, 0);
        private readonly Vector3D TopoOritationForFFL_LRY = new Vector3D(0, 0, -1);

        //FFL_Axial(0,1,0)/(1,0,0)
        private readonly Vector3D AxialOritationForFFL_X = new Vector3D(0, 1, 0);
        private readonly Vector3D AxialOritationForFFL_Y = new Vector3D(1, 0, 0);

        //FFR_TB(0,-1,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFR_TBX = new Vector3D(0, -1, 0);
        private readonly Vector3D TopoOritationForFFR_TBY = new Vector3D(0, 0, -1);

        //FFR_LR(1,0,0)/(0,0,-1)
        private readonly Vector3D TopoOritationForFFR_LRX = new Vector3D(1, 0, 0);
        private readonly Vector3D TopoOritationForFFR_LRY = new Vector3D(0, 0, -1);

        //FFR_Axial(0,-1,0)/(-1,0,0)
        private readonly Vector3D AxialOritationForFFR_X = new Vector3D(0, -1, 0);
        private readonly Vector3D AxialOritationForFFR_Y = new Vector3D(-1, 0, 0);


        [Test]
        public void TestImageOrientationOnHFS()
        {
            //Device to Patient
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFS, DeviceTopoOritationForHF_X),
                Is.EqualTo(TopoOritationForHFS_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFS, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFS_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFS, DeviceTopoOritationForHF_Y),
                Is.EqualTo(TopoOritationForHFS_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFS, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFS_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFS, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForHFS_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFS, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForHFS_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFS, TopoOritationForHFS_TBX),
                Is.EqualTo(DeviceTopoOritationForHF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFS, TopoOritationForHFS_TBY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFS, TopoOritationForHFS_LRX),
                Is.EqualTo(DeviceTopoOritationForHF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFS, TopoOritationForHFS_LRY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFS, AxialOritationForHFS_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFS, AxialOritationForHFS_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }


        [Test]
        public void TestImageOrientationOnHFP()
        {
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFP, DeviceTopoOritationForHF_X),
                Is.EqualTo(TopoOritationForHFP_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFP, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFP_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFP, DeviceTopoOritationForHF_Y),
                Is.EqualTo(TopoOritationForHFP_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFP, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFP_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFP, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForHFP_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFP, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForHFP_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFP, TopoOritationForHFP_TBX),
                Is.EqualTo(DeviceTopoOritationForHF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFP, TopoOritationForHFP_TBY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFP, TopoOritationForHFP_LRX),
                Is.EqualTo(DeviceTopoOritationForHF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFP, TopoOritationForHFP_LRY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFP, AxialOritationForHFP_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFP, AxialOritationForHFP_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }
        [Test]
        public void TestImageOrientationOnHFL()
        {
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDL, DeviceTopoOritationForHF_X),
                Is.EqualTo(TopoOritationForHFL_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDL, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFL_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDL, DeviceTopoOritationForHF_Y),
                Is.EqualTo(TopoOritationForHFL_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDL, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFL_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDL, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForHFL_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDL, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForHFL_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDL, TopoOritationForHFL_TBX),
                Is.EqualTo(DeviceTopoOritationForHF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDL, TopoOritationForHFL_TBY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDL, TopoOritationForHFL_LRX),
                Is.EqualTo(DeviceTopoOritationForHF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDL, TopoOritationForHFL_LRY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDL, AxialOritationForHFL_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDL, AxialOritationForHFL_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }
        [Test]
        public void TestImageOrientationOnHFR()
        {
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDR, DeviceTopoOritationForHF_X),
                Is.EqualTo(TopoOritationForHFR_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDR, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFR_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDR, DeviceTopoOritationForHF_Y),
                Is.EqualTo(TopoOritationForHFR_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDR, DeviceTopoOritationForHF_Z),
                Is.EqualTo(TopoOritationForHFR_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDR, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForHFR_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.HFDR, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForHFR_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDR, TopoOritationForHFR_TBX),
                Is.EqualTo(DeviceTopoOritationForHF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDR, TopoOritationForHFR_TBY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDR, TopoOritationForHFR_LRX),
                Is.EqualTo(DeviceTopoOritationForHF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDR, TopoOritationForHFR_LRY),
                Is.EqualTo(DeviceTopoOritationForHF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDR, AxialOritationForHFR_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.HFDR, AxialOritationForHFR_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }


        [Test]
        public void TestImageOrientationOnFFS()
        {
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFS, DeviceTopoOritationForFF_X),
                Is.EqualTo(TopoOritationForFFS_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFS, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFS_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFS, DeviceTopoOritationForFF_Y),
                Is.EqualTo(TopoOritationForFFS_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFS, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFS_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFS, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForFFS_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFS, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForFFS_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFS, TopoOritationForFFS_TBX),
                Is.EqualTo(DeviceTopoOritationForFF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFS, TopoOritationForFFS_TBY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFS, TopoOritationForFFS_LRX),
                Is.EqualTo(DeviceTopoOritationForFF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFS, TopoOritationForFFS_LRY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFS, AxialOritationForFFS_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFS, AxialOritationForFFS_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }

        [Test]
        public void TestImageOrientationOnFFP()
        {
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFP, DeviceTopoOritationForFF_X),
                Is.EqualTo(TopoOritationForFFP_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFP, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFP_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFP, DeviceTopoOritationForFF_Y),
                Is.EqualTo(TopoOritationForFFP_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFP, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFP_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFP, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForFFP_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFP, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForFFP_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFP, TopoOritationForFFP_TBX),
                Is.EqualTo(DeviceTopoOritationForFF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFP, TopoOritationForFFP_TBY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFP, TopoOritationForFFP_LRX),
                Is.EqualTo(DeviceTopoOritationForFF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFP, TopoOritationForFFP_LRY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFP, AxialOritationForFFP_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFP, AxialOritationForFFP_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }

        [Test]
        public void TestImageOrientationOnFFL()
        {
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDL, DeviceTopoOritationForFF_X),
                Is.EqualTo(TopoOritationForFFL_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDL, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFL_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDL, DeviceTopoOritationForFF_Y),
                Is.EqualTo(TopoOritationForFFL_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDL, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFL_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDL, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForFFL_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDL, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForFFL_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDL, TopoOritationForFFL_TBX),
                Is.EqualTo(DeviceTopoOritationForFF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDL, TopoOritationForFFL_TBY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDL, TopoOritationForFFL_LRX),
                Is.EqualTo(DeviceTopoOritationForFF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDL, TopoOritationForFFL_LRY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDL, AxialOritationForFFL_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDL, AxialOritationForFFL_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }

        [Test]
        public void TestImageOrientationOnFFR()
        {
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDR, DeviceTopoOritationForFF_X),
                Is.EqualTo(TopoOritationForFFR_TBX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDR, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFR_TBY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDR, DeviceTopoOritationForFF_Y),
                Is.EqualTo(TopoOritationForFFR_LRX));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDR, DeviceTopoOritationForFF_Z),
                Is.EqualTo(TopoOritationForFFR_LRY));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDR, DeviceAxialOritation_X),
                Is.EqualTo(AxialOritationForFFR_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorDeviceToPatient(PatientPosition.FFDR, DeviceAxialOritation_Y),
                Is.EqualTo(AxialOritationForFFR_Y));

            //Patient to Device
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDR, TopoOritationForFFR_TBX),
                Is.EqualTo(DeviceTopoOritationForFF_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDR, TopoOritationForFFR_TBY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDR, TopoOritationForFFR_LRX),
                Is.EqualTo(DeviceTopoOritationForFF_Y));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDR, TopoOritationForFFR_LRY),
                Is.EqualTo(DeviceTopoOritationForFF_Z));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDR, AxialOritationForFFR_X),
                Is.EqualTo(DeviceAxialOritation_X));
            Assert.That(CoordinateConverter.Instance.TransformVectorPatientToDevice(PatientPosition.FFDR, AxialOritationForFFR_Y),
                Is.EqualTo(DeviceAxialOritation_Y));
        }
    }
}