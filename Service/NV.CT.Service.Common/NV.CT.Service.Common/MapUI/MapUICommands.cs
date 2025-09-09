using System;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Common.MapUI.Templates;

namespace NV.CT.Service.Common.MapUI
{
    public static class MapUICommands
    {
        public static RelayCommand<AbstractMapUITemplate> CollectionSetCommand { get; } = new(OnCollectionSet);

        private static void OnCollectionSet(AbstractMapUITemplate? template)
        {
            if (template == null)
            {
                return;
            }

            var win = new SetCollectionWindow(template);
            var handle = ServiceFramework.Global.Instance.MainWindowHwnd;

            if (handle == IntPtr.Zero)
            {
                win.Owner = Application.Current.MainWindow;
            }
            else
            {
                WindowOwnerHelper.SetWindowOwner(win, handle);
            }

            win.ShowDialog();
        }
    }
}