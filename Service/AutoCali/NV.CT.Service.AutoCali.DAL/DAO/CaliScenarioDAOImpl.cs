using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.Util;

namespace NV.CT.Service.AutoCali.DAL
{
    /// <summary>
    /// 校准场景的数据访问接口
    /// </summary>
    public interface ICaliScenarioConfigDAO
    {
        /// <summary>
        /// 获取全部的校准场景
        /// </summary>
        /// <returns>校准场景列表</returns>
        CaliScenarioConfig GetConfig();
    }

    /// <summary>
    /// 校准场景的数据访问接口
    /// </summary>
    public interface ICaliScenarioDAO
    {
        /// <summary>
        /// 获取全部的校准场景
        /// </summary>
        /// <returns>校准场景列表</returns>
        List<CalibrationScenario> Get();

        /// <summary>
        /// 添加一个新的校准场景
        /// </summary>
        /// <returns></returns>
        void Add(CalibrationScenario caliScenario);

        /// <summary>
        /// 删除一个校准场景
        /// </summary>
        /// <returns></returns>
        void Delete(CalibrationScenario caliScenario);

        /// <summary>
        /// 更新一个校准场景
        /// </summary>
        /// <returns></returns>
        void Update(CalibrationScenario caliScenario);

        /// <summary>
        /// 保存全部的校准场景
        /// </summary>
        /// <returns></returns>
        void Save(List<CalibrationScenario> caliScenarioList);
    }

    /// <summary>
    /// 校准场景的数据访问接口实现，采用文件方式保存
    /// </summary>
    internal abstract class AbstractCaliScenarioDAO_FileImpl : ICaliScenarioDAO, ICaliScenarioConfigDAO
    {
        public static readonly string AutoCalibrationScenarioConfigName = "AutoCalibrationScenarioConfig.xml";
        public static readonly string DailyCalibrationScenarioConfigName = "DailyCalibrationScenarioConfig.xml";

        public abstract string ConfigName { get; set; }
        private string ConfigFullPath;

        public AbstractCaliScenarioDAO_FileImpl()
        {
            string root = System.AppDomain.CurrentDomain.BaseDirectory;
            ConfigFullPath = Path.Combine(root, ConfigName);
            Console.WriteLine($"Set the path:{ConfigFullPath}");
        }

        public void Add(CalibrationScenario caliScenario)
        {
            throw new NotImplementedException();
        }

        public void Delete(CalibrationScenario caliScenario)
        {
            throw new NotImplementedException();
        }

        public void Update(CalibrationScenario caliScenario)
        {
            throw new NotImplementedException();
        }

        public List<CalibrationScenario> Get()
        {
            return GetConfig().CalibrationScenarioGroup;
        }

        public void Save(List<CalibrationScenario> caliScenarioList)
        {
            //保存校准场景列表
            //TODO:config属性是否需要UI编辑并保存，待定
            _config.CalibrationScenarioGroup = caliScenarioList;
            XmlUtil.SaveToFile(ConfigFullPath, _config);
        }

        public CaliScenarioConfig GetConfig()
        {
            if (_config != null) return _config;

            _config = XmlUtil.DeserializeFromFile<CaliScenarioConfig>(ConfigFullPath);
            return _config;
        }

        private CaliScenarioConfig _config;
    }

    /// <summary>
    /// 自动校准通过配置文件访问数据
    /// </summary>
    internal class AutoCaliScenarioDAO_FileImpl : AbstractCaliScenarioDAO_FileImpl
    {
        public override string ConfigName { get; set; } = AutoCalibrationScenarioConfigName;
    }

    /// <summary>
    /// 日常校准通过配置文件访问数据
    /// </summary>
    internal class DailyCaliScenarioDAO_FileImpl : AbstractCaliScenarioDAO_FileImpl
    {
        public override string ConfigName { get; set; } = DailyCalibrationScenarioConfigName;
    }
}
