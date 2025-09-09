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
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Axial;
using NV.CT.Alg.ScanReconCalculation.Scan.Table;

namespace NV.CT.ScanReconCalculation.UnitTest.Scan.Table.Axial
{
    public class AxialTableCalculatorTests
    {
        //private readonly AxialTableCalculator testee = new AxialTableCalculator();


        //[TestCase(ScanOption.Surview, false)]
        //[TestCase(ScanOption.DualScout, false)]
        //[TestCase(ScanOption.Axial, true)]
        //[TestCase(ScanOption.Helical, false)]
        //public void TestCanAccept(ScanOption scanOption, bool expectedResult)
        //{
        //    Assert.That(testee.CanAccept(new TableControlInput() { ScanOption = scanOption}), Is.EqualTo(expectedResult));
        //}

        //[TestCase(TableDirection.In,1,300,422.4, 0 )]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, 422.4 )]
        //[TestCase(TableDirection.In, 1, 300, 422.4, -211.2)]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, 211.2)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestGetPreOffsetD2V(TableDirection direction, int ignoreN, double tableFeed, double collimatedSliceWidth, double expOffset)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, 0, 0, 0, 0, ignoreN, collimatedSliceWidth, tableFeed, 0, 0);

        //    var result = testee.GetPreOffsetD2V(input);
        //    Assert.That(result, Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //}


        //[TestCase(TableDirection.In, 1, 300, 422.4,422.4)]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, 0)]
        //[TestCase(TableDirection.In, 1, 300, 422.4, 211.2)]
        //[TestCase(TableDirection.Out, 1, 300, 422.4, -211.2)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestGetPostOffsetD2V(TableDirection direction, int ignoreN, double tableFeed, double collimatedSliceWidth, double expOffset)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, 0, 0, 0, 0, ignoreN, collimatedSliceWidth, tableFeed, 0, 0);

        //    var result = testee.GetPostOffsetD2V(input);
        //    Assert.That(result, Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In, 300, 150000, 2000, 0)]
        //[TestCase(TableDirection.Out, 300, 150000, 2000, 0)]
        //public void TestGetPrePostOffsetT2D(TableDirection direction, double tableFeed, double frameTime, double acc, double expOffset)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, 0, 0, frameTime, 1080, 1, 0,tableFeed, acc, 0);

        //    Assert.That(testee.GetPostOffsetT2D(input), Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(testee.GetPreOffsetT2D(input), Is.EqualTo(expOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In, -1000000, -12422.4, 150000, 1, 42240, 20000, 20000,
        //    -10211.2, -12211.2, -10211.2, -12211.2)]
        ////[TestCase(TableDirection.In, -10000, -12422.4, 150000, 1, 422.4, 400, 2000,
        ////     -10000, -12000, -10000, -12000)]

        ////[TestCase(TableDirection.Out, -12422.4, -10000, 150000, 1, 422.4, 400, 2000,
        ////     -12211.2, -10211.2, -12211.2, -10211.2)]
        ////[TestCase(TableDirection.Out, -12422.4, -10000, 150000, 1, 422.4, 400, 2000,
        ////     -12000, -10000, -12000, -10000)]
        ////[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestCalculateTableControlInfo(
        //    TableDirection direction,
        //    double volBeginPos, double volEndPos,
        //    double frameTime,
        //    int ignoreN,
        //    double collimatedSliceWidth,
        //    double tableFeed, double tableAcc,
        //    double expDataBeginPos, double expDataEndPos, double expTableBeginPos, double expTableEndPos)
        //{

        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, volBeginPos, volEndPos,
        //        frameTime, 1080, ignoreN, collimatedSliceWidth, tableFeed, tableAcc, 0);

        //    var result = testee.CalculateTableControlInfo(input);

        //    Assert.That(result.ReconVolumeBeginPos, Is.EqualTo(volBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.ReconVolumeEndPos, Is.EqualTo(volEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.DataBeginPos, Is.EqualTo(expDataBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.DataEndPos, Is.EqualTo(expDataEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.TableBeginPos, Is.EqualTo(expTableBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.TableEndPos, Is.EqualTo(expTableEndPos).Within(CommonDefinition.DoubleTollerance));

        //    Assert.That(result.TableSpeed, Is.EqualTo(0).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.TableAccTime, Is.EqualTo(0).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.Out, -12400.4, -10000, 150000, 1, 422.4, 400, 2000,
        //     -12000, -10000, -12000, -10000)]
        //[TestCase(TableDirection.In, -10000, -12400.4, 150000, 1, 422.4, 400, 2000,
        //     -12000, -10000, -12000, -10000)]
        //[TestCase(TableDirection.Out, -12400.4, -10000, 150000, 1, 422.4, 400, 2000,
        //     -12000, -10000, -12000, -10000)]
        //[TestCase(TableDirection.In, -10000, -12400.4, 150000, 1, 422.4, 400, 2000,
        //     -12000, -10000, -12000, -10000)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestCalculateTableControlInfoException(
        //    TableDirection direction,
        //    double volBeginPos, double volEndPos,
        //    double frameTime,
        //    int ignoreN,
        //    double collimatedSliceWidth,
        //    double tableFeed, double tableAcc,
        //    double expDataBeginPos, double expDataEndPos, double expTableBeginPos, double expTableEndPos)
        //{
        //    TableControlInput input = new TableControlInput(ScanOption.Surview, ScanMode.None, direction, volBeginPos, volEndPos,
        //        frameTime, 1080, ignoreN, collimatedSliceWidth, tableFeed, tableAcc, 0);

        //    Assert.Throws<InvalidOperationException>(
        //        () => {
        //            testee.CalculateTableControlInfo(input);
        //        });
        //}

    }
}
