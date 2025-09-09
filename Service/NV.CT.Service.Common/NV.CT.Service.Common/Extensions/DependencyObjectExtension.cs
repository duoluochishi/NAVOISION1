using System;
using System.Windows;
using System.Windows.Media;

namespace NV.CT.Service.Common.Extensions
{
    public static class DependencyObjectExtension
    {
        public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }

            if (parentObject is T parent)
            {
                return parent;
            }

            return FindParent<T>(parentObject);
        }

        public static DependencyObject? FindParent(this DependencyObject child, Type targetType)
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }

            if (parentObject.GetType() == targetType)
            {
                return parentObject;
            }

            return parentObject.FindParent(targetType);
        }

        public static T? FindVisualChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var childObject = VisualTreeHelper.GetChild(parent, i);

                if (childObject is T child)
                {
                    return child;
                }

                var childOfChild = FindVisualChild<T>(childObject);

                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }
    }
}