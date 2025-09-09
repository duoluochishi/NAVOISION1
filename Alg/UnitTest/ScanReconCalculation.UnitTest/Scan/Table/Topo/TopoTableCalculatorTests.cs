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
using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Topo;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.ScanReconCalculation.UnitTest.Scan.Table.Topo
{
    public class TopoTableCalculatorTests
    {

        //private readonly TopoTableCalculator testee = new TopoTableCalculator();


        //[TestCase(ScanOption.Surview, true)]
        //[TestCase(ScanOption.DualScout, false)]
        //[TestCase(ScanOption.Axial, false)]
        //[TestCase(ScanOption.Helical, false)]
        
        //public void TestCanAccept(ScanOption scanOption, bool expectedResult)
        //{
        //    Assert.That(testee.CanAccept(new TableControlInput() { ScanOption = scanOption }), Is.EqualTo(expectedResult));
        //}

        //[TestCase(47.52, 0, 0)]
        //[TestCase(44.24, 0, 0)]
        //[TestCase(44.24, -0.5, -22.12)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestNearSideOffsetD2V(double collimatedSliceWidth, double nearSideOffsetWeight, double expectedOffset)
        //{
        //    Assert.That(testee.GetLargeSideOffsetD2V(null), Is.EqualTo(expectedOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(47.52, 1, 47.52)]
        //[TestCase(44.24, 1, 44.24)]
        //[TestCase(44.24, 0.5, 22.12)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestFarSideOffsetD2V(double collimatedSliceWidth, double farSideOffsetWeight,double expectedOffset)
        //{
        //    Assert.That(testee.GetSmallSideOffsetD2V(null),  Is.EqualTo(expectedOffset).Within(CommonDefinition.DoubleTollerance));
        //}



        //[TestCase(0, 10000, 0)]
        //[TestCase(1, 30, 30)]
        //[TestCase(2, 30, 60)]
        //public void TestIgnoredNLength(int ignoredN, double tableFeed, double expectedResult)
        //{
        //    Assert.That(testee.GetIgnoredND2V(ignoredN, tableFeed), Is.EqualTo(expectedResult).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In, 1, 300, 422.4, 300)]
        //[TestCase(TableDirection.In, 2, 300, 422.4, 600)]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, 122.4)]
        //[TestCase(TableDirection.In, 1, 300, 422.4, 88.8)]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, -88.8)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestGetPreOffsetD2V(TableDirection direction,int ignoreN,double tableFeed,double collimatedSliceWidth, double expOffset)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview,ScanMode.None,direction,0,0,0,0,ignoreN,collimatedSliceWidth, tableFeed,0,0);

            
        //    var result = testee.GetPreOffsetD2V(input);
        //    Assert.That(result , Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In, 1, 300, 422.4, 422.4)]
        //[TestCase(TableDirection.In, 2, 300, 422.4, 422.4)]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, 0)]
        //[TestCase(TableDirection.In, 1, 300, 422.4, 211.2)]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, -211.2)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestGetPostOffsetD2V(TableDirection direction, int ignoreN, double tableFeed, double collimatedSliceWidth, double expOffset)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, 0, 0, 0, 0, ignoreN, collimatedSliceWidth, tableFeed,0, 0);


        //    var result = testee.GetPostOffsetD2V(input);
        //    Assert.That(result, Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In, 300,150000,2000,1000)]
        //[TestCase(TableDirection.Out, 300, 150000, 2000, -1000)]
        ////[TestCase(TableDirection.In, 300, 150000, 2000, 1010,10)]
        ////[TestCase(TableDirection.Out, 300, 150000, 2000, -1010, 10)]
        //public void TestGetPreOffsetT2D(TableDirection direction, double tableFeed, double frameTime, double acc, double expOffset, double moveTollerance = 0)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction,0,0, frameTime, 1080,1, 0,tableFeed, acc, 0);

        //    TopoTableCalculator.TopoTableMoveTolleranceWeight = moveTollerance;
        //    var result = testee.GetPreOffsetT2D(input);
        //    Assert.That(result, Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In, 300, 150000, 2000, -1000)]
        //[TestCase(TableDirection.Out, 300, 150000, 2000, 1000)]
        ////[TestCase(TableDirection.In, 300, 150000, 2000, -1010, 10)]
        ////[TestCase(TableDirection.Out, 300, 150000, 2000, 1010, 10)]
        //public void TestGetPostOffsetT2D(TableDirection direction, double tableFeed, double frameTime, double acc, double expOffset, double moveTollerance = 0)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, 0, 0, frameTime, 1080, 1, 0,tableFeed, acc, 0);

        //    TopoTableCalculator.TopoTableMoveTolleranceWeight = moveTollerance;
        //    var result = testee.GetPostOffsetT2D(input);
        //    Assert.That(result, Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In,-10000,-13422.4,150000,1,422.4, 300,2000,
        //    -9911.2,-13211.2, -8911.2, -14211.2, 2000, 1000000)]
        //[TestCase(TableDirection.In, -10000, -13400.4, 150000, 1, 422.4, 300, 2000,
        //    -9911.2, -13211.2, -8911.2, -14211.2, 2000, 1000000)]
        //[TestCase(TableDirection.Out, -13422.4, - 10000, 150000, 1, 422.4, 300, 2000,
        //     -13511.2, -10211.2, -14511.2, -9211.2, 2000, 1000000)]
        //[TestCase(TableDirection.Out, -13422.4, -10020, 150000, 1, 422.4, 300, 2000,
        //     -13511.2, -10211.2, -14511.2, -9211.2, 2000, 1000000)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestCalculateTableControlInfo(
        //    TableDirection direction,
        //    double volBeginPos, double volEndPos,
        //    double frameTime, 
        //    int ignoreN,
        //    double collimatedSliceWidth,
        //    double tableFeed, double tableAcc,
        //    double expDataBeginPos, double expDataEndPos,double expTableBeginPos,double expTableEndPos,double expTableSpeed,double accTime,
        //    double topoTolWeight = 0, double minTopoTol = 0)
        //{
        //    TopoTableCalculator.TopoTableMoveTolleranceWeight = topoTolWeight;
        //    TopoTableCalculator.MinTopoTableMoveTollerance = minTopoTol;

        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, volBeginPos, volEndPos,
        //        frameTime, 1080, ignoreN, collimatedSliceWidth, tableFeed, tableAcc, 0);

        //    var result = testee.CalculateTableControlInfo(input);

        //    Assert.That(result.ReconVolumeBeginPos , Is.EqualTo(volBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.ReconVolumeEndPos, Is.EqualTo(volEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.DataBeginPos, Is.EqualTo(expDataBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.DataEndPos, Is.EqualTo(expDataEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.TableBeginPos, Is.EqualTo(expTableBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.TableEndPos, Is.EqualTo(expTableEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.TableSpeed, Is.EqualTo(expTableSpeed).Within(CommonDefinition.DoubleTollerance));

        //    Assert.That(result.TableAccTime, Is.EqualTo(accTime).Within(CommonDefinition.DoubleTollerance));
        //}
    }
}
