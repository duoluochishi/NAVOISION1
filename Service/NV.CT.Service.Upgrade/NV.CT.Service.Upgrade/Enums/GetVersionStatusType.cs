namespace NV.CT.Service.Upgrade.Enums
{
    public enum GetVersionStatusType
    {
        /// <summary>
        /// 初始状态，或不可升级时不读取版本号时的状态
        /// </summary>
        None,

        /// <summary>
        /// 成功获取到版本号
        /// <para>如果选择的升级包路径中包含此固件，则同时代表与升级包的版本号匹配(不匹配的会转为<see cref="Warning"/>)</para>
        /// </summary>
        Success,

        /// <summary>
        /// 成功获取到版本号，且选择的升级包路径中包含此固件，但与升级包的版本号不匹配
        /// </summary>
        Warning,

        /// <summary>
        /// 获取版本号发生错误
        /// </summary>
        Error,
    }
}