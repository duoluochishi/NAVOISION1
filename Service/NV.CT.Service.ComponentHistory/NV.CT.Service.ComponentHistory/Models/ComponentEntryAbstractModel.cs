using System.Collections.Generic;
using NV.CT.FacadeProxy.Common.Models.Components;

namespace NV.CT.Service.ComponentHistory.Models
{
    public abstract class ComponentEntryAbstractModel
    {
        /// <summary>
        /// 获取条目明细项
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<ComponentEntryItemModel> GetEntryItems();

        /// <summary>
        /// 获取可更新的条目明细项
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<ComponentEntryItemModel> GetCanUpdateEntryItems();
    }
}