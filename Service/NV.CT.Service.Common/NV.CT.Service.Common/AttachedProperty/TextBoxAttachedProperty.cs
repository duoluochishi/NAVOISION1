using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InputType = NV.CT.Service.Common.Enums.InputType;

namespace NV.CT.Service.Common.AttachedProperty
{
    public static class TextBoxAttachedProperty
    {
        #region UpdateSourceWhenEnter

        public static readonly DependencyProperty UpdateSourceWhenEnterProperty 
            = DependencyProperty.RegisterAttached("UpdateSourceWhenEnter", typeof(bool), typeof(TextBoxAttachedProperty), new PropertyMetadata(false, OnUpdateSourceWhenEnterPropertyChanged));

        public static bool GetUpdateSourceWhenEnter(DependencyObject obj)
        {
            return (bool)obj.GetValue(UpdateSourceWhenEnterProperty);
        }

        public static void SetUpdateSourceWhenEnter(DependencyObject obj, bool value)
        {
            obj.SetValue(UpdateSourceWhenEnterProperty, value);
        }

        private static void OnUpdateSourceWhenEnterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox || e.NewValue is not bool isUpdate)
            {
                return;
            }

            if (isUpdate)
            {
                textBox.KeyDown += TextBox_KeyDown_UpdateSourceWhenEnter;
            }
            else
            {
                textBox.KeyDown -= TextBox_KeyDown_UpdateSourceWhenEnter;
            }
        }

        private static void TextBox_KeyDown_UpdateSourceWhenEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                textBox.Focus();
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        #endregion

        #region InputType

        public static readonly DependencyProperty InputTypeProperty = 
            DependencyProperty.RegisterAttached("InputType", typeof(InputType), typeof(TextBoxAttachedProperty), new PropertyMetadata(InputType.None, OnInputTypeChanged));

        public static InputType GetInputType(DependencyObject obj)
        {
            return (InputType)obj.GetValue(InputTypeProperty);
        }

        public static void SetInputType(DependencyObject obj, InputType value)
        {
            obj.SetValue(InputTypeProperty, value);
        }

        private static readonly Regex IntegerRegex = new(@"^-?0*\d+$");
        private static readonly Regex UnsignedIntegerRegex = new(@"^0*\d+$");
        private static readonly Regex DecimalRegex = new(@"^-?(0|([0-9][0-9]*))(\.[\d]*)?$");
        private static readonly Regex UnsignedDecimalRegex = new(@"^(0|([0-9][0-9]*))(\.[\d]*)?$");

        private static void OnInputTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox || e.OldValue is not InputType oldInputType || e.NewValue is not InputType newInputType)
            {
                return;
            }

            if (newInputType == InputType.None)
            {
                textBox.ClearValue(InputMethod.IsInputMethodEnabledProperty);
                textBox.LostFocus -= TextBox_LostFocus_InputType;
                textBox.PreviewTextInput -= TextBox_PreviewTextInput_InputType;
                DataObject.RemovePastingHandler(textBox, OnPaste_InputType);
                return;
            }

            if (oldInputType == InputType.None)
            {
                InputMethod.SetIsInputMethodEnabled(textBox, false); //禁止输入法，只允许输入字母、符号、数字
                textBox.LostFocus += TextBox_LostFocus_InputType;
                textBox.PreviewTextInput += TextBox_PreviewTextInput_InputType;
                DataObject.AddPastingHandler(textBox, OnPaste_InputType);
            }
        }

        private static void TextBox_LostFocus_InputType(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text.EndsWith('.'))
            {
                textBox.SetCurrentValue(TextBox.TextProperty, textBox.Text.TrimEnd('.'));
            }
        }

        private static void TextBox_PreviewTextInput_InputType(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                e.Handled = !IsValid(textBox, e.Text);
            }
        }

        private static void OnPaste_InputType(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text) && sender is TextBox textBox)
            {
                var text = Convert.ToString(e.DataObject.GetData(DataFormats.Text));

                if (string.IsNullOrWhiteSpace(text) || !IsValid(textBox, text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static bool IsValid(TextBox textBox, string text)
        {
            var completeText = textBox.Text.Remove(textBox.CaretIndex, textBox.SelectionLength).Insert(textBox.CaretIndex, text);

            return GetInputType(textBox) switch
            {
                InputType.None => true,
                InputType.Integer => IntegerRegex.IsMatch(completeText),
                InputType.UnsignedInteger => UnsignedIntegerRegex.IsMatch(completeText),
                InputType.Decimal => DecimalRegex.IsMatch(completeText),
                InputType.UnsignedDecimal => UnsignedDecimalRegex.IsMatch(completeText),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion
    }
}