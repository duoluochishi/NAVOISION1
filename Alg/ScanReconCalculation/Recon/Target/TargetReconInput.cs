//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/14/04 14:02:21    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Recon.Target
{
    public class TargetReconInput
    {
        public ScanOption ScanOption { get; set; }
        public PatientPosition PatientPosition { get; set; }
        public int ReconVolumeStartPosition { get; set; }
        public int ReconVolumeEndPosition { get; set; }

        //public int PreDeleteLength { get; set; }
        //public int PostDeleteLength { get; set; }

        public int SmallAngleDeleteLength { get; set; }
        public int LargeAngleDeleteLength { get; set; }

        public int CollimatedSW { get; set; }

        public int FullSW { get; set; }

        public int CenterFirstX { get; set; }
        public int CenterFirstY { get; set; }
        public int CenterFirstZ { get; set; }
        public int CenterLastX { get; set; }
        public int CenterLastY { get; set; }
        public int CenterLastZ { get; set; }



        public int FoVLengthHor { get; set; }
        public int FoVLengthVert { get; set; }

        public TargetReconInput()
        {

        }

        public TargetReconInput(ScanOption scanoption,PatientPosition pp,
            int fullSW,int collimatedSW, int reconVolumnStart, int reconVolumnEnd,
            int centerFirstX,int centerFirstY,int centerFirstZ,
            int centerLastX,int centerLastY,int centerLastZ,
            int fovLengthHor,int fovLengthVer,
            int smallAngleDeleteLength, int largeAngleDeleteLength)
        {
            this.ScanOption = scanoption;
            this.PatientPosition = pp;
            this.FullSW = fullSW;
            this.CollimatedSW = collimatedSW;
            this.ReconVolumeStartPosition = reconVolumnStart;
            this.ReconVolumeEndPosition = reconVolumnEnd;
            this.CenterFirstX = centerFirstX;
            this.CenterFirstY = centerFirstY;
            this.CenterFirstZ = centerFirstZ;
            this.CenterLastX = centerLastX;
            this.CenterLastY = centerLastY;
            this.CenterLastZ = centerLastZ;
            this.FoVLengthHor = fovLengthHor;
            this.FoVLengthVert = fovLengthVer;
            this.SmallAngleDeleteLength = smallAngleDeleteLength;
            this.LargeAngleDeleteLength = largeAngleDeleteLength;   
        }

    }
}
