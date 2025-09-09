using NV.CT.CTS.Enums;
using System.Collections.ObjectModel;

namespace NV.CT.UI.Controls.Export
{
    public class TreeItemBase
    {
        public TreeItemBase(TargetDiskType directoryTreeItemType, string name, string fullPath, string virtualPath, bool isSelected, bool isExpanded)
        {
            this.DirectoryTreeItemType = directoryTreeItemType;
            this.Name = name;
            this.FullPath = fullPath;
            this.VirtualPath = virtualPath;
            this.IsSelected = isSelected;
            this.IsExpanded = isExpanded;
        }

        public TargetDiskType DirectoryTreeItemType { get; set; }

        public TreeItemBase Parent { get; set; }

        public string Name { get; set; }

        public string FullPath { get; set; }

        public string VirtualPath { get; set; }

        public bool IsSelected { get; set; }

        public bool IsExpanded { get; set; }

    }

    public class Folder : TreeItemBase
    {
        public Folder(TargetDiskType directoryTreeItemType, string name, string fullPath,
                      string virtualPath, bool isSelected = false, bool isExpanded = false, bool isLogicalDisk = false)
            : base(directoryTreeItemType, name, fullPath, virtualPath, isSelected, isExpanded)
        {

            this.IsLogicalDisk = isLogicalDisk;
        }

        public ObservableCollection<Folder>? SubFolders { get; set; }

        //是否是逻辑磁盘，比如直接挂在USB节点或CDROM节点下的G:、H:
        public bool IsLogicalDisk { get; set; } = false;
    }

    public sealed class DriverType : TreeItemBase
    {
        public DriverType(TargetDiskType directoryTreeItemType, string name, string fullPath, string virtualPath, Folder[] folders, bool isSelected = false, bool isExpanded = false)
            : base(directoryTreeItemType, name, fullPath, virtualPath, isSelected, isExpanded)
        {
            Folders = new ObservableCollection<Folder>(folders);
        }

        public ObservableCollection<Folder> Folders { get; private set; }
    }

}
