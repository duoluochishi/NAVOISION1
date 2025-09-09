using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Service.Common.Helper
{
    public class CommonCalcuteHelper
    {
        public static int CONST_PreDeleteRatio = 1;

        private static uint OBJECT_FOV_150 = 150000;//微米
        private static uint OBJECT_FOV_300 = 300000;//微米
        private static uint OneCollimatorSliceWidth = (uint)(0.165 * 1000);//1 slice = 0.165mm, 看准直器开度CollimatorOpenWidth
        public static uint GetObjectFov(BodyPart bodyPart)
        {
            return bodyPart switch
            {
                BodyPart.HEAD => OBJECT_FOV_150,
                BodyPart.CHEST => OBJECT_FOV_300,
                _ => OBJECT_FOV_300,
            };
        }

        public static uint GetCollimatorSliceWidth(uint collimatorZ)
        {
            return collimatorZ * OneCollimatorSliceWidth;
        }
    }
}
