using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.Util;
using NV.MPS.Environment;

namespace NV.CT.Service.AutoCali.DAL
{
    /// <summary>
    /// 校准历史的数据访问接口实现，采用文件方式保存
    /// </summary>
    internal abstract class AbstractCaliHistoryDAO_FileImpl : IDtoService<CaliHistoryItem>
    {
        public static readonly string AutoCaliHistoryFileName = "AutoCalibrationHistory.xml";
        public static readonly string DailyCaliHistoryFileName = "DailyCalibrationHistory.xml";
        protected abstract string HistoryFileName { get; set; }
        private string HistoryFullPath;

        public AbstractCaliHistoryDAO_FileImpl()
        {
            //string root = System.AppDomain.CurrentDomain.BaseDirectory;
            string root = Path.Combine(RuntimeConfig.Console.MCSAppData.Path, "AutoCalibration");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            HistoryFullPath = Path.Combine(root, HistoryFileName);
            Console.WriteLine($"Set the path:{HistoryFullPath}");
        }

        public void Add(CaliHistoryItem item)
        {
            throw new NotImplementedException();
        }

        public void Delete(CaliHistoryItem item)
        {
            throw new NotImplementedException();
        }

        public void Update(CaliHistoryItem item)
        {
            throw new NotImplementedException();
        }

        public List<CaliHistoryItem> Get()
        {
            List<CaliHistoryItem> items = null;
            try
            {
                items = XmlUtil.DeserializeFromFile<List<CaliHistoryItem>>(HistoryFullPath);
            }
            catch (Exception e)
            {
                //ToDo:使用Logger记录
                Console.WriteLine($"Get History Data Exception! Message:{e.ToString()}");
            }
            finally
            {
                if (items == null)
                {
                    items = new List<CaliHistoryItem>();
                }
            }
            return items;
        }

        public void Save(List<CaliHistoryItem> items)
        {
            try
            {
                XmlUtil.SaveToFile(HistoryFullPath, items);
            }
            catch (Exception e)
            {
                //ToDo:使用Logger记录
                Console.WriteLine($"Save History Data Exception! Message:{e.ToString()}");
            }
        }
    }

    internal class AutoCaliHistoryDAO_FileImpl : AbstractCaliHistoryDAO_FileImpl
    {
        protected override string HistoryFileName { get; set; } = AutoCaliHistoryFileName;
    }
    internal class DailyCaliHistoryDAO_FileImpl : AbstractCaliHistoryDAO_FileImpl
    {
        protected override string HistoryFileName { get; set; } = DailyCaliHistoryFileName;
    }
}
