namespace NV.CT.Service.HardwareTest.Attachments.Interfaces
{
    public interface IModuleDirectory
    {
        /// <summary>
        /// 获取子模块目录路径
        /// </summary>
        string ModuleDataDirectoryPath { get; }

        /// <summary>
        /// 初始化子模块目录路径
        /// </summary>
        void InitializeModuleDirectory();
    }
}
