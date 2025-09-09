namespace NV.CT.Service.QualityTest.Alg.Models
{
    internal class SliceThicknessAlgParam
    {
        /// <summary>
        /// 对应的 Image Path
        /// </summary>
        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// 金属丝1 矩形框
        /// </summary>
        public RectPoint Item1 { get; init; }

        /// <summary>
        /// 金属丝2 矩形框
        /// </summary>
        public RectPoint Item2 { get; init; }
    }
}