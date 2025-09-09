using System.Linq;
using NV.CT.Service.ComponentHistory.Extensions;
using NV.CT.Service.ComponentHistory.Models;
using NV.MPS.Configuration;

namespace NV.CT.Service.ComponentHistory.Views
{
    /// <summary>
    /// HistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryWindow
    {
        public HistoryWindow(ComponentCategoryModel categoryItem)
        {
            InitializeComponent();
            Title = $"{categoryItem.Name} History";
            DataGridHistory.ItemsSource = GetHistoryData(categoryItem);
        }

        public HistoryWindow(ComponentEntryItemModel entryItem)
        {
            InitializeComponent();
            Title = $"{entryItem.SeniorName} {entryItem.Name} History";
            DataGridHistory.ItemsSource = GetHistoryData(entryItem);
        }

        private ComponentEntryItemHistoryModel[] GetHistoryData(ComponentCategoryModel category)
        {
            var items = category.GetAllEntryItems();
            var getInfos = items.Select(i => new ComponentInfo { ComponentType = i.ComponentType.ToDeviceComponentType(), Id = i.ID, });
            ComponentEntryItemHistoryModel[] historyInfos =
            [
                ..SystemConfig.GetComponentHistory(getInfos)
                              .Select(i =>
                               {
                                   var item = items.FirstOrDefault(x => x.ComponentType.ToDeviceComponentType() == i.ComponentType && x.ID == i.Id);
                                   var model = new ComponentEntryItemHistoryModel
                                   {
                                       Name = $"{item?.SeniorName} {item?.Name}",
                                       SN = i.SerialNumber,
                                       InstallTime = i.UsingBeginTime,
                                       RetireTime = i.UsingEndTime,
                                   };
                                   return model;
                               }),
                ..items.Select(i => i.ToHistoryModel()),
            ];
            return historyInfos.OrderByDescending(i => i.InstallTime).ToArray();
        }

        private ComponentEntryItemHistoryModel[] GetHistoryData(ComponentEntryItemModel item)
        {
            var getHistoryInfos = SystemConfig.GetComponentHistory(item.ComponentType.ToDeviceComponentType(), item.ID)
                                           .Select(i => new ComponentEntryItemHistoryModel
                                            {
                                                Name = $"{item.SeniorName} {item.Name}",
                                                SN = i.SerialNumber,
                                                InstallTime = i.UsingBeginTime,
                                                RetireTime = i.UsingEndTime,
                                            })
                                           .OrderByDescending(i => i.InstallTime);
            return [item.ToHistoryModel(), .. getHistoryInfos];
        }
    }
}