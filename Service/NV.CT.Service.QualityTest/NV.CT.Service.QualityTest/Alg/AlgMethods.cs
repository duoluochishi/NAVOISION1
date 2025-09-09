using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.QualityTest.Alg.Models;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Utilities;
using NV.MPS.Native.QualityTest;

namespace NV.CT.Service.QualityTest.Alg
{
    internal static class AlgMethods
    {
        private static readonly ILogService Logger;

        private static readonly JsonSerializerOptions SerializerOption = new()
        {
            IncludeFields = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };

        static AlgMethods()
        {
            Logger = Global.ServiceProvider.GetRequiredService<ILogService>();
        }

        /// <summary>
        /// 获取模体定位球
        /// </summary>
        /// <returns></returns>
        internal static ResultModel<PhantomBalls> GetPhantomLocateBalls(string filePath, string configFolder)
        {
            var param = new BPRParam()
            {
                FilePath = filePath,
                ConfigPath = configFolder,
            };
            var ret = new BPRRet() { BallPos = new BallPosition[4] };
            var name = nameof(QTNativeMethods.BPRQA);
            var result = RunFunction(() => QTNativeMethods.BPRQA(param, ref ret), name, param);

            if (!result.IsSuccess)
            {
                return ResultModel<PhantomBalls>.Create(false, "Alg return false", result.ErrorCode);
            }

            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {name} Success, Ret: {Serialize(ret)}");
            var res = new PhantomBalls()
            {
                BallTop = ret.BallPos[0].ToPoint3D(),
                BallLeft = ret.BallPos[1].ToPoint3D(),
                BallBottom = ret.BallPos[2].ToPoint3D(),
                BallRight = ret.BallPos[3].ToPoint3D(),
            };

            return ResultModel<PhantomBalls>.Create(true, res);
        }

        /// <summary>
        /// 获取测量层厚项目的金属丝框位置
        /// </summary>
        /// <returns></returns>
        internal static ResultModel<MetalWireAlgModel> MetalWireIdentify(string filePath, string configFolder)
        {
            var param = new SSPParam()
            {
                PathFile = filePath,
                PathConfig = configFolder,
                IsAuto = true,
                PosAChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
                PosBChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
            };
            var ret = new SSPRet()
            {
                SSP = new float[2],
                PosAAutoChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
                PosBAutoChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
            };
            var name = nameof(QTNativeMethods.SSPQA);
            var result = RunFunction(() => QTNativeMethods.SSPQA(param, ref ret), name, param);

            if (!result.IsSuccess)
            {
                return ResultModel<MetalWireAlgModel>.Create(false, "Alg return false", result.ErrorCode);
            }

            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {name} Success, Ret: {Serialize(ret)}");
            var res = new MetalWireAlgModel()
            {
                Item1 = ret.PosAAutoChoose.ToRectPoint(),
                Item2 = ret.PosBAutoChoose.ToRectPoint(),
            };

            return ResultModel<MetalWireAlgModel>.Create(true, res);
        }

        /// <summary>
        /// 轴向层厚算法
        /// </summary>
        /// <returns></returns>
        internal static ResultModel<SliceThicknessAlgModel> SliceThicknessAxial(SliceThicknessAlgParam algParam, string configFolder)
        {
            var param = new SSPParam()
            {
                PathFile = algParam.Path,
                PathConfig = configFolder,
                IsAuto = false,
                PosAChoose = algParam.Item1.ToNVRECT(),
                PosBChoose = algParam.Item2.ToNVRECT(),
            };
            var ret = new SSPRet()
            {
                SSP = new float[2],
                PosAAutoChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
                PosBAutoChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
            };
            var name = nameof(QTNativeMethods.SSPQA);
            var result = RunFunction(() => QTNativeMethods.SSPQA(param, ref ret), name, param);

            if (!result.IsSuccess)
            {
                return ResultModel<SliceThicknessAlgModel>.Create(false, "Alg return false", result.ErrorCode);
            }

            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {name} Success, Ret: {Serialize(ret)}");
            var res = new SliceThicknessAlgModel()
            {
                SliceThickness1 = ret.SSP[0],
                SliceThickness2 = ret.SSP[1],
            };

            return ResultModel<SliceThicknessAlgModel>.Create(true, res);
        }

        /// <summary>
        /// 螺旋层厚算法
        /// </summary>
        /// <returns></returns>
        internal static ResultModel<SliceThicknessAlgModel> SliceThicknessSpiral(SliceThicknessAlgParam algParam, string configFolder)
        {
            var param = new SSPParam()
            {
                PathFile = algParam.Path,
                PathConfig = configFolder,
                IsAuto = false,
                PosAChoose = algParam.Item1.ToNVRECT(),
                PosBChoose = algParam.Item2.ToNVRECT(),
            };
            var ret = new SSPRet()
            {
                SSP = new float[2],
                PosAAutoChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
                PosBAutoChoose = new NVRECT() { LeftTop = new NVPOINT(), RightBottom = new NVPOINT() },
            };
            var name = nameof(QTNativeMethods.SSPQA);
            var result = RunFunction(() => QTNativeMethods.SSPQA(param, ref ret), name, param);

            if (!result.IsSuccess)
            {
                return ResultModel<SliceThicknessAlgModel>.Create(false, "Alg return false", result.ErrorCode);
            }

            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {name} Success, Ret: {Serialize(ret)}");
            var res = new SliceThicknessAlgModel()
            {
                SliceThickness1 = ret.SSP[0],
                SliceThickness2 = ret.SSP[1],
            };

            return ResultModel<SliceThicknessAlgModel>.Create(true, res);
        }

