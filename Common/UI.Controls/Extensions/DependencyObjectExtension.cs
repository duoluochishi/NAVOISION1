using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;

namespace NV.CT.UI.Controls.Extensions;
public static class DependencyObjectExtension
{
    public static List<T> FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
    {
        try
        {
            List<T> TList = new List<T> { };
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is not null && child is T)
                {
                    TList.Add((T)child);
                    List<T> childOfChildren = FindVisualChild<T>(child);
                    if (childOfChildren is not null)
                    {
                        TList.AddRange(childOfChildren);
                    }
                }
                else
                {
                    if (child is not null)
                    {
                        List<T> childOfChildren = FindVisualChild<T>(child);
                        if (childOfChildren is not null)
                        {
                            TList.AddRange(childOfChildren);
                        }
                    }
                }
            }
            return TList;
        }
        catch (Exception ee)
        {
            MessageBox.Show(ee.Message);
            return new List<T>();
        }
    }
}