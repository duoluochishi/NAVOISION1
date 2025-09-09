using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xaml.Behaviors;
using NV.CT.ProtocolManagement.ViewModels;
using NV.CT.ProtocolManagement.ViewModels.Enums;
using NV.CT.ProtocolManagement.ViewModels.Models;
using NV.CT.UI.Controls.Converter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NV.CT.ProtocolManagement.Views.English
{
    public partial class ProtocolTreeControl : UserControl
    {
        private ProtocolTreeControlViewModel _protocolTreeViewModel;
        private TreeViewItem CurrentTreeViewItem { get; set; }
        public ProtocolTreeControl()
        {
            InitializeComponent();
            _protocolTreeViewModel = Global.Instance.ServiceProvider?.GetRequiredService<ProtocolTreeControlViewModel>();
            _protocolTreeViewModel.NodeSelecting = NodeSelecting;
            _protocolTreeViewModel.Expanding = Expanding;
            _protocolTreeViewModel.Collapsing = Collapsing;

            DataContext = _protocolTreeViewModel;
        }

        private void NodeSelecting(string templateId)
        {
            //TreeViewItem selectedItem = null;
            var nodeId = _protocolTreeViewModel.TempPrepareExpandNode.SelectedNode.NodeId;
            TraverseTreeViewItems(protocolTreeView.Items, nodeId);
            if (CurrentTreeViewItem.Parent != null)
            {
                CurrentTreeViewItem.IsSelected = true;
                (CurrentTreeViewItem.Parent as TreeViewItem).IsExpanded = true;
            }
        }

        private void TraverseTreeViewItems(ItemCollection items, string nodeId)
        {
            foreach (var item in items)
            {
                // 处理当前节点
                // ...
                if (((Node)item).NodeId == nodeId)
                {
                    CurrentTreeViewItem = (TreeViewItem)protocolTreeView.ItemContainerGenerator.ContainerFromItem(item);
                    break;
                }
                var treeViewItem = (TreeViewItem)protocolTreeView.ItemContainerGenerator.ContainerFromItem(item);
                // 检查当前节点是否有子节点
                if (treeViewItem !=null&&treeViewItem.Items.Count > 0)
                {
                    // 递归遍历子节点
                    TraverseTreeViewItems(((TreeViewItem)protocolTreeView.ItemContainerGenerator.ContainerFromItem(item)).Items, nodeId);
                }
            }
        }

        private void Expanding()
        {
            if (null == protocolTreeView.SelectedItem) return;
            CurrentTreeViewItem.IsExpanded = true;
            if (((Node)protocolTreeView.SelectedItem).NodeType != NodeType.ProtocolNode.ToString()) return;
            CurrentTreeViewItem.ExpandSubtree();
            //foreach (var item in CurrentTreeViewItem.Items)
            //{
            //    if (item is TreeViewItem)
            //    {
            //        ((TreeViewItem)item).IsExpanded = true;
            //    }
            //}
        }
        private void Collapsing()
        {
            if (null == protocolTreeView.SelectedItem) return;
            CurrentTreeViewItem.IsExpanded = false;
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject? source)
        {
            while (source is not null && source.GetType() != typeof(T))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }

        /// <summary>
        /// 右键协议树显示菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) is TreeViewItem treeViewItem)
            {
                treeViewItem?.Focus();
                CurrentTreeViewItem = treeViewItem;
                e.Handled = true;
            }
            var TargetItem = GetNearestAboveContainer(e.OriginalSource as UIElement);
            if (TargetItem is not null)
            {
                ContextMenu? cm = FindResource("menu") as ContextMenu;
                cm.PlacementTarget = sender as TreeViewItem;
                cm.Placement = PlacementMode.MousePoint;
                cm.IsOpen = true;
                foreach (var item in cm.Items)
                {
                    InvokeCommandAction invokeCommandAction = new(){Command = ((ProtocolTreeControlViewModel)DataContext).Commands["RightMenuClickCommand"]};
                    MultiBinding multiBinding = new();
                    multiBinding.Bindings.Add(new Binding("Header") { Source = item });
                    multiBinding.Bindings.Add(new Binding("DataContext") { Source = TargetItem });
                    multiBinding.Converter = new TreeNodeParameterConvert();
                    (item as MenuItem).SetBinding(MenuItem.CommandParameterProperty, multiBinding);
                    invokeCommandAction.CommandParameter = (item as MenuItem).CommandParameter;
                    Microsoft.Xaml.Behaviors.EventTrigger eventTrigger = new("Click");
                    eventTrigger.Actions.Add(invokeCommandAction);
                    if (Interaction.GetTriggers(item as MenuItem).Count != 0)
                    {
                        Interaction.GetTriggers(item as MenuItem).RemoveAt(0);
                    }

                    Interaction.GetTriggers(item as MenuItem).Add(eventTrigger); 
                }
            }
        }

        private TreeViewItem GetNearestAboveContainer(UIElement? element)
        {
            TreeViewItem? container = element as TreeViewItem;
            while ((container is null) && (element is not null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            return container;
        }

        private void ProtocolTreeView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) is TreeViewItem treeViewItem)
            {
                CurrentTreeViewItem = treeViewItem;
                e.Handled = true;
            }
        }
    }
}
