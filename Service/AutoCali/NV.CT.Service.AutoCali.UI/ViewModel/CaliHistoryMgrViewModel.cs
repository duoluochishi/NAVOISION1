using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.AutoCali.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.Service.AutoCali.Logic
{
    public class CaliHistoryMgrViewModel
    {
        public CaliHistoryMgrViewModel()
        {
            var service = AutoCaliHistoryServiceImpl.Instance;
            var historyItems = service.Get();
            HistoryItems = new ObservableCollection<CaliHistoryItem>(historyItems);
        }

        public ObservableCollection<CaliHistoryItem> HistoryItems { get; private set; }

        /// <summary>
        /// 添加校准历史条目
        /// </summary>
        /// <param name="item"></param>
        public void AddHistoryItem(CaliHistoryItem item)
        {
            HistoryItems.Add(item);

            AutoCaliHistoryServiceImpl.Instance.Save(HistoryItems.ToList());
        }
    }
}
