using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.ComponentHistory.Models
{
    public class ComponentModuleEntryModel : ComponentEntryAbstractModel
    {
        /// <summary>
        /// 模组显示名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// 模组Index 从1开始
        /// </summary>
        public required int ModuleIndex { get; init; }

        /// <summary>
        /// 模组内的条目明细项集合
        /// </summary>
        public required ComponentEntryItemModel[] EntryItems { get; init; }

        public override IEnumerable<ComponentEntryItemModel> GetEntryItems()
        {
            return EntryItems;
        }

        public override IEnumerable<ComponentEntryItemModel> GetCanUpdateEntryItems()
        {
            return EntryItems.Where(item => item.IsCanUpdate);
        }
    }
}