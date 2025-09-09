using NV.CT.Service.AutoCali.Model;

namespace NV.CT.Service.AutoCali.DAL
{
    public class AutoCaliHistoryServiceImpl: IDtoService<CaliHistoryItem>
    {
        public static AutoCaliHistoryServiceImpl Instance { get => InnerInstance.impl; }

        public void Add(CaliHistoryItem item)
        {
            instanceCRUDImpl.Add(item);
        }

        public void Delete(CaliHistoryItem item)
        {
            instanceCRUDImpl.Delete(item);
        }

        public void Edit(CaliHistoryItem item)
        {
            instanceCRUDImpl.Update(item);
        }

        public List<CaliHistoryItem> Get()
        {
            return instanceCRUDImpl.Get();
        }

        public void Save(List<CaliHistoryItem> items)
        {
            instanceCRUDImpl.Save(items);
        }

        public void Update(CaliHistoryItem item)
        {
            instanceCRUDImpl.Update(item);
        }

        private AutoCaliHistoryServiceImpl()
        {
            instanceCRUDImpl = new AutoCaliHistoryDAO_FileImpl();
        }

        private IDtoService<CaliHistoryItem> instanceCRUDImpl;

        private static class InnerInstance
        {
            internal static AutoCaliHistoryServiceImpl? impl = null;
            static InnerInstance()
            {
                impl = new AutoCaliHistoryServiceImpl();//延迟加载
            }
        }
    }
}