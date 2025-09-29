using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.Graphic
{
    public class TomoDirectionSpecification
    {

        private static Dictionary<PatientPosition, double[]>
            TomoDirectionDic = new Dictionary<PatientPosition, double[]>();

        static TomoDirectionSpecification()
        {
            var hfs = new double[] { 1, 0, 0, 0, 1, 0 };

            var hfp = new double[] { -1, 0, 0, 0, -1, 0 };

            var ffs = new double[] { 1, 0, 0, 0, 1, 0 };

            var ffp = new double[] {-1, 0, 0, 0, -1, 0 };

            var hfdl = new double[] {0, -1, 0, 1, 0, 0 };

            var hfdr = new double[] {0, 1, 0, -1, 0, 0 };

            var ffdl = new double[] {0, 1, 0, 1, 0, 0 };

            var ffdr = new double[] {0, -1, 0, -1, 0, 0 };

            TomoDirectionDic[PatientPosition.HFS] = hfs;
            TomoDirectionDic[PatientPosition.HFP] = hfp;
            TomoDirectionDic[PatientPosition.FFS] = ffs;
            TomoDirectionDic[PatientPosition.FFP] = ffp;
            TomoDirectionDic[PatientPosition.HFDL] = hfdl;
            TomoDirectionDic[PatientPosition.HFDR] = hfdr;
            TomoDirectionDic[PatientPosition.FFDL] = ffdl;
            TomoDirectionDic[PatientPosition.FFDR] = ffdr;
        }

        public static double[] GetTomoDirection(PatientPosition pp)
        {
            return TomoDirectionDic[pp];
        }
    }
}
