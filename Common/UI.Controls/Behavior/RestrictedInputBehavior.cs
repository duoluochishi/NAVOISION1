using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace NV.CT.UI.Controls.Behavior
{
    public class RestrictedInputBehavior : Behavior<TextBox>
    {
        // 正则表达式：允许数字、字母、下划线和空格
        private static readonly Regex _allowedRegex = new Regex(@"^[\w\s+]*$", RegexOptions.Compiled);

        protected override void OnAttached()
        {
            base.OnAttached();
            // 禁用输入法，避免中文输入法干扰
            InputMethod.SetIsInputMethodEnabled(AssociatedObject, false);
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            DataObject.AddPastingHandler(AssociatedObject, OnPasting);
        }

        // 释放资源
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            DataObject.RemovePastingHandler(AssociatedObject, OnPasting);
        }

        // 拦截文本输入
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_allowedRegex.IsMatch(e.Text);
        }

        // 处理快捷键（允许空格）
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 允许退格、删除、空格键
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Space)
            {
                e.Handled = false;
                return;
            }
            // 阻止其他非文本键（如Ctrl+V）
            //e.Handled = true;
        }

        // 粘贴内容验证
        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                if (!_allowedRegex.IsMatch(text))
                {
                    e.CancelCommand();
                    // 可选：显示错误提示
                    //AssociatedObject.ToolTip = "";
                }
            }
        }
    }
}
