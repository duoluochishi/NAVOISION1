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
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Helic;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.ScanReconCalculation.UnitTest.Scan.Table.Helic
{
    public class HelicTableCalculatorTests
    {
        private readonly HelicTableCalculator testee = new HelicTableCalculator();


        [TestCase(48, 1080, 422.4, 1, 18.8)]
        [TestCase(48, 360, 422.4, 1, 56.3)]
        [TestCase(48, 360, 422.4, 0.5, 28.2)]
        public void TestGetIgnoredND2V(int preIgnoredFrames , int framesPerCycle, double collimatedSliceWidth,double pitch,double expResult)
        {
            var input = new TableControlInput();
            input.PreIgnoredFrames = preIgnoredFrames;
            input.FramesPerCycle = framesPerCycle;
            input.CollimatedSliceWidth = collimatedSliceWidth;
            input.Pitch = pitch;
            var result = testee.GetIgnoredND2V(input);

            result = Math.Round(result,1);

            Assert.That(result, Is.EqualTo(expResult).Within(CommonDefinition.DoubleTollerance)); 
        }

        [TestCase(5000, 360, 1, 422.4, 1, 234.7)]
        [TestCase(10000,1080,1,422.4,1,39.1)]
        [TestCase(5000, 480,  1, 422.4, 1.5, 264)]
        [TestCase(6000, 480, 3, 211.2, 0.5, 110)]
        public void TestGetHelicTableSpeed(double frameTime, int framesPerCycle, int expSourceCount,double collimatedSliceWidth,double pitch, double expTableSpeed)
        {
            var input = new TableControlInput();
            input.FrameTime = frameTime;
            input.FramesPerCycle = framesPerCycle;
            input.ExpSourceCount = expSourceCount;
            input.CollimatedSliceWidth = collimatedSliceWidth;
            input.Pitch = pitch;

            var result = testee.GetHelicTableSpeed(input);
            result = Math.Round(result,CommonDefinition.TableRoundDigitLength);
            Assert.That(result,Is.EqualTo(expTableSpeed).Within(CommonDefinition.DoubleTollerance));
        }


        //[TestCase(TableDirection.In,2000,5000,360,1,422.4,1,13.8,-13.8)]
        //[TestCase(TableDirection.Out, 2000, 5000, 360, 1, 422.4, 1, -13.8, 13.8)]
        //[TestCase(TableDirection.In, 2000, 6000, 1080, 1, 422.4, 1.5, 2.4, -2.4)]
        //[TestCase(TableDirection.Out, 2000, 6000, 1080, 1, 422.4, 1.5, -2.4, 2.4)]
        //[TestCase(TableDirection.In, 2000, 6000, 1080, 3, 422.4, 1.5, 21.5, -21.5)]
        //[TestCase(TableDirection.Out, 2000, 6000, 1080, 3, 422.4, 1.5, -21.5, 21.5)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestGetPrePostOffsetT2D(TableDirection direction, double tableAcc, double frameTime, int framesPerCycle, int expSourceCount, double collimatedSliceWidth, double pitch,double expPreOffset,double expPostOffset)
        //{
        //    var input = new TableControlInput(ScanOption.Helical,ScanMode.Plain,direction,0,0,frameTime,framesPerCycle,0,collimatedSliceWidth,0,tableAcc,0);
        //    input.Pitch = pitch;
        //    input.ExpSourceCount = expSourceCount;

        //    var preOffset = testee.GetPreOffsetT2D(input);
        //    preOffset = Math.Round(preOffset,CommonDefinition.TableRoundDigitLength);
        //    var postOffset = testee.GetPostOffsetT2D(input);
        //    postOffset = Math.Round(postOffset, CommonDefinition.TableRoundDigitLength);


        //    Assert.That(preOffset, Is.EqualTo(expPreOffset).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(postOffset, Is.EqualTo(expPostOffset).Within(CommonDefinition.DoubleTollerance));
        //}

        //[TestCase(TableDirection.In, 48, 360, 422.4, 1, 267.5, -211.2)]
        //[TestCase(TableDirection.Out, 48, 360, 422.4,1, -267.5, 211.2)]
        //[TestCase(TableDirection.In, 48, 1080, 422.4, 1, 230, -211.2)]
        //[TestCase(TableDirection.Out, 48, 1080, 422.4, 1, -230, 211.2)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]

        [TestCase(TableDirection.In, 0, 360, 422.4, 1, 237.6, -184.8)]
        [TestCase(TableDirection.Out, 0, 360, 422.4, 1, -184.8,237.6)]
        public void TestGetPrePostOffsetD2V(TableDirection direction,int ignoredN,int framesPerCycle,double collimatedSW, double pitch,double expPreOffset,double expPostOffset,double preWeight = 1,double postWeight = 1)
        {
            HelicTableCalculator.HelicPreOffsetWeightD2V = preWeight;
            HelicTableCalculator.HelicPostOffsetWeightD2V = postWeight;

            var input = new TableControlInput();
            input.TableDirection = direction;
            input.PreIgnoredFrames = ignoredN;
            input.FramesPerCycle = framesPerCycle;
            input.CollimatedSliceWidth = collimatedSW;
            input.Pitch = pitch;
            
            var pre = Math.Round(testee.GetPreOffsetD2V(input),CommonDefinition.TableRoundDigitLength);
            var post = Math.Round(testee.GetPostOffsetD2V(input), CommonDefinition.TableRoundDigitLength);

            Assert.That(pre, Is.EqualTo(expPreOffset).Within(CommonDefinition.DoubleTollerance));
            Assert.That(post, Is.EqualTo(expPostOffset).Within(CommonDefinition.DoubleTollerance));
        }

        //[TestCase(TableDirection.In,-10000,-12000,5000,360,1,48,422.4,2000,1,
        //    -9732.5,-12211.2,-9718.7,-12225,234.7,117350)]
        //[TestCase(TableDirection.Out, -12000, -10000, 5000, 360, 1, 48, 422.4, 2000, 1,
        //    -12267.5, -9788.8, -12281.3, -9775, 234.7, 117350)]
        //[Ignore("暂时忽略，根据更新静态CT数据与计划范围关系调整算法。")]
        //public void TestCalculateTableControlInfo(TableDirection direction,
        //    double volBeginPos, double volEndPos,
        //    double frameTime,int framesPerCycle, int expSourceCount,
        //    int ignoreN,
        //    double collimatedSliceWidth, 
        //    double tableAcc,double pitch,
        //    double expDataBeginPos, double expDataEndPos, double expTableBeginPos, double expTableEndPos, double expTableSpeed, double accTime,
        //    double nearWeight = -0.5, double farWeight = 0.5)
        //{
        //    var input = new TableControlInput(ScanOption.Helical,ScanMode.Plain,direction,volBeginPos,volEndPos,frameTime,framesPerCycle,ignoreN,collimatedSliceWidth,0,tableAcc,0);
        //    input.Pitch = pitch;
        //    input.ExpSourceCount = expSourceCount;

        //    var result = testee.CalculateTableControlInfo(input);

        //    Assert.That(result.ReconVolumeBeginPos, Is.EqualTo(volBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(result.ReconVolumeEndPos, Is.EqualTo(volEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(Math.Round(result.DataBeginPos, CommonDefinition.TableRoundDigitLength), Is.EqualTo(expDataBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(Math.Round(result.DataEndPos, CommonDefinition.TableRoundDigitLength), Is.EqualTo(expDataEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(Math.Round(result.TableBeginPos,CommonDefinition.TableRoundDigitLength), Is.EqualTo(expTableBeginPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(Math.Round(result.TableEndPos, CommonDefinition.TableRoundDigitLength), Is.EqualTo(expTableEndPos).Within(CommonDefinition.DoubleTollerance));
        //    Assert.That(Math.Round(result.TableSpeed, CommonDefinition.TableRoundDigitLength), Is.EqualTo(expTableSpeed).Within(CommonDefinition.DoubleTollerance));

        //}

        [Test]
        public void Test()
        {
            var input = new TableControlInput();
            input.ReconVolumeBeginPos = -1200000;
            input.ReconVolumeEndPos = -1000000;
            input.PreIgnoredFrames = 48;
            input.CollimatorZ = 242;
            input.CollimatedSliceWidth = 39930;
            input.Pitch = 1;
            input.ObjectFov = 300000;
            input.PreDeleteRatio = 1;
            input.FramesPerCycle = 540;
            input.TableAcc = 270000;
            input.TableDirection = TableDirection.Out;
            TableCommonConfig.ZOffset = 100000;
            TableCommonConfig.ResolutionZ = 265;
            TableCommonConfig.SID = 1143700;
            TableCommonConfig.FullSliceWidth = 47520;
            TableCommonConfig.ZChannelCount = 288;
            
            var result = testee.CalculateTableControlInfo(input);

            Assert.That(result.PreDeleteLength, Is.EqualTo(0));
        }

    }
}
