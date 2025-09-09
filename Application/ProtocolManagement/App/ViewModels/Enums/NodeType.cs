namespace NV.CT.ProtocolManagement.ViewModels.Enums
{
    /// <summary>
    /// Defines the NodeType.
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// 协议结点
        /// </summary>
        ProtocolNode,

        /// <summary>
        /// 参照系节点(体位)
        /// </summary>
        FrameOfReferenceNode,

        /// <summary>
        /// 连扫节点
        /// </summary>
        MeasurementNode,

        /// <summary>
        /// 扫描节点
        /// </summary>
        ScanNode,

        /// <summary>
        /// 重建节点
        /// </summary>
        ReconNode,
    }
}