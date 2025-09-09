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
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Axial;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.ScanReconCalculation.UnitTest.Scan.Gantry.Axial
{
    public class AxialGantryCalculatorTests
    {
        private AxialGantryCalculator testee = new AxialGantryCalculator();

        [TestCase(ScanOption.Surview, false)]
        [TestCase(ScanOption.DualScout, false)]
        [TestCase(ScanOption.Axial, true)]
        [TestCase(ScanOption.Helical, false)]
        public void TestCanAccept(ScanOption scanOption, bool expectedResult)
        {
            var input = new GantryControlInput();
            input.ScanOption = scanOption;

            Assert.That(testee.CanAccept(input), Is.EqualTo(expectedResult));
        }

        [TestCase(5000, 360, 24, 1, 2700, 833.3, 128.6, 308642.0)]
        [TestCase(5000, 360, 24, 3, 2700, 2500, 1157.4, 925925.9)]
        [TestCase(5000, 1080, 24, 1, 2700, 277.8, 14.3, 102880.7)]
        public void TestGetAxialGantrySpeed(double frameTime, int framesPerCycle,int totalSouceCount, int expSouceCount,double acc,double expSpeed, double expAngle, double expTime)
        {
            var input = new GantryControlInput();
            input.FrameTime = frameTime;
            input.FramesPerCycle = framesPerCycle;
            input.TotalSourceCount = totalSouceCount;
            input.ExpSourceCount = expSouceCount;
            input.GantryAcc = acc;

            var speed = testee.GetAxialGantrySpeed(input);
            var angle = testee.GetGantryAccAngle(input);
            var time = testee.GetGantryAccTime(input);

            speed = Math.Round(speed, CommonDefinition.TableRoundDigitLength);
            angle = Math.Round(angle, CommonDefinition.TableRoundDigitLength);
            time = Math.Round(time, CommonDefinition.TableRoundDigitLength);
            Assert.That(speed, Is.EqualTo(expSpeed).Within(CommonDefinition.DoubleTollerance));
            Assert.That(angle, Is.EqualTo(expAngle).Within(CommonDefinition.DoubleTollerance));
            Assert.That(time, Is.EqualTo(expTime).Within(CommonDefinition.DoubleTollerance));
        }

        [TestCase(5000, 360, 48, 1, 24, 4, 8.2)]
        [TestCase(5000, 1080, 48, 1, 24, 5, 28.2)]
        public void TestGetTotalScanTime(
            double frameTime, int framesPerCycle, int ignoredN, 
            int expSouceCount, int totalSouceCount, int numOfScan,double expResult)
        {
            var input = new GantryControlInput();
            input.FrameTime = frameTime;
            input.FramesPerCycle = framesPerCycle;
            input.PreIgnoredN = ignoredN;
            input.TotalSourceCount = totalSouceCount;
            input.ExpSourceCount = expSouceCount;
            input.NumOfScan = numOfScan;

            var result = testee.GetTotalScanTime(input);
            result = Math.Round(result, CommonDefinition.TableRoundDigitLength);
            Assert.That(result, Is.EqualTo(expResult).Within(CommonDefinition.DoubleTollerance));
        }

        [TestCase(5000, 360, 48, 24, 1, 2700, 4, 422.4, 2000, 9355.0)]
        [TestCase(5000, 360, 48, 24, 1, 2700, 4, 400, 2000, 9293.3)]
        [TestCase(5000, 1080, 48, 24, 1, 2700, 5, 400, 2000, 8855.7)]
        public void TestGetTotalGantryAngle(
            double frameTime, int framesPerCycle, int ignoredN, int totalSouceCount, int expSouceCount, 
            double gantryAcc, int numOfScan,double tableFeed,double tableAcc, double expAngle,double moveToWeight = 0,double minWeight = 0)
        {
            GantryCommonConfig.MinGantryMoveTolerance = moveToWeight;
            GantryCommonConfig.GantryMoveToleranceWeight = minWeight;

            var input = new GantryControlInput();
            input.FrameTime = frameTime;
            input.FramesPerCycle = framesPerCycle;
            input.PreIgnoredN = ignoredN;
            input.TotalSourceCount = totalSouceCount;
            input.ExpSourceCount = expSouceCount;
            input.GantryAcc = gantryAcc;
            input.NumOfScan = numOfScan;
            input.TableFeed = tableFeed;
            input.TableAcc = tableAcc;

            var result = testee.GetTotalGantryAngle(input);
            result = Math.Round(result, CommonDefinition.TableRoundDigitLength);
            Assert.That(result, Is.EqualTo(expAngle).Within(CommonDefinition.DoubleTollerance));
        }
    }
}
