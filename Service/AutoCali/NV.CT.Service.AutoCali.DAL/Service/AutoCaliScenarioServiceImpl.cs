using NV.CT.Service.AutoCali.Model;

namespace NV.CT.Service.AutoCali.DAL
{
    public class AutoCaliScenarioServiceImpl : ICaliScenarioDAO, ICaliScenarioConfigDAO
    {
        public static AutoCaliScenarioServiceImpl Instance { get => InnerInstance.impl; }

        public void Add(CalibrationScenario caliScenario)
        {
            caliScenarioDAOImpl.Add(caliScenario);
        }

        public void Delete(CalibrationScenario caliScenario)
        {
            caliScenarioDAOImpl.Delete(caliScenario);
        }

        public void Edit(CalibrationScenario caliScenario)
        {
            caliScenarioDAOImpl.Update(caliScenario);
        }

        public List<CalibrationScenario> Get()
        {
            var scenarioList = caliScenarioDAOImpl.Get();
            return scenarioList;
        }

        public CaliScenarioConfig GetConfig()
        {
            if (!(caliScenarioDAOImpl is ICaliScenarioConfigDAO)) return null;

            var config = (caliScenarioDAOImpl as ICaliScenarioConfigDAO).GetConfig();
            return config;
        }

        public void Save(List<CalibrationScenario> caliScenarioList)
        {
            caliScenarioDAOImpl.Save(caliScenarioList);
        }

        public void Update(CalibrationScenario caliScenario)
        {
            caliScenarioDAOImpl.Update(caliScenario);
        }

        private AutoCaliScenarioServiceImpl()
        {
            caliScenarioDAOImpl = new AutoCaliScenarioDAO_FileImpl();
        }

        private ICaliScenarioDAO caliScenarioDAOImpl;

        private static class InnerInstance
        {
            internal static AutoCaliScenarioServiceImpl? impl = null;
            static InnerInstance()
            {
                impl = new AutoCaliScenarioServiceImpl();//延迟加载
            }
        }
    }
}
