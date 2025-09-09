//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/3/19 17:26:30    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NSubstitute.Routing.Handlers;
using NUnit.Framework;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Axial;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.DualTopo;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.ScanReconCalculation.UnitTest.Scan.Gantry.DualTopo
{
    public class DualTopoGantryCalculatorTests
    {
        private DualTopoGantryCalculator testee = new DualTopoGantryCalculator();

        [TestCase(26500, new TubePosition[] { TubePosition.Angle0, TubePosition.Angle90 }, 12, 200,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 },
            20, 14, 27000)]
        [TestCase(26500, new TubePosition[] { TubePosition.Angle0, TubePosition.Angle90 }, 12, 200,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 400, 150, 150, 150 },
            21, 15, 28500)]
        [TestCase(26500, new TubePosition[] { TubePosition.Angle0, TubePosition.Angle90 }, 12, 200,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 400, 400, 400, 400, 150, 150, 150, 150, 150, 150, 150 },
            18, 12, 24000)]
        [TestCase(7500, new TubePosition[] { TubePosition.Angle0, TubePosition.Angle270 }, 12, 200,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 },
            7, 13, 7500)]
        [TestCase(6200, new TubePosition[] { TubePosition.Angle0, TubePosition.Angle270 }, 12, 200,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 },
            6, 12, 6000)]
        [TestCase(24600, new TubePosition[] { TubePosition.Angle0, TubePosition.Angle270 }, 12, 200,
            new double[] { 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150, 150 },
            18, 0, 24000)]

        public void TestGetPreferDualTubeNumAndNewPosition(double currentGantryPos, TubePosition[] tubePositions, int preferTubeCount, double preferTubeCap,
            double[] heatCaps,int expTube0,int expTube1,double expGantryPos)
        {
            var gantryInput = new GantryControlInput();
            gantryInput.CurrentGantryPos = currentGantryPos;
            gantryInput.TubePositions = tubePositions;
            gantryInput.HeatCaps = heatCaps;
            var (tube0,tube1,gantry) = testee.GetPreferDualTubeNumAndNewPosition(gantryInput,preferTubeCount,preferTubeCap);

            Assert.That(tube0,Is.EqualTo(expTube0));
            Assert.That(tube1, Is.EqualTo(expTube1));
            Assert.That(gantry, Is.EqualTo(expGantryPos).Within(CommonDefinition.DoubleTollerance));
        }

    }
}
