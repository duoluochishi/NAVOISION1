using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NV.CT.Service.Common.Controls.CustomControls
{
    public class MonitorGrid : Grid
    {
        #region DependencyProperty / AttachedProperty

        public static readonly DependencyProperty IsMonitorErrorsProperty = DependencyProperty.RegisterAttached("IsMonitorErrors", typeof(bool), typeof(MonitorGrid), new PropertyMetadata(false));
        public static readonly DependencyProperty HasMonitorErrorsProperty = DependencyProperty.Register(nameof(HasMonitorErrors), typeof(bool), typeof(MonitorGrid), new PropertyMetadata(false));

        #endregion

        private readonly Dictionary<DependencyObject, bool> _errorDic = [];

        static MonitorGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MonitorGrid), new FrameworkPropertyMetadata(typeof(MonitorGrid)));
        }

        public MonitorGrid()
        {
            Loaded += MonitorGrid_Loaded;
        }

        public static bool GetIsMonitorErrors(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitorErrorsProperty);
        }

        public static void SetIsMonitorErrors(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitorErrorsProperty, value);
        }

        public bool HasMonitorErrors
        {
            get => (bool)GetValue(HasMonitorErrorsProperty);
            set => SetValue(HasMonitorErrorsProperty, value);
        }

        private void MonitorGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MonitorGrid_Loaded;
            var monitorErrorsItems = FindMonitorErrorsVisualChildren(this);

            foreach (var item in monitorErrorsItems)
            {
                _errorDic.Add(item, (bool)item.GetValue(Validation.HasErrorProperty));
                var desc = DependencyPropertyDescriptor.FromProperty(Validation.HasErrorProperty, item.GetType());
                desc.AddValueChanged(item, OnHasErrorChanged);
            }

            UpdateHasMonitorErrors();
        }

        private void OnHasErrorChanged(object? sender, EventArgs e)
        {
            if (sender is not DependencyObject depObj)
            {
                return;
            }

            _errorDic[depObj] = (bool)depObj.GetValue(Validation.HasErrorProperty);
            UpdateHasMonitorErrors();
        }

        private void UpdateHasMonitorErrors()
        {
            var hasMonitorErrors = _errorDic.Values.Any(i => i);

            if (HasMonitorErrors != hasMonitorErrors)
            {
                HasMonitorErrors = hasMonitorErrors;
            }
        }

        private List<DependencyObject> FindMonitorErrorsVisualChildren(DependencyObject depObj)
        {
            var children = new List<DependencyObject>();

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (GetIsMonitorErrors(child))
                {
                    children.Add(child);
                }

                children.AddRange(FindMonitorErrorsVisualChildren(child));
            }

            return children;
        }
    }
}