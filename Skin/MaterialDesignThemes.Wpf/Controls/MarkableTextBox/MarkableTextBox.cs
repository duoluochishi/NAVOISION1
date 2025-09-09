#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace MaterialDesignThemes.Wpf.Controls.MarkableTextBox;

public class MarkableTextBox : TextBox, IMarkableControlBase
{
    private readonly string IllegalCharacter = @"[`~!！￥@#$%？》【】《》……^&amp;*、|\——+=·。，&lt;&gt;?:""{},.\/;'[\]]";
    private Regex regexObj = new Regex(@"[^A-Za-z0-9]");
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PreviewKeyDown += TextBox_PreviewKeyDown;
        //PreviewTextInput += TextBox_PreviewTextInput;
        TextChanged += TextBox_TextChanged;
        LostFocus += TextBox_LostFocus;

        KeyDown += MarkableTextBox_KeyDown;
    }

    private void MarkableTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        var textBox = sender as TextBox;
        if (TextType == TextTypes.ALL || textBox == null)
            return;
        var txt = textBox.Text;
        if (TextType == TextTypes.Decimal)
        {
            //屏蔽非法按键
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal || e.Key == Key.Subtract || e.Key.ToString() == "Tab")
            {
                if (txt.Contains(".") && e.Key == Key.Decimal)
                {
                    e.Handled = true;
                    return;
                }
                else if ((txt.Contains("-") || this.CaretIndex != 0) && e.Key == Key.Subtract)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else if (((e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod || e.Key == Key.OemMinus) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                if (txt.Contains(".") && e.Key == Key.OemPeriod)
                {
                    e.Handled = true;
                    return;
                }
                else if ((txt.Contains("-") || this.CaretIndex != 0) && e.Key == Key.OemMinus)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                if (e.Key.ToString() != "RightCtrl")
                { }
            }
        }
        if (TextType == TextTypes.LetterAndNumber)
        {
            e.Handled = regexObj.IsMatch(txt);
        }
        base.OnKeyDown(e);
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox == null)
        {
            return;
        }

        if (TextType == TextTypes.Decimal)
        {
            if (textBox.Text == "-" || textBox.Text == "0.")
            {
                textBox.Clear();
                return;
            }
            if (textBox.Text.Length == 2)
            {
                string text = textBox.Text.Substring(textBox.Text.Length - 1);
                int count = text.Length - text.Replace(".", "").Length;
                if (count == 1)
                {
                    textBox.Clear();
                    return;
                }
            }

            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = Minimum.ToString(CultureInfo.InvariantCulture);
            }
            if (Convert.ToDouble(textBox.Text) > Maximum)
            {
                textBox.Text = Maximum.ToString(CultureInfo.InvariantCulture);
            }
            else if (Convert.ToDouble(textBox.Text) < Minimum)
            {
                textBox.Text = Minimum.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (TextType == TextTypes.ALL || textBox == null)
            return;

        TextChange[] changes = new TextChange[e.Changes.Count];

        e.Changes.CopyTo(changes, 0);
        // 引起Text改变的字符串的起点
        int offset = changes[0].Offset;
        // 引起Text改变的字符串的长度
        if (changes[0].AddedLength > 0)
        {
            if (textBox.Text.Contains("\\"))
            {
                textBox.Text = textBox.Text.Remove(offset, changes[0].AddedLength);
                return;
            }

            Regex? regex = null;
            if (TextType == TextTypes.Decimal)
            {
                if (textBox.Text == "-" || textBox.Text == "-0")
                {
                    return;
                }

                int count = textBox.Text.Count(c => c == '.');
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    string text = textBox.Text.Substring(textBox.Text.Length - 1);
                    int count2 = text.Length - text.Replace("-", "").Length;
                    if (count == 1 && textBox.Text != "-." && textBox.Text != "." && count2 == 0)
                    {
                        return;
                    }
                }

                regex = new Regex(@"^(?:-(?:[1-9](?:\d{0,2}(?:,\d{3})+|\d*))|(?:0|(?:[1-9](?:\d{0,2}(?:,\d{3})+|\d*))))(?:.\d+|)$");
            }
            else if (TextType == TextTypes.LetterAndNumber)
            {
                //regex = new Regex(@"^[A-Za-z0-9]+$");
                //regex = new Regex(@"[a-z0-9A-Z]+");
                regex = new Regex(@"^[A-Za-z0-9]+$");
            }
            else if (TextType == TextTypes.Text)
            {
                regex = new Regex(IllegalCharacter);
            }
            else if (TextType == TextTypes.IP)
            {
                regex = new Regex(@"((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}");
                int count = textBox.Text.Length - textBox.Text.Replace(".", "").Length;
                if (count < 4)
                {
                    return;
                }
            }

            bool isMatch = regex != null && regex.IsMatch(textBox.Text);
            if (TextType == TextTypes.Text)
            {
                if (isMatch)
                {
                    textBox.Text = textBox.Text.Remove(offset, changes[0].AddedLength);
                    // 控制光标位置，使其还能定位到变动前的位置
                    textBox.SelectionStart = offset;
                }
                else//检查空格
                {
                    textBox.Text = new Regex("[\\s]+").Replace(textBox.Text, " ");
                }
            }
            else if (!isMatch)
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    // 由于我们已经从键盘输入事件中屏蔽了非法字符；所以基本可以断定此非法输入是由于"粘贴"引起的
                    textBox.Text = textBox.Text.Remove(offset, changes[0].AddedLength);
                    // 控制光标位置，使其还能定位到变动前的位置
                    textBox.SelectionStart = offset;
                }
            }
        }
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (TextType == TextTypes.ALL)
        {
            return;
        }

        var textBox = sender as TextBox;
        if (textBox == null)
        {
            return;
        }

        Regex? regex;

        //char a = default(char);
        //if (char.TryParse(e.Text, out a))
        //{
        //    if ((int)a == 92)
        //    {
        //        e.Handled = true;
        //        return;
        //    }
        //}
        if (TextType == TextTypes.Decimal)
        {
            //正数、负数、和小数
            regex = new Regex(@"^(?:-(?:[1-9](?:\d{0,2}(?:,\d{3})+|\d*))|(?:0|(?:[1-9](?:\d{0,2}(?:,\d{3})+|\d*))))(?:.\d+|)$");

            if (e.Text == "-")
            {
                e.Handled = Minimum >= 0; //最小值大于0时，不允许输入负号
                return;
            }

            if (e.Text == ".")
            {
                e.Handled = false;
                return;
            }

            if (regex.IsMatch(e.Text))
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    if (double.TryParse(textBox.Text, out var maxValue))
                    {
                        if (maxValue > Maximum)
                        {
                            textBox.Text = Maximum.ToString(CultureInfo.InvariantCulture);
                            e.Handled = true;
                        }
                    }
                }
            }
            else
            {
                e.Handled = true;
            }
        }
        else if (TextType == TextTypes.LetterAndNumber)
        {
            // 匹配非英文字母和数字
            //  regex = new Regex(@"[a-z0-9A-Z]+");
            regex = new Regex(@"^[A-Za-z0-9]+$");
            //  regex = new Regex(@"^[A-Za-z0-9]+");
            // 根据匹配结果 设置Handled属性 (true--表示事件已处理，输入中断)
            bool isMatch = regex.IsMatch(e.Text);
            e.Handled = !isMatch;
        }
        else if (TextType == TextTypes.Text)
        {
            regex = new Regex(IllegalCharacter);
            bool isMatch = regex.IsMatch(e.Text);
            e.Handled = isMatch;
        }
        else if (TextType == TextTypes.IP)
        {
            //regex = new Regex(@"((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0- 4]\d)) |[0 - 1] ?\d{ 1,2})){ 3}");
            regex = new Regex(@"^[0-9]*$");
            if (e.Text == ".")
            {
                e.Handled = false;
                return;
            }
            bool isMatch = regex.IsMatch(e.Text);
            e.Handled = !isMatch;
        }
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (TextType == TextTypes.ALL && e.Key != Key.Enter)
        {
            return;
        }

        var textBox = sender as TextBox;
        if (textBox == null)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            if (TextType == TextTypes.Decimal)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = Minimum.ToString(CultureInfo.InvariantCulture);
                }
                else if (Convert.ToDouble(textBox.Text) > Maximum)
                {
                    textBox.Text = Maximum.ToString(CultureInfo.InvariantCulture);
                }
                else if (Convert.ToDouble(textBox.Text) < Minimum)
                {
                    textBox.Text = Minimum.ToString(CultureInfo.InvariantCulture);
                }
            }
            textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
            textBox.Focus();
        }
    }

    #region 控件状态

    public MarkControlStatus MarkStatus
    {
        get => (MarkControlStatus)GetValue(MarkStatusProperty);
        set => SetValue(MarkStatusProperty, value);
    }

    public static readonly DependencyProperty MarkStatusProperty =
        DependencyProperty.Register(nameof(MarkStatus), typeof(MarkControlStatus), typeof(MarkableTextBox), new PropertyMetadata(MarkControlStatus.Default));

    #endregion

    #region 文本类型

    public TextTypes TextType
    {
        get => (TextTypes)GetValue(TextTypeProperty);
        set => SetValue(TextTypeProperty, value);
    }

    public static readonly DependencyProperty TextTypeProperty =
        DependencyProperty.Register(nameof(TextType), typeof(TextTypes), typeof(MarkableTextBox), new PropertyMetadata(TextTypes.ALL, OnTextTypeChanged));

    private static void OnTextTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var textBox = d as MarkableTextBox;
        if (textBox == null)
        {
            return;
        }

        if (textBox.TextType == TextTypes.Text)
        {
            InputMethod.SetIsInputMethodEnabled(textBox, true);
        }
        else
        {
            InputMethod.SetIsInputMethodEnabled(textBox, false);
        }
    }

    #endregion

    #region 最大值

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(MarkableTextBox), new PropertyMetadata(100.0, OnMaximumChanged, CoerceMaximum), ArithmeticHelper.IsValidDoubleValue);

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (MarkableTextBox)d;
        ctl.CoerceValue(MinimumProperty);
    }

    private static object CoerceMaximum(DependencyObject d, object baseValue)
    {
        var minimum = ((MarkableTextBox)d).Minimum;
        return (double)baseValue < minimum ? minimum : baseValue;
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion

    #region 最小值

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(double), typeof(MarkableTextBox), new PropertyMetadata(default(double), OnMinimumChanged, CoerceMinimum), ArithmeticHelper.IsValidDoubleValue);

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (MarkableTextBox)d;
        ctl.CoerceValue(MaximumProperty);
    }

    private static object CoerceMinimum(DependencyObject d, object baseValue)
    {
        var maximum = ((MarkableTextBox)d).Maximum;
        return (double)baseValue > maximum ? maximum : baseValue;
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    #endregion
}