        /// <summary>
        /// 获取MTF项目的金属丝框位置
        /// </summary>
        /// <returns></returns>
        internal static ResultModel<MetalWireAlgModel> MetalIdentify(string filePath, string configFolder)
        {
            var param = new SSPMTFPositionParam()
            {
                PathFile = filePath,
                PathConfig = configFolder,
            };
            var ret = new NVRECT()
            {
                LeftTop = new NVPOINT(),
                RightBottom = new NVPOINT(),
            };
            var name = nameof(QTNativeMethods.SSPQA_MTFPosition);
            var result = RunFunction(() => QTNativeMethods.SSPQA_MTFPosition(param, ref ret), name, param);

            if (!result.IsSuccess)
            {
                return ResultModel<MetalWireAlgModel>.Create(false, "Alg return false", result.ErrorCode);
            }

            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {name} Success, Ret: {Serialize(ret)}");
            var res = new MetalWireAlgModel()
            {
                Item1 = ret.ToRectPoint(),
            };

            return ResultModel<MetalWireAlgModel>.Create(true, res);
        }

        /// <summary>
        /// XY-MTF
        /// </summary>
        /// <returns></returns>
        internal static ResultModel<MTFAlgModel> MTF_XY(MTFAlgParam algParam, string configFolder)
        {
            var param = new MTFParam()
            {
                FilePath = algParam.Path,
                ConfigPath = configFolder,
                IsMTFZ = false,
                StartFile = 0,
                EndFile = 0,
                NVRectSet = algParam.Item.ToNVRECT(),
            };
            var ret = new MTFRet();
            var name = nameof(QTNativeMethods.MTFQA);
            var result = RunFunction(() => QTNativeMethods.MTFQA(param, ref ret), name, param);

            if (!result.IsSuccess)
            {
                return ResultModel<MTFAlgModel>.Create(false, "Alg return false", result.ErrorCode);
            }

            var doubleArr = new double[ret.Size * 2];
            Marshal.Copy(ret.Pointer, doubleArr, 0, doubleArr.Length);
            var array = doubleArr.Chunk(2).Select(i => new Point2D(i[0], i[1])).ToArray();
            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {name} Success, Ret: {Serialize(ret)}, Array: {SerializeUtility.JsonSerialize(array)}");
            var res = new MTFAlgModel()
            {
                MTF0 = ret.MTF_0,
                MTF2 = ret.MTF_1,
                MTF10 = ret.MTF_2,
                MTF50 = ret.MTF_3,
                MTFArray = array,
            };

            return ResultModel<MTFAlgModel>.Create(true, res);
        }

        /// <summary>
        /// Z-MTF
        /// </summary>
        /// <returns></returns>
        internal static ResultModel<MTFAlgModel> MTF_Z(MTFAlgParam algParam, string configFolder)
        {
            var fileCount = Directory.GetFiles(algParam.Path).Length;
            var param = new MTFParam()
            {
                FilePath = algParam.Path,
                ConfigPath = configFolder,
                IsMTFZ = true,
                StartFile = 0,
                EndFile = fileCount > 0 ? fileCount - 1 : 0,
                NVRectSet = algParam.Item.ToNVRECT(),
            };
            var ret = new MTFRet();
            var name = nameof(QTNativeMethods.MTFQA);
            var result = RunFunction(() => QTNativeMethods.MTFQA(param, ref ret), name, param);

            if (!result.IsSuccess)
            {
                return ResultModel<MTFAlgModel>.Create(false, "Alg return false", result.ErrorCode);
            }

            var doubleArr = new double[ret.Size * 2];
            Marshal.Copy(ret.Pointer, doubleArr, 0, doubleArr.Length);
            var array = doubleArr.Chunk(2).Select(i => new Point2D(i[0], i[1])).ToArray();
            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {name} Success, Ret: {Serialize(ret)}, Array: {SerializeUtility.JsonSerialize(array)}");
            var res = new MTFAlgModel()
            {
                MTF0 = ret.MTF_0,
                MTF2 = ret.MTF_1,
                MTF10 = ret.MTF_2,
                MTF50 = ret.MTF_3,
                MTFArray = array,
            };

            return ResultModel<MTFAlgModel>.Create(true, res);
        }

        private static T RunFunction<T>(Func<T> func, string funcName, object? param = null)
        {
            var paramStr = param == null ? string.Empty : $"Parameters: {Serialize(param)}";
            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {funcName} Start. {(paramStr)}");
            var stopwatch = Stopwatch.StartNew();
            var t = func();
            stopwatch.Stop();
            Logger.Info(ServiceCategory.QualityTest, $"NativeMethod {funcName} End. Time Cost: {stopwatch.ElapsedMilliseconds}ms. Result: {Serialize(t)}");
            return t;
        }

        private static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, SerializerOption);
        }
    }
}