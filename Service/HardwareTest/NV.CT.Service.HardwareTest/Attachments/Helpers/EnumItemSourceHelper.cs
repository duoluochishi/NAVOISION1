using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public class EnumItemSourceHelper
    {

        public static bool GetEnumValuesToItemsSourceProperty(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnumValuesToItemsSourceProperty);
        }

        public static void SetEnumValuesToItemsSourceProperty(DependencyObject obj, bool value)
        {
            obj.SetValue(EnumValuesToItemsSourceProperty, value);
        }
       
        public static readonly DependencyProperty EnumValuesToItemsSourceProperty =
            DependencyProperty.RegisterAttached(
                "EnumValuesToItemsSource", 
                typeof(bool), 
                typeof(EnumItemSourceHelper),
                new PropertyMetadata(default(bool), OnEnumValuesToItemsSourceChanged));

        private static void OnEnumValuesToItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl && GetEnumValuesToItemsSourceProperty(itemsControl))
            {
                if (itemsControl.IsLoaded)
                {
                    SetItemsSource(itemsControl);
                }
                else
                {
                    itemsControl.Loaded += ItemsControl_Loaded;
                }
            }
        }

        private static void SetItemsSource(ItemsControl itemsControl)
        {
            var itemsBingdingExpression = BindingOperations.GetBinding(itemsControl, ItemsControl.ItemsSourceProperty);

            if (itemsBingdingExpression != null)
            {
                throw new InvalidOperationException("When using ItemsControlHelper.EnumValuesToItemsSource, cannot be used ItemsSource at the same time.");
            }

            if (itemsControl.Items.Count > 0)
            {
                throw new InvalidOperationException("When using ItemsControlHelper.EnumValuesToItemsSource, Items Collection must be null");
            }

            var bindingExpression = BindingOperations.GetBindingExpression(itemsControl, Selector.SelectedItemProperty);

            if (bindingExpression == null)
            {
                throw new InvalidOperationException("ItemsControl must be binding SelectedItem property");
            }

            var binding = bindingExpression.ParentBinding;
            var dataType = bindingExpression.DataItem?.GetType();
            var paths = binding.Path.Path.Split('.');

            foreach (var path in paths)
            {
                var propertyInfo = dataType?.GetProperty(path);
                if (propertyInfo == null) { return; }
                dataType = propertyInfo.PropertyType;
            }

            if (!dataType!.IsEnum)
            {
                var underlyingType = Nullable.GetUnderlyingType(dataType);
                if (underlyingType == null)
                {
                    return;
                }
                dataType = underlyingType;
            }

            var itemsSourceBinding = new Binding();
            itemsSourceBinding.Source = Enum.GetValues(dataType);
            itemsSourceBinding.Mode = BindingMode.OneWay;
            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, itemsSourceBinding);
        }

        private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = (ItemsControl)sender;
            itemsControl.Loaded -= ItemsControl_Loaded;
            SetItemsSource(itemsControl);
        }
    }
}
