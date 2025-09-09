using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.Graphic
{
    public class TopoDirectionSpecification
    {

        private static Dictionary<PatientPosition, Dictionary<TubePosition, double[]>> 
            TopoDirectionDic = new Dictionary<PatientPosition, Dictionary<TubePosition, double[]>>();
            
        static TopoDirectionSpecification()
        {
            var hfs = new Dictionary<TubePosition, double[]>();
            hfs[TubePosition.Angle0] = new double[] { 1, 0, 0, 0, 0, -1 };
            hfs[TubePosition.Angle180] = new double[] { 1, 0, 0, 0, 0, -1 };
            hfs[TubePosition.Angle90] = new double[] { 0, 1, 0, 0, 0, -1 };
            hfs[TubePosition.Angle270] = new double[] { 0, 1, 0, 0, 0, -1 };

            var hfp = new Dictionary<TubePosition, double[]>();
            hfp[TubePosition.Angle0] = new double[] { -1, 0, 0, 0, 0, -1 };
            hfp[TubePosition.Angle180] = new double[] { -1, 0, 0, 0, 0, -1 };
            hfp[TubePosition.Angle90] = new double[] { 0, 1, 0, 0, 0, -1 };
            hfp[TubePosition.Angle270] = new double[] { 0, 1, 0, 0, 0, -1 };

            var ffs = new Dictionary<TubePosition, double[]>();
            ffs[TubePosition.Angle0] = new double[] { -1, 0, 0, 0, 0, -1 };
            ffs[TubePosition.Angle180] = new double[] { -1, 0, 0, 0, 0, -1 };
            ffs[TubePosition.Angle90] = new double[] { 0, 1, 0, 0, 0, -1 };
            ffs[TubePosition.Angle270] = new double[] { 0, 1, 0, 0, 0, -1 };

            var ffp = new Dictionary<TubePosition, double[]>();
            ffp[TubePosition.Angle0] = new double[] { 1, 0, 0, 0, 0, -1 };
            ffp[TubePosition.Angle180] = new double[] { 1, 0, 0, 0, 0, -1 };
            ffp[TubePosition.Angle90] = new double[] { 0, 1, 0, 0, 0, -1 };
            ffp[TubePosition.Angle270] = new double[] { 0, 1, 0, 0, 0, -1 };

            var hfdl = new Dictionary<TubePosition, double[]>();
            hfdl[TubePosition.Angle0] = new double[] { 0, -1, 0, 0, 0, -1 };
            hfdl[TubePosition.Angle180] = new double[] { 0, -1, 0, 0, 0, -1 };
            hfdl[TubePosition.Angle90] = new double[] { 1, 0, 0, 0, 0, -1 };
            hfdl[TubePosition.Angle270] = new double[] { 1, 0, 0, 0, 0, -1 };

            var hfdr = new Dictionary<TubePosition, double[]>();
            hfdr[TubePosition.Angle0] = new double[] { 0, 1, 0, 0, 0, -1 };
            hfdr[TubePosition.Angle180] = new double[] { 0, 1, 0, 0, 0, -1 };
            hfdr[TubePosition.Angle90] = new double[] { 1, 0, 0, 0, 0, -1 };
            hfdr[TubePosition.Angle270] = new double[] { 1, 0, 0, 0, 0, -1 };

            var ffdl = new Dictionary<TubePosition, double[]>();
            ffdl[TubePosition.Angle0] = new double[] { 0, -1, 0, 0, 0, -1 };
            ffdl[TubePosition.Angle180] = new double[] { 0, -1, 0, 0, 0, -1 };
            ffdl[TubePosition.Angle90] = new double[] { 1, 0, 0, 0, 0, -1 };
            ffdl[TubePosition.Angle270] = new double[] { 1, 0, 0, 0, 0, -1 };

            var ffdr = new Dictionary<TubePosition, double[]>();
            ffdr[TubePosition.Angle0] = new double[] { 0, 1, 0, 0, 0, -1 };
            ffdr[TubePosition.Angle180] = new double[] { 0, 1, 0, 0, 0, -1 };
            ffdr[TubePosition.Angle90] = new double[] { 1, 0, 0, 0, 0, -1 };
            ffdr[TubePosition.Angle270] = new double[] { 1, 0, 0, 0, 0, -1 };

            TopoDirectionDic[PatientPosition.HFS] = hfs;
            TopoDirectionDic[PatientPosition.HFP] = hfp;
            TopoDirectionDic[PatientPosition.FFS] = ffs;
            TopoDirectionDic[PatientPosition.FFP] = ffp;
            TopoDirectionDic[PatientPosition.HFDL] = hfdl;
            TopoDirectionDic[PatientPosition.HFDR] = hfdr;
            TopoDirectionDic[PatientPosition.FFDL] = ffdl;
            TopoDirectionDic[PatientPosition.FFDR] = ffdr;
        }

        public static double[] GetTopoDirection(PatientPosition pp, TubePosition tubePosition)
        {
            return TopoDirectionDic[pp][tubePosition];
        }

    }
}
