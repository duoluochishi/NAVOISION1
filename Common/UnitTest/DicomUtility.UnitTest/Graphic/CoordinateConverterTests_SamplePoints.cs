using NUnit.Framework;
using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.UnitTest.Graphic
{
    public class CoordinateConverterTests_SamplePoints
    {
        [TestCase(0, 0,0,0,0,0)]                                                //原点
        [TestCase(0,0,-2000,0,0,-2000)]                                         //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, -168.96, -168.96, -2000)]            //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, 168.96, 168.96, -2000)]                //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0 , -2000, -168.96, 0, -2000)]                       //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, 168.96, 0, -2168.96)]                    //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0,168.96,-2000,0,168.96,-2000)]                               //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96, 0, -168.96, -2168.96)]                  //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, 100, 200, 300)]                                //一般位置
        public void TestPointTransformOnHFS(double x,double y,double z,double expectedX, double expectedY,double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX,expectedY,expectedZ), 
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.HFS,new Point3D(x, y, z))));

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.HFS, new Point3D(expectedX, expectedY, expectedZ))));
        }


        [TestCase(0, 0, 0, 0, 0, 0)]                                                    //原点
        [TestCase(0, 0, -2000, 0, 0, -2000)]                                            //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, 168.96, 168.96, -2000)]                      //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, -168.96, -168.96, -2000)]                      //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0, -2000, 168.96, 0, -2000)]                                 //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, -168.96, 0, -2168.96)]                           //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0, 168.96, -2000, 0, -168.96, -2000)]                                 //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96, 0, 168.96, -2168.96)]                           //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, -100, -200, 300)]                                      //一般位置
        public void TestPointTransformOnHFP(double x, double y, double z, double expectedX, double expectedY, double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX, expectedY, expectedZ),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.HFP, new Point3D(x, y, z))));

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.HFP, new Point3D(expectedX, expectedY, expectedZ))));
        }

        [TestCase(0, 0, 0, 0, 0, 0)]                                                    //原点
        [TestCase(0, 0, -2000, 0, 0, -2000)]                                            //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, -168.96, 168.96, -2000)]                     //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, 168.96, -168.96, -2000)]                       //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0, -2000, 0, 168.96, -2000)]                                 //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, 0, -168.96, -2168.96)]                           //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0, 168.96, -2000, 168.96,0 , -2000)]                                  //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96, -168.96, 0, -2168.96)]                          //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, 200, -100, 300)]                                      //一般位置
        public void TestPointTransformOnHFL(double x, double y, double z, double expectedX, double expectedY, double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX, expectedY, expectedZ),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.HFDL, new Point3D(x, y, z))));

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.HFDL, new Point3D(expectedX, expectedY, expectedZ))));

        }


        [TestCase(0, 0, 0, 0, 0, 0)]                                                    //原点
        [TestCase(0, 0, -2000, 0, 0, -2000)]                                            //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, 168.96, -168.96, -2000)]                     //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, -168.96, 168.96, -2000)]                       //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0, -2000, 0, -168.96, -2000)]                                //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, 0, 168.96, -2168.96)]                            //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0, 168.96, -2000, -168.96, 0, -2000)]                                 //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96, 168.96, 0, -2168.96)]                           //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, -200, 100, 300)]                                       //一般位置
        public void TestPointTransformOnHFR(double x, double y, double z, double expectedX, double expectedY, double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX, expectedY, expectedZ),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.HFDR, new Point3D(x, y, z))));

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.HFDR, new Point3D(expectedX, expectedY, expectedZ))));

        }


        [TestCase(0, 0, 0, 0, 0, 0)]                                                    //原点
        [TestCase(0, 0, -2000, 0, 0, 2000)]                                            //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, 168.96, -168.96, 2000)]                     //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, -168.96, 168.96, 2000)]                       //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0, -2000, 168.96, 0, 2000)]                                 //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, -168.96, 0, 2168.96)]                           //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0, 168.96, -2000,  0, 168.96, 2000)]                                  //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96,  0, -168.96, 2168.96)]                          //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, -100, 200, -300)]                                      //一般位置
        public void TestPointTransformOnFFS(double x, double y, double z, double expectedX, double expectedY, double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX, expectedY, expectedZ),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.FFS, new Point3D(x, y, z))));

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.FFS, new Point3D(expectedX, expectedY, expectedZ))));

        }


        [TestCase(0, 0, 0, 0, 0, 0)]                                                    //原点
        [TestCase(0, 0, -2000, 0, 0, 2000)]                                            //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, -168.96, 168.96, 2000)]                     //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, 168.96, -168.96, 2000)]                       //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0, -2000, -168.96, 0, 2000)]                                 //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, 168.96, 0, 2168.96)]                           //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0, 168.96, -2000, 0, -168.96, 2000)]                                  //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96, 0, 168.96, 2168.96)]                          //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, 100, -200, -300)]                                      //一般位置
        public void TestPointTransformOnFFP(double x, double y, double z, double expectedX, double expectedY, double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX, expectedY, expectedZ),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.FFP, new Point3D(x, y, z))));

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.FFP, new Point3D(expectedX, expectedY, expectedZ))));

        }

        [TestCase(0, 0, 0, 0, 0, 0)]                                                    //原点
        [TestCase(0, 0, -2000, 0, 0, 2000)]                                            //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, -168.96, -168.96, 2000)]                     //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, 168.96, 168.96, 2000)]                       //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0, -2000, 0, -168.96, 2000)]                                 //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, 0, 168.96, 2168.96)]                           //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0, 168.96, -2000, 168.96, 0, 2000)]                                  //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96, -168.96, 0, 2168.96)]                          //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, 200, 100, -300)]                                      //一般位置
        public void TestPointTransformOnFFL(double x, double y, double z, double expectedX, double expectedY, double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX, expectedY, expectedZ),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.FFDL, new Point3D(x, y, z)))); 

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.FFDL, new Point3D(expectedX, expectedY, expectedZ))));

        }
        [TestCase(0, 0, 0, 0, 0, 0)]                                                    //原点
        [TestCase(0, 0, -2000, 0, 0, 2000)]                                            //-2000床位下孔径中心
        [TestCase(-168.96, -168.96, -2000, 168.96, 168.96, 2000)]                     //0.165*2048矩阵轴扫图像，图像左上角点
        [TestCase(168.96, 168.96, -2000, -168.96, -168.96, 2000)]                       //0.165*2048矩阵轴扫图像，图像右下角点
        [TestCase(-168.96, 0, -2000, 0, 168.96, 2000)]                                 //0.165*2048矩阵正位定位像，图像左上角点
        [TestCase(168.96, 0, -2168.96, 0, -168.96, 2168.96)]                           //0.165*2048矩阵正位定位像，图像右下角点
        [TestCase(0, 168.96, -2000, -168.96, 0, 2000)]                                  //0.165*2048矩阵侧位定位像，图像左上角点
        [TestCase(0, -168.96, -2168.96, 168.96, 0, 2168.96)]                          //0.165*2048矩阵侧位定位像，图像右下角点
        [TestCase(100, 200, 300, -200, -100, -300)]                                      //一般位置
        public void TestPointTransformOnFFR(double x, double y, double z, double expectedX, double expectedY, double expectedZ)
        {
            //Device To Patient
            Assert.That(new Point3D(expectedX, expectedY, expectedZ),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointDeviceToPatient(PatientPosition.FFDR, new Point3D(x, y, z))));

            //Patient To Device
            Assert.That(new Point3D(x, y, z),
                Is.EqualTo(CoordinateConverter.Instance.TransformPointPatientToDevice(PatientPosition.FFDR, new Point3D(expectedX, expectedY, expectedZ))));
        }
    }
}
