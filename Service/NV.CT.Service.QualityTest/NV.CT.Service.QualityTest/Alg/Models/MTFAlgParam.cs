namespace NV.CT.Service.QualityTest.Alg.Models
{
    internal class MTFAlgParam
    {
        /// <summary>
        /// 对应的 Image Path 或 Image文件夹路径
        /// </summary>
        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// 金属丝矩形框
        /// </summary>
        public RectPoint Item { get; init; }
    }
}