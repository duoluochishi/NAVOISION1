//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/07/08 16:35:59    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using System.Windows;


namespace NV.CT.UI.Controls.Panel
{
    /// <summary>
    /// Custom panel that supports resizability
    /// </summary>
    public class ResizablePanel : System.Windows.Controls.Panel
    {
        //可见调整大小的手柄区域
        public Rect ResizeHandle { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            // 测量子元素
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(new Point(), finalSize));
            }

            // 设置可见调整大小的手柄区域为：右下角10像素的面积
            ResizeHandle = new Rect(new Point(finalSize.Width - 10, finalSize.Height - 10), new Size(10, 10));
            return finalSize;
        }

    }
}
