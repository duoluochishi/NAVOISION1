//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Windows.Media;
using Enum = System.Enum;
using Type = System.Type;

namespace NV.CT.UI.Exam.Extensions;
public class Extensions
{
    /// <summary>
    /// 将枚举类型转成结合多语言机制的ItemModel的list数据源
    /// </summary>
    /// <param name="enumType">枚举类型</param>
    /// <returns></returns>
    public static ObservableCollection<KeyValuePair<int, string>> EnumToItems(Type enumType)
    {
        ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
        foreach (var enumItem in Enum.GetValues(enumType))
        {
            var value = LanguageResource.ResourceManager.GetString("Enum_" + enumType.Name + "_" + (enumItem is null ? "" : enumItem.ToString()));
            if (!string.IsNullOrEmpty(value))
            {
                list.Add(new KeyValuePair<int, string>(enumItem is null ? 0 : (int)enumItem, value));
            }
        }
        return list;
    }

    public static ObservableCollection<KeyValuePair<int, string>> EnumToItems(ICollection<string> list, int magnify = 1)
    {
        var resultList = new ObservableCollection<KeyValuePair<int, string>>();
        list?.Distinct().ForEach(item =>
        {
            var isSuccess = decimal.TryParse(item, out decimal decimalVal);
            var intVal = (int)(decimalVal * magnify);
            resultList.Add(isSuccess
                ? new KeyValuePair<int, string>(intVal, item)
                : new KeyValuePair<int, string>(0, item));
        });

        return resultList;
    }

    public static ObservableCollection<KeyValuePair<int, string>> EnumToList(Type enumType)
    {
        ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
        foreach (var enumItem in Enum.GetValues(enumType))
        {
            list.Add(new KeyValuePair<int, string>(enumItem is null ? 0 : (int)enumItem, enumItem?.ToString() ?? string.Empty));
        }
        return list;
    }

    /// <summary>
    /// 图像水平翻转
    /// </summary>
    /// <param name="bmp">原来图像</param>
    /// <returns></returns>
    public static Bitmap HorizontalFlip(Bitmap bmp)
    {
        var width = bmp.Width;
        var height = bmp.Height;
        Graphics g = Graphics.FromImage(bmp);
        Rectangle rect = new Rectangle(0, 0, width, height);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
        g.DrawImage(bmp, rect);

        return bmp;
    }

    /// <summary>
    /// 图像原点旋转180度
    /// </summary>
    /// <param name="bmp">原来图像</param>
    /// <returns></returns>
    public static Bitmap Rotate180FlipNone(Bitmap bmp)
    {
        var width = bmp.Width;
        var height = bmp.Height;
        Graphics g = Graphics.FromImage(bmp);
        Rectangle rect = new Rectangle(0, 0, width, height);
        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
        g.DrawImage(bmp, rect);
        return bmp;
    }

    /// <summary>
    /// 图像垂直翻转
    /// </summary>
    /// <param name="bit">原来图像</param>
    /// <returns></returns>
    public static Bitmap VerticalFlip(Bitmap bmp)
    {
        var width = bmp.Width;
        var height = bmp.Height;
        Graphics g = Graphics.FromImage(bmp);
        Rectangle rect = new Rectangle(0, 0, width, height);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        g.DrawImage(bmp, rect);

        return bmp;
    }

    /// <summary>
    /// 顺时针旋转90度
    /// </summary>
    /// <param name="bmp"></param>
    /// <returns></returns>
    public static Bitmap Rotate90FlipNone(Bitmap bmp)
    {
        var width = bmp.Width;
        var height = bmp.Height;
        Graphics g = Graphics.FromImage(bmp);
        Rectangle rect = new Rectangle(0, 0, height, width);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
        g.DrawImage(bmp, rect);
        return bmp;
    }

