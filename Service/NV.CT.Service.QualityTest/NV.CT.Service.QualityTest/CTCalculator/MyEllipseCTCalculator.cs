using System.Linq;
using NV.CT.Service.QualityTest.Enums;
using NV.MPS.Annotations;
using NV.MPS.Annotations.Calculators;

namespace NV.CT.Service.QualityTest.CTCalculator
{
    public class MyEllipseCTCalculator : EllipseCTCalculator
    {
        public const string DValue = nameof(DValue);
        public const string Noise = nameof(Noise);

        public override ResultCollection Calculate(CalculatorParameters para)
        {
            var resultCollection = base.Calculate(para);

            switch (Global.CurrentQTType)
            {
                case QTType.Homogeneity:
                {
                    var result = new Result
                    {
                        ResultName = DValue,
                        Prefix = DValue,
                        Postfix = AnnotationsStringTable.NMS_CT_Annotation_CTValue_Unit_HU,
                        Separator = AnnotationsStringTable.NMS_CT_Annotation_ColonSeparator,
                    };
                    resultCollection.ResultList.Add(result);
                    break;
                }
                case QTType.NoiseOfWater:
                {
                    var noiseValue = (resultCollection.ResultList.FirstOrDefault(x => x.ResultName == ResultNameTable.StdDev)?.Content ?? 0) / 1000;
                    var result = new Result
                    {
                        ResultName = Noise,
                        Prefix = Noise,
                        Postfix = "%",
                        Precision = 3,
                        Separator = AnnotationsStringTable.NMS_CT_Annotation_ColonSeparator,
                        Content = noiseValue
                    };
                    resultCollection.ResultList.Add(result);
                    break;
                }
            }

            return resultCollection;
        }
    }
}