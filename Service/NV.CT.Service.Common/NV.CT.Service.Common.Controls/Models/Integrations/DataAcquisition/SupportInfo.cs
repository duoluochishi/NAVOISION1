namespace NV.CT.Service.Common.Controls.Models.Integrations.DataAcquisition
{
    public class SupportInfo
    {
        public SupportInfo()
        {
            Initialize();
        }

        #region Initialize

        private void Initialize() 
        {
            this.Slope0 = new float[3];
            this.Slope1 = new float[3];
        }

        #endregion

        /** 扫描床床码 **/
        public float TablePosition { get; set; }
        /** 扫描床旋转角度 **/
        public float GantryRotateAngle { get; set; }
        /** 源编号 **/
        public int SourceID { get; set; }
        /** 帧序列号 **/
        public int FrameSeriesNumber { get; set; }
        /** Slope信息 **/
        public float[] Slope0 { get; set; } = null!;
        public float[] Slope1 { get; set; } = null!;
    }
}