    /// <summary>
    /// 逆时针旋转90度
    /// </summary>
    /// <param name="bmp"></param>
    /// <returns></returns>
    public static Bitmap Rotate90FlipXY(Bitmap bmp)
    {
        var width = bmp.Width;
        var height = bmp.Height;
        Graphics g = Graphics.FromImage(bmp);
        Rectangle rect = new Rectangle(0, 0, height, width);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipXY);
        g.DrawImage(bmp, rect);
        return bmp;
    }

    /// <summary>
    /// 逆时针旋转270度
    /// </summary>
    /// <param name="bmp"></param>
    /// <returns></returns>
    public static Bitmap Rotate90FlipX(Bitmap bmp)
    {
        var width = bmp.Width;
        var height = bmp.Height;
        Graphics g = Graphics.FromImage(bmp);
        Rectangle rect = new Rectangle(0, 0, height, width);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
        g.DrawImage(bmp, rect);
        return bmp;
    }

    /// <summary>
    /// 顺时针旋转270度
    /// </summary>
    /// <param name="bmp"></param>
    /// <returns></returns>
    public static Bitmap Rotate90FlipY(Bitmap bmp)
    {
        var width = bmp.Width;
        var height = bmp.Height;
        Graphics g = Graphics.FromImage(bmp);
        Rectangle rect = new Rectangle(0, 0, height, width);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipY);
        g.DrawImage(bmp, rect);
        return bmp;
    }

    /// <summary>
    /// 查找父控件
    /// </summary>
    /// <typeparam name="T">父控件的类型</typeparam>
    /// <param name="obj">要找的是obj的父控件</param>
    /// <param name="name">想找的父控件的Name属性</param>
    /// <returns>目标父控件</returns>
    public static T? GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
    {
        DependencyObject parent = VisualTreeHelper.GetParent(obj);

        while (parent is not null)
        {
            if (parent is T && (((T)parent).Name.Equals(name) | string.IsNullOrEmpty(name)))
            {
                return (T)parent;
            }
            // 在上一级父控件中没有找到指定名字的控件，就再往上一级找
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    /// <summary>
    /// 查找子控件
    /// </summary>
    /// <typeparam name="T">子控件的类型</typeparam>
    /// <param name="obj">要找的是obj的子控件</param>
    /// <param name="name">想找的子控件的Name属性</param>
    /// <returns>目标子控件</returns>
    public static T? GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
    {
        DependencyObject? child = null;
        T? grandChild = default;

        for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
        {
            child = VisualTreeHelper.GetChild(obj, i);

            if (child is T && (((T)child).Name.Equals(name) | string.IsNullOrEmpty(name)))
            {
                return (T)child;
            }
            else
            {
                // 在下一级中没有找到指定名字的子控件，就再往下一级找
                grandChild = GetChildObject<T>(child, name);
                if (grandChild is not null)
                {
                    return grandChild;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 获取所有同一类型的子控件
    /// </summary>
    /// <typeparam name="T">子控件的类型</typeparam>
    /// <param name="obj">要找的是obj的子控件集合</param>
    /// <param name="name">想找的子控件的Name属性</param>
    /// <returns>子控件集合</returns>
    public static List<T> GetChildObjects<T>(DependencyObject obj, string name) where T : FrameworkElement
    {
        DependencyObject? child = null;
        List<T> childList = new List<T>();

        for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
        {
            child = VisualTreeHelper.GetChild(obj, i);

            if (child is T && (((T)child).Name.Equals(name) || string.IsNullOrEmpty(name)))
            {
                childList.Add((T)child);
            }
            childList.AddRange(GetChildObjects<T>(child, ""));
        }
        return childList;
    }
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj is not null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                if (child is not null && child is T)
                {
                    yield return (T)child;
                }
                if (child is not null)
                {
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }

    public static string GetMaxFixedLengthStrings(string str, int maxLength)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > maxLength)
        {
            str = str.Substring(0, maxLength);
        }
        return str;
    }

    /// <summary>
    /// 范围判断函数，检查给定的值是否在指定的最小值和最大值之间。
    /// 例如，可以用来判断当前日期是否在开始日期和结束日期之间。
    /// 该方法适用于任何实现了 IComparable 接口的类型，比如 int、double、DateTime 等等。
    /// </summary>
    /// <typeparam name="T">实现了 IComparable 接口的泛型类型参数</typeparam>
    /// <param name="value">要比较的值</param>
    /// <param name="min">范围的最小值</param>
    /// <param name="max">范围的最大值</param>
    /// <returns>如果 value 在 min 和 max 之间，则返回 true；否则返回 false</returns>
    public static bool Between<T>(T value, T min, T max) where T : IComparable<T>
    {
        // 使用 CompareTo 方法比较 value、min 和 max 的大小关系
        // value 必须大于或等于 min，并且小于或等于 max
        // 这里可以根据实际业务场景需求调整
        if (max.CompareTo(min) >= 0)
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }
        else
        {
            return value.CompareTo(max) >= 0 && value.CompareTo(min) <= 0;
        }
    }
}