using System.Linq;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.Service.ComponentHistory.Enums;

namespace NV.CT.Service.ComponentHistory.Models
{
    public class ComponentCategoryModel
    {
        private ComponentEntryItemModel[]? _allEntryItems;

        /// <summary>
        /// 显示名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// 部件类别
        /// </summary>
        public required ComponentCategoryType CategoryType { get; init; }

        /// <summary>
        /// 此部件类别下包含的部件明细类型(即具体板子类型)集合
        /// </summary>
        public required SerialNumberComponentType[] ComponentTypes { get; init; }

        /// <summary>
        /// 此部件类别中的条目集合
        /// </summary>
        public required ComponentEntryAbstractModel[] Entries { get; init; }

        /// <summary>
        /// 获取此部件类别中的所有条目的明细项集合
        /// </summary>
        /// <returns></returns>
        public ComponentEntryItemModel[] GetAllEntryItems()
        {
            return _allEntryItems ??= Entries.SelectMany(i => i.GetEntryItems()).ToArray();
        }
    }
}