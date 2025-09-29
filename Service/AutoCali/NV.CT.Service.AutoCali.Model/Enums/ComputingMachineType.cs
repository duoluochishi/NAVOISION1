namespace NV.CT.Service.AutoCali.Model
{
    /// <summary>
    /// 计算任务执行在那台机器上
    /// </summary>
    public enum ComputingMachineType
	{
		/// <summary>
		/// 计算任务在离线重建机上执行
		/// </summary>
		OfflineReconMachine = 0,

		/// <summary>
		/// 计算任务在主控机上执行
		/// </summary>
		MasterControlMachine = 1,
	}
}