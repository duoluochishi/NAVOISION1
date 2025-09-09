//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom.Network;
using FellowOakDicom.Media;

namespace NV.CT.DicomUtility.Transfer.CStoreSCP
{
    public class C_StorageSCPAdapter
    {
        private static readonly Lazy<C_StorageSCPAdapter> _instance = new Lazy<C_StorageSCPAdapter>(() => new C_StorageSCPAdapter());
        public static C_StorageSCPAdapter Instance => _instance.Value;
        public List<DicomAssociation> CurrentActiveAssociations => _activeAssociations;


        public event EventHandler<DicomAssociation> StorageStarted;

        public event EventHandler<(DicomAssociation association, DicomDirectory dir)> StorageComplated;

        public event EventHandler<(int logLevel,string message)> StorageMessageReceived;

        private readonly List<DicomAssociation> _activeAssociations = new List<DicomAssociation>();

        private bool _isServiceStarted = false;
        private IDicomServer _dicomServer;

        private C_StorageSCPAdapter() { 

        }

        public string GetStorageRootPath()
        {
            //Todo: 后续使用系统配置的节点属性替换 
            return @"d:\tmp";
        }

        public int GetStoragePort()
        {
            //Todo: 后续使用系统配置的节点属性替换 
            return 5104;
        }

        public string GetCalledAETitle()
        {
            //Todo: 后续使用系统配置的节点属性替换 
            return "TEST_ImageServer";
        }

        public List<string> GetAcceptedCallingAETitle()
        {
            //Todo: 后续使用系统配置的节点属性替换 
            var result = new List<string>
            {
                "Test_ImageClient"
            };
            return result;
        }

        internal void RaiseStorageStarted(DicomAssociation association)
        {
            StorageStarted?.Invoke(this, association);
        }

        internal void RaiseStorageComplete(DicomAssociation association, DicomDirectory dicomDir)
        {
            StorageComplated?.Invoke(this, (association, dicomDir));
        }

        internal void RaiseMessage(int level, string message)
        {
            StorageMessageReceived?.Invoke(this,(level, message));
        }

        public void StartStorageService()
        {
            if (_isServiceStarted)
            {
                return;
            }


            _dicomServer = DicomServerFactory.Create<C_StoreSCP>(GetStoragePort());
            _isServiceStarted = true;
        }
    }
}
