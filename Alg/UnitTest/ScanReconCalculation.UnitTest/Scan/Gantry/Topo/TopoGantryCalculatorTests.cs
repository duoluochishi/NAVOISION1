//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:41:39    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NUnit.Framework;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Topo;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.ScanReconCalculation.UnitTest.Scan.Gantry.Topo
{
    public class TopoGantryCalculatorTests
    {
        private TopoGantryCalculator testee = new TopoGantryCalculator();


        [TestCase(0, TubePosition.Angle90, 6000)]
        [TestCase(9, TubePosition.Angle90, 19500)]
        [TestCase(23, TubePosition.Angle90, 40500)]
        [TestCase(0, TubePosition.Angle270, 24000)]
        [TestCase(9, TubePosition.Angle270, 37500)]
        [TestCase(23, TubePosition.Angle270, 22500)]
        [TestCase(0, TubePosition.Angle0, 33000)]
        [TestCase(9, TubePosition.Angle0, 10500)]
        [TestCase(23, TubePosition.Angle0, 31500)]
        [TestCase(0, TubePosition.Angle180, 15000)]
        [TestCase(9, TubePosition.Angle180, 28500)]
        [TestCase(23, TubePosition.Angle180, 13500)]
        public void TestGetGantryPosForTubeAtTubePosition(int tubeNum,TubePosition tubePosition,double expGantryPos)
        {
            var result = testee.GetGantryPosForTubeAtTubePosition(tubeNum, tubePosition);
            Assert.That(result,Is.EqualTo(expGantryPos).Within(CommonDefinition.DoubleTollerance));
        }

        [TestCase(6000, TubePosition.Angle0, 6)]
        [TestCase(6000, TubePosition.Angle90, 0)]
        [TestCase(6000, TubePosition.Angle180, 18)]
        [TestCase(6000, TubePosition.Angle270, 12)]
        [TestCase(10600, TubePosition.Angle0, 9)]
        [TestCase(10600, TubePosition.Angle90, 3)]
        [TestCase(10600, TubePosition.Angle180, 21)]
        [TestCase(10600, TubePosition.Angle270, 15)]
        [TestCase(29400, TubePosition.Angle0, 22)]
        [TestCase(29400, TubePosition.Angle90, 16)]
        [TestCase(29400, TubePosition.Angle180, 10)]
        [TestCase(29400, TubePosition.Angle270, 4)]
        [TestCase(54000, TubePosition.Angle0, 13)]
        [TestCase(54000, TubePosition.Angle90, 7)]
        [TestCase(54000, TubePosition.Angle180, 1)]
        [TestCase(54000, TubePosition.Angle270, 19)]
        public void TestSelectNearestTube(double gantryPos, TubePosition tubePosition, int expTubeNum)
        {
            var result = testee.GetNearestTubeNumAndNewPosition(gantryPos, tubePosition);
            Assert.That(result.tubeNum, Is.EqualTo(expTubeNum));
        }


        [TestCase(6000, TubePosition.Angle0, 3, 100,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 }, -1, double.NaN)]
        [TestCase(6000, TubePosition.Angle0, 3, 100,
            new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 6, 6000)]
        [TestCase(6000, TubePosition.Angle0, 3, 100,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 44, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 }, 8, 9000)]
        [TestCase(42000, TubePosition.Angle0, 3, 100,
            new double[] { 150, 150, 99, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 }, -1, double.NaN)]
        [TestCase(42000, TubePosition.Angle0, 5, 100,
            new double[] { 150, 150, 99, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 }, 2, 36000)]
        public void TestGetPreferTubeNumAndNewPosition(double gantryPos, TubePosition tubePosition, int preferTubeCount, double preferHeatCapacity, double[] heatCaps, int expTubeNum,double expGantryPos)
        {
            GantryControlInput input = new GantryControlInput();
            input.CurrentGantryPos = gantryPos;
            input.TubePositions[0] = tubePosition;
            input.HeatCaps = heatCaps;

            var (newTubeNum,newGantryPos) = testee.GetPreferTubeNumAndNewPosition(input, preferTubeCount, preferHeatCapacity);

            Assert.That(newTubeNum, Is.EqualTo(expTubeNum));
            Assert.That(newGantryPos, Is.EqualTo(expGantryPos).Within(CommonDefinition.DoubleTollerance));

        }

    }
}
