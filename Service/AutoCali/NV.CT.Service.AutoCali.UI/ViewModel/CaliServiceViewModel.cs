using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.AutoCali.Logic;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.UI.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 自动校准的ViewModel
    /// </summary>
    internal class CaliServiceViewModel : BindableBase
    {
        public CaliServiceViewModel()
        {
            CaliHistoryMgrViewModel = new CaliHistoryMgrViewModel();
        }

        public CaliHistoryMgrViewModel CaliHistoryMgrViewModel { get; set; }
    }
}
