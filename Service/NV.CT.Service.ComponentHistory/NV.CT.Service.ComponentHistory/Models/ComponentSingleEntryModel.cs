using System.Collections.Generic;
using NV.CT.FacadeProxy.Common.Models.Components;

namespace NV.CT.Service.ComponentHistory.Models
{
    public class ComponentSingleEntryModel : ComponentEntryAbstractModel
    {
        /// <summary>
        /// 条目明细项
        /// </summary>
        public required ComponentEntryItemModel EntryItem { get; init; }

        public override IEnumerable<ComponentEntryItemModel> GetEntryItems()
        {
            yield return EntryItem;
        }

        public override IEnumerable<ComponentEntryItemModel> GetCanUpdateEntryItems()
        {
            if (EntryItem.IsCanUpdate)
            {
                yield return EntryItem;
            }
        }
    }
}