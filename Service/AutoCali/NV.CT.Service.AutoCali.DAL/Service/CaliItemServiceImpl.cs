using NV.CT.Service.AutoCali.Model;

namespace NV.CT.Service.AutoCali.DAL
{
    public class CaliItemServiceImpl
    {
        public static CaliItemServiceImpl Instance { get => InnerInstance.impl; }

        private IList<CalibrationItem> mCacheCaliItems;
        public IList<CalibrationItem> CacheCaliItems { get => mCacheCaliItems ?? Get(); set => mCacheCaliItems = value; }

        private ICaliItemDAO caliItemDAOImpl;

        private CaliItemServiceImpl()
        {
            caliItemDAOImpl = new CaliItemDAO_FileImpl();
        }

        public IList<CalibrationItem> Get()
        {
            mCacheCaliItems = caliItemDAOImpl.Get();
            return mCacheCaliItems;
        }
        public void Save(IList<CalibrationItem> caliItems)
        {
            mCacheCaliItems = caliItems;
            caliItemDAOImpl.Save(caliItems);
        }

        private static class InnerInstance
        {
            internal static CaliItemServiceImpl impl;

            static InnerInstance()
            {
                impl = new CaliItemServiceImpl();
            }
        }
    }
}
