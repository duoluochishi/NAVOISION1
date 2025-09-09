//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using System;
using System.Data;
using System.Diagnostics;

namespace NV.CT.DicomUtility.Contract
{
    public static class DicomContentHelper
    {
        public static T GetDicomTag<T>(DicomDataset dataset, DicomTag dicomTag, int index = 0)
        {
            try
            {
                if (dataset is null)
                {
                    return default(T);
                }

                if (!dataset.TryGetValue(dicomTag, index, out T tagValue))
                {
                    return default(T);
                }

                return tagValue;
            }
            catch (Exception ex)
            {
                if(index != 0)  //有目的地获取失败
                { 
                    Trace.TraceWarning($"Get dicom tag {dicomTag} failed: {ex.Message}");
                }
                else
                {
                    //获取第0个值失败，即文件中相应字段没有值，不需要报警。
                    //todo: 后续考虑添加必须字段判断？
                }
                //Global.Logger.LogWarning($"Get dicom tag {dicomTag} failed: {ex.Message}");
                return default(T);
            }
        }

        public static DicomSequence TryGetDicomSequence(DicomDataset dataset, DicomTag dicomTag)
        {
            try
            {
                if (dataset is null)
                {
                    return default;
                }

                if (!dataset.TryGetSequence(dicomTag, out DicomSequence tagSequenceValue))
                {
                    return default;
                }

                return tagSequenceValue;
            }
            catch
            {
                //不存在对应Sequence，可能数据集中不存在。
                //Global.Logger.LogWarning($"Get dicom tag {dicomTag} failed: {ex.Message}");
                return default;
            }


        }

        public static T[] GetDicomTags<T>(DicomDataset dataset, DicomTag dicomTag)
        {
            try
            {
                if (dataset is null)
                {
                    return null;
                }

                if (!dataset.TryGetValues<T>(dicomTag, out T[] tagValues))
                {
                    return default;
                }

                return tagValues;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Get dicom tag {dicomTag} failed: {ex.Message}");
                //Global.Logger.LogWarning($"Get dicom tag {dicomTag} failed: {ex.Message}");
                return null;
            }
        }
        public static DateTime GetDicomDateTime(DicomDataset dataset, DicomTag dicomTagDate, DicomTag dicomTagTime)
        {
            try
            {
                if (dataset is null)
                {
                    return DateTime.MinValue;
                }

                return dataset.GetDateTime(dicomTagDate, dicomTagTime);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Get dicom tag {dicomTagDate} & {dicomTagTime} failed: {ex.Message}");
                //Global.Logger.LogWarning($"Get dicom tag {dicomTagDate} & {dicomTagTime} failed: {ex.Message}");
                return DateTime.MinValue;
            }
        }

    }
}
