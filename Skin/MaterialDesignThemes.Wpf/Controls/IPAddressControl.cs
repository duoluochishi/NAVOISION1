using MT = MaterialDesignThemes.Wpf.Controls.MarkableTextBox;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MaterialDesignThemes.Wpf.Controls
{
    [TemplatePart(Name = "PART_TextBox1", Type = typeof(MT.MarkableTextBox))]
    [TemplatePart(Name = "PART_TextBox2", Type = typeof(MT.MarkableTextBox))]
    [TemplatePart(Name = "PART_TextBox3", Type = typeof(MT.MarkableTextBox))]
    [TemplatePart(Name = "PART_TextBox4", Type = typeof(MT.MarkableTextBox))]
    public class IPAddressControl : Control
    {
        private string _pendingIPAddress = string.Empty;

        static IPAddressControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IPAddressControl), new FrameworkPropertyMetadata(typeof(IPAddressControl)));
        }

        #region 依赖属性：IPAddress
        public string IPAddress
        {
            get { return (string)GetValue(IPAddressProperty); }
            set { SetValue(IPAddressProperty, value); }
        }

        public static readonly DependencyProperty IPAddressProperty =
            DependencyProperty.Register("IPAddress", typeof(string), typeof(IPAddressControl),
                new PropertyMetadata("0.0.0.0", OnIPAddressChanged));

        private static void OnIPAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IPAddressControl control && e.NewValue is string ip)
            {
                if (control.TextBox1 == null)
                {
                    // TextBox 尚未初始化，缓存 IP 值
                    control._pendingIPAddress = ip;
                }
                else
                {
                    // TextBox 已初始化，直接设置
                    control.SetIPToTextBox(ip);
                }
            }
        }

        /// <summary>
        /// 支持默认值为 4 段的IPv4地址
        /// </summary>
        /// <param name="ip"></param>
        private void SetIPToTextBox(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                if (TextBox1 != null) TextBox1.Text = string.Empty;
                if (TextBox2 != null) TextBox2.Text = string.Empty;
                if (TextBox3 != null) TextBox3.Text = string.Empty;
                if (TextBox4 != null) TextBox4.Text = string.Empty;
                return;
            }

            var segments = ip.Split('.');
            if (segments.Length == 4)
            {
                if (TextBox1 != null)
                {
                    string validatedText = ValidateSegment(segments[0]);
                    if (TextBox1.Text != validatedText)
                        TextBox1.Text = validatedText;
                }
                if (TextBox2 != null)
                {
                    string validatedText = ValidateSegment(segments[1]);
                    if (TextBox2.Text != validatedText)
                        TextBox2.Text = validatedText;
                }
                if (TextBox3 != null)
                {
                    string validatedText = ValidateSegment(segments[2]);
                    if (TextBox3.Text != validatedText)
                        TextBox3.Text = validatedText;
                }
                if (TextBox4 != null)
                {
                    string validatedText = ValidateSegment(segments[3]);
                    if (TextBox4.Text != validatedText)
                        TextBox4.Text = validatedText;
                }
            }
        }
        #endregion

        internal MT.MarkableTextBox TextBox1 { get; set; }
        internal MT.MarkableTextBox TextBox2 { get; set; }
        internal MT.MarkableTextBox TextBox3 { get; set; }
        internal MT.MarkableTextBox TextBox4 { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox1 = GetTemplateChild("PART_TextBox1") as MT.MarkableTextBox;
            TextBox2 = GetTemplateChild("PART_TextBox2") as MT.MarkableTextBox;
            TextBox3 = GetTemplateChild("PART_TextBox3") as MT.MarkableTextBox;
            TextBox4 = GetTemplateChild("PART_TextBox4") as MT.MarkableTextBox;

            // 如果之前有缓存的 IP 地址，现在设置
            if (!string.IsNullOrEmpty(_pendingIPAddress))
            {
                SetIPToTextBox(_pendingIPAddress);
                _pendingIPAddress = string.Empty;
            }

            UnsubscribeEvents(); // 防止重复订阅
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (TextBox1 != null)
            {
                TextBox1.PreviewTextInput += TextBox_PreviewTextInput;
                TextBox1.PreviewKeyDown += TextBox_PreviewKeyDown;
                TextBox1.LostFocus += TextBox_LostFocus;
                TextBox1.TextChanged += TextBox_TextChanged;
                DataObject.AddPastingHandler(TextBox1, TextBox_Pasting);
            }
            if (TextBox2 != null)
            {
                TextBox2.PreviewTextInput += TextBox_PreviewTextInput;
                TextBox2.PreviewKeyDown += TextBox_PreviewKeyDown;
                TextBox2.LostFocus += TextBox_LostFocus;
                TextBox2.TextChanged += TextBox_TextChanged;
                DataObject.AddPastingHandler(TextBox2, TextBox_Pasting);
            }
            if (TextBox3 != null)
            {
                TextBox3.PreviewTextInput += TextBox_PreviewTextInput;
                TextBox3.PreviewKeyDown += TextBox_PreviewKeyDown;
                TextBox3.LostFocus += TextBox_LostFocus;
                TextBox3.TextChanged += TextBox_TextChanged;
                DataObject.AddPastingHandler(TextBox3, TextBox_Pasting);
            }
            if (TextBox4 != null)
            {
                TextBox4.PreviewTextInput += TextBox_PreviewTextInput;
                TextBox4.PreviewKeyDown += TextBox_PreviewKeyDown;
                TextBox4.LostFocus += TextBox_LostFocus;
                TextBox4.TextChanged += TextBox_TextChanged;
                DataObject.AddPastingHandler(TextBox4, TextBox_Pasting);
            }
        }

        private void UnsubscribeEvents()
        {
            if (TextBox1 != null)
            {
                TextBox1.PreviewTextInput -= TextBox_PreviewTextInput;
                TextBox1.PreviewKeyDown -= TextBox_PreviewKeyDown;
                TextBox1.LostFocus -= TextBox_LostFocus;
                TextBox1.TextChanged -= TextBox_TextChanged;
                DataObject.RemovePastingHandler(TextBox1, TextBox_Pasting);
            }
            if (TextBox2 != null)
            {
                TextBox2.PreviewTextInput -= TextBox_PreviewTextInput;
                TextBox2.PreviewKeyDown -= TextBox_PreviewKeyDown;
                TextBox2.LostFocus -= TextBox_LostFocus;
                TextBox2.TextChanged -= TextBox_TextChanged;
                DataObject.RemovePastingHandler(TextBox2, TextBox_Pasting);
            }
            if (TextBox3 != null)
            {
                TextBox3.PreviewTextInput -= TextBox_PreviewTextInput;
                TextBox3.PreviewKeyDown -= TextBox_PreviewKeyDown;
                TextBox3.LostFocus -= TextBox_LostFocus;
                TextBox3.TextChanged -= TextBox_TextChanged;
                DataObject.RemovePastingHandler(TextBox3, TextBox_Pasting);
            }
            if (TextBox4 != null)
            {
                TextBox4.PreviewTextInput -= TextBox_PreviewTextInput;
                TextBox4.PreviewKeyDown -= TextBox_PreviewKeyDown;
                TextBox4.LostFocus -= TextBox_LostFocus;
                TextBox4.TextChanged -= TextBox_TextChanged;
                DataObject.RemovePastingHandler(TextBox4, TextBox_Pasting);
            }
        }

        #region 输入校验（input、Backspace、Paste）
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as MT.MarkableTextBox;
            if (textBox == null)
            {
                e.Handled = true;
                return;
            }

            string input = e.Text;

            if (input == ".")
            {
                // 如果当前 TextBox 有 1 或 2 位数字，输入 '.' 时跳转
                if (textBox.Text.Length >= 1 && textBox.Text.Length <= 2)
                {
                    MoveToNextTextBox(textBox);
                    e.Handled = true;
                }
                else
                {
                    e.Handled = true; // 其他情况（如空、3位）不允许输入 .
                }
            }
            else if (char.IsDigit(input, 0))
            {
                // 限制最多输入 3 位数字
                if (textBox.Text.Length >= 3 && !textBox.IsSelectionActive)
                {
                    e.Handled = true;
                }
            }
            else
            {
                // 非数字字符，禁止输入
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as MT.MarkableTextBox;
            if (textBox == null) return;

            if (e.Key == Key.Back)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    MoveToPreviousTextBox(textBox);
                    e.Handled = true;
                }
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as MT.MarkableTextBox;
            if (textBox == null || !e.DataObject.ContainsText())
            {
                e.CancelCommand();
                return;
            }

            var pastedText = e.DataObject.GetText();
            if (string.IsNullOrEmpty(pastedText))
            {
                e.CancelCommand();
                return;
            }

            // 处理粘贴完整 IP 地址的情况（如 "192.168.1.1"）
            if (pastedText.Contains("."))
            {
                var segments = pastedText.Split('.');
                if (segments.Length == 4)
                {
                    // 分别设置 4 个 MarkableTextBox 的值
                    TextBox1.Text = ValidateSegment(segments[0]);
                    TextBox2.Text = ValidateSegment(segments[1]);
                    TextBox3.Text = ValidateSegment(segments[2]);
                    TextBox4.Text = ValidateSegment(segments[3]);
                    UpdateIPAddress();
                    e.CancelCommand(); // 取消原始粘贴操作
                    return;
                }
            }

            // 处理粘贴纯数字的情况
            if (Regex.IsMatch(pastedText, "^\\d+$"))
            {
                // 计算粘贴后的总长度
                var newLength = textBox.Text.Length + pastedText.Length;
                if (newLength > 3)
                {
                    // 截断过长的部分
                    pastedText = pastedText.Substring(0, 3 - textBox.Text.Length);
                }

                // 替换当前 TextBox 内容
                var newText = textBox.Text.Insert(textBox.CaretIndex, pastedText);
                textBox.Text = newText;
                textBox.CaretIndex = newText.Length;

                // 如果填满，自动跳到下一个 TextBox
                if (newText.Length == 3)
                {
                    MoveToNextTextBox(textBox);
                }

                e.CancelCommand(); // 取消原始粘贴操作
            }
            else
            {
                e.CancelCommand(); // 非法内容，取消粘贴
            }
        }

        private string ValidateSegment(string segment)
        {
            if (int.TryParse(segment, out int value))
            {
                return Math.Max(0, Math.Min(255, value)).ToString();
            }
            return "0";
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as MT.MarkableTextBox;
            if (textBox != null && textBox.Text.Length == 3)
            {
                e.Handled = true;
                MoveToNextTextBox(textBox);
            }
        }
        #endregion

        #region 失去焦点时自动补全/修正

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as MT.MarkableTextBox;
            if (textBox == null) return;
            UpdateIPAddress();
        }
        #endregion

        #region 辅助方法

        private void MoveToPreviousTextBox(MT.MarkableTextBox current)
        {
            if (current == TextBox2) MoveTextBox(TextBox1);
            else if (current == TextBox3) MoveTextBox(TextBox2);
            else if (current == TextBox4) MoveTextBox(TextBox3);
        }

        private void MoveToNextTextBox(MT.MarkableTextBox current)
        {
            if (current == TextBox1) MoveTextBox(TextBox2);
            else if (current == TextBox2) MoveTextBox(TextBox3);
            else if (current == TextBox3) MoveTextBox(TextBox4);
        }

        private void MoveTextBox(MT.MarkableTextBox textBox)
        {
            if (textBox != null)
            {
                textBox.Focus();
                if (textBox.Text.Length > 0)
                    textBox.CaretIndex = textBox.Text.Length;
            }
        }

        private void UpdateIPAddress()
        {
            var newIP = $"{TextBox1?.Text ?? "0"}.{TextBox2?.Text ?? "0"}.{TextBox3?.Text ?? "0"}.{TextBox4?.Text ?? "0"}";
            if (newIP != IPAddress)
            {
                IPAddress = newIP;
            }
        }
        #endregion
    }

    internal static class ClipboardHelper
    {
        public static bool ContainsText(this IDataObject dataObject)
        {
            return dataObject.GetDataPresent(DataFormats.Text);
        }

        public static string GetText(this IDataObject dataObject)
        {
            return dataObject.GetData(DataFormats.Text) as string;
        }
    }
}
