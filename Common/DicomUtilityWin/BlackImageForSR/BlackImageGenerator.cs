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
using NV.CT.DicomUtility.BlackImageForSR;
using NV.CT.DicomUtility.DicomIOD;

namespace NV.CT.DicomUtility.BlackImage
{
    public class BlackImageGenerator
    {

        private static BlackImageGenerator _instance;

        public static BlackImageGenerator Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new BlackImageGenerator();
                }
                return _instance;
            }
        }

        public void GenerateBlackImage(SecondaryCaptureImageIOD iod, BlackImageConentInfo imageInfo, string path)
        {
            DicomDataset ds = new DicomDataset();
            iod.Update(ds);
            BlackImageHelper.Instance.UpdateBlackImageContent(imageInfo, ds);

            DicomFile df = new DicomFile(ds);
            df.Save(path);
        }
    }
}
