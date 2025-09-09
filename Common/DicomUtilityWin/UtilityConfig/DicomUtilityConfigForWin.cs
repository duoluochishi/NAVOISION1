//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 13:23:31     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using FellowOakDicom;
using NV.CT.DicomUtility.ImagingExtension;

namespace NV.CT.DicomUtility.UtilityConfig
{
    public static class DicomUtilityConfigForWin
    {
        public static void InitializeDicomUtilityConfig()
        {
            RegistDicomImageManagerAndEncoder();
        }

        private static void RegistDicomImageManagerAndEncoder()
        {
            new DicomSetupBuilder()
                .RegisterServices(s => s.AddImageManager<WinFormsImageManager>())
                .RegisterServices(s => s.AddFellowOakDicom().AddTranscoderManager<FellowOakDicom.Imaging.NativeCodec.NativeTranscoderManager>())
                .Build();
        }
    }
}
