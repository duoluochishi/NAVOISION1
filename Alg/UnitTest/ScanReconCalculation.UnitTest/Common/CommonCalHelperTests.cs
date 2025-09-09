//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/02/02 09:42:22    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NUnit.Framework;
using NUnit.Framework.Internal;
using NV.CT.Alg.ScanReconCalculation.Scan.Common;

namespace NV.CT.ScanReconCalculation.UnitTest.Common
{
    public class CommonCalHelperTests
    {

        [TestCase(20,10,20)]
        [TestCase(200, 200, 100)]
        [TestCase(22.22, 200, 1.234)]
        public void TestGetAccelerateLength(double speed, double acc,double expLength)
        {
            var result = CommonCalHelper.GetAccelerateLength(speed, acc);
            result = Math.Round(result,CommonDefinition.TableRoundDigitLength);
            Assert.That(result, Is.LessThan(expLength).Within(CommonDefinition.DoubleTollerance));
        }

        [TestCase(20, 10, 40)]
        [TestCase(200, 200, 100)]
        public void TestGetAccelerateTime(double speed, double acc, double expTime)
        {
            var result = CommonCalHelper.GetAccelerateTime(speed, acc);
            Assert.That(result, Is.LessThan(expTime).Within(CommonDefinition.DoubleTollerance));
        }

        [TestCase(0, 0)]
        [TestCase(20, 0)]
        public void TestGetAccelerateLengthWithException(double speed,double acc)
        {
            Assert.Throws<InvalidOperationException>(
                () => {
                    CommonCalHelper.GetAccelerateLength(speed, acc);
                });
        }


        [TestCase(0, 0)]
        [TestCase(20, 0)]
        public void TestGetAccelerateTimeWithException(double speed, double acc)
        {
            Assert.Throws<InvalidOperationException>(
                () => {
                    CommonCalHelper.GetAccelerateTime(speed, acc);
                    });
        }

        [TestCase(0.005,1080,3,1.8)]
        [TestCase(0.006, 540, 1, 3.24)]
        [TestCase(6, 540, 1, 3240)]
        public void TestGetCycleTime(double frameTime, int framesPerCycle, int expSourceCount, double expTime)
        {
            var result = CommonCalHelper.GetCycleTime(frameTime, framesPerCycle, expSourceCount);
            Assert.That(result, Is.EqualTo(expTime).Within(CommonDefinition.DoubleTollerance));
        }



        [TestCase(10.0, 2.0, true)]
        [TestCase(10.0000001, 2.0, true)]
        [TestCase(10.0, 2.000001, true)]
        [TestCase(10.0, 2.1, false)]
        [TestCase(10.0, 1.999999, true)]
        [TestCase(422, 422.4, true)]
        public void TestIsDoubleMode(double a, double b, bool expResult)
        {
            var result = CommonCalHelper.IsDoubleMod(a, b);
            Assert.That(result, Is.EqualTo(expResult));
        }

        [TestCase(new double[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 0, 1, 2, 3, 4, 5, 6 })]
        [TestCase(new double[] { 1, 4, 8, 3, 7, 2, 5, 1 }, new int[] { 0, 7, 5, 3, 1, 6, 4, 2 })]
        public void TestSortTubeByCapacity(double[] capacities, int[] expResult)
        {

            var result = CommonCalHelper.GetSortedIndex(capacities);
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}
