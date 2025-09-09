using NUnit.Framework;
using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomUtility.UnitTest.Graphic
{
    public class TopoDirectionSpecTest
    {
        [TestCase(PatientPosition.HFS,TubePosition.Angle0,new double[] { 1,0,0,0,0,-1})]
        [TestCase(PatientPosition.HFS, TubePosition.Angle270, new double[] { 0,1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.HFP, TubePosition.Angle0, new double[] { -1, 0, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.HFP, TubePosition.Angle270, new double[] { 0, 1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFS, TubePosition.Angle0, new double[] { -1, 0, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFS, TubePosition.Angle270, new double[] { 0, 1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFP, TubePosition.Angle0, new double[] { 1, 0, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFP, TubePosition.Angle270, new double[] { 0, 1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.HFDL, TubePosition.Angle0, new double[] { 0, -1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.HFDL, TubePosition.Angle270, new double[] { 1, 0, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.HFDR, TubePosition.Angle0, new double[] { 0, 1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.HFDR, TubePosition.Angle270, new double[] { 1, 0, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFDL, TubePosition.Angle0, new double[] { 0, -1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFDL, TubePosition.Angle270, new double[] { 1, 0, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFDR, TubePosition.Angle0, new double[] { 0, 1, 0, 0, 0, -1 })]
        [TestCase(PatientPosition.FFDR, TubePosition.Angle270, new double[] { 1, 0, 0, 0, 0, -1 })]

        public void TestGetTopoDirection(PatientPosition pp, TubePosition tb, double[] expResult)
        {
            var result = TopoDirectionSpecification.GetTopoDirection(pp,tb);
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}
