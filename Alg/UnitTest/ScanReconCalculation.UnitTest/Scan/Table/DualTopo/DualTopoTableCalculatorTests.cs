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
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Topo;
using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.DualTopo;

namespace NV.CT.ScanReconCalculation.UnitTest.Scan.Table.DualTopo
{
    public class DualTopoTableCalculatorTests
    {

        //    private DualTopoTableCalculator testee = new DualTopoTableCalculator();


        //    [TestCase(ScanOption.Surview, false)]
        //    [TestCase(ScanOption.DualScout, true)]
        //    [TestCase(ScanOption.Axial, false)]
        //    [TestCase(ScanOption.Helical, false)]
        //    public void TestCanAccept(ScanOption scanOption, bool expectedResult)
        //    {
        //        Assert.That(testee.CanAccept(new TableControlInput() { ScanOption = scanOption }), Is.EqualTo(expectedResult));
        //    }


        //    [TestCase(TableDirection.In, -10000, -13422.4, 150000, 1, 422.4, 300, 2000, 5000,
        //        -9690, -13300, -8690, -14300, 2000, 1000000)]
        //    [TestCase(TableDirection.Out, -13422.4, -10000, 150000, 1, 422.4, 300, 2000, 5000,
        //         -13310, -9700, -14310, -8700, 2000, 1000000)]
        //    [TestCase(TableDirection.In, -10000, -13400.4, 150000, 1, 422.4, 300, 2000, 5000,
        //        -9690, -13000, -8690, -14000, 2000, 1000000)]
        //    [TestCase(TableDirection.Out, -13422.4, -10020, 150000, 1, 422.4, 300, 2000, 5000,
        //         -13310, -10000, -14310, -9000, 2000, 1000000)]
        //    [Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //    public void TestCalculateTableControlInfo(
        //        TableDirection direction,
        //        double volBeginPos, double volEndPos,
        //        double frameTime,
        //        int ignoreN,
        //        double collimatedSliceWidth,
        //        double tableFeed, double tableAcc,
        //        double expTime,
        //        double expDataBeginPos, double expDataEndPos, double expTableBeginPos, double expTableEndPos, double expTableSpeed, double accTime,
        //        double topoTolWeight = 0, double minTopoTol = 0)
        //    {
        //        TopoTableCalculator.TopoTableMoveTolleranceWeight = topoTolWeight;
        //        TopoTableCalculator.MinTopoTableMoveTollerance = minTopoTol;

        //        TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, volBeginPos, volEndPos,
        //            frameTime, 1080, ignoreN, collimatedSliceWidth, tableFeed, tableAcc, expTime);

        //        var result = testee.CalculateTableControlInfo(input);

        //        Assert.That(result.ReconVolumeBeginPos, Is.EqualTo(volBeginPos).Within(CommonDefinition.DoubleTollerance));
        //        Assert.That(result.ReconVolumeEndPos, Is.EqualTo(volEndPos).Within(CommonDefinition.DoubleTollerance));
        //        Assert.That(result.DataBeginPos, Is.EqualTo(expDataBeginPos).Within(CommonDefinition.DoubleTollerance));
        //        Assert.That(result.DataEndPos, Is.EqualTo(expDataEndPos).Within(CommonDefinition.DoubleTollerance));
        //        Assert.That(result.TableBeginPos, Is.EqualTo(expTableBeginPos).Within(CommonDefinition.DoubleTollerance));
        //        Assert.That(result.TableEndPos, Is.EqualTo(expTableEndPos).Within(CommonDefinition.DoubleTollerance));
        //        Assert.That(result.TableSpeed, Is.EqualTo(expTableSpeed).Within(CommonDefinition.DoubleTollerance));

        //        Assert.That(result.TableAccTime, Is.EqualTo(accTime).Within(CommonDefinition.DoubleTollerance));
        //    }
    }

}
