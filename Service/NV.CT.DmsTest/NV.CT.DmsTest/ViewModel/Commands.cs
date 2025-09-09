using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.DmsTest
{
    public static class Commands
    {
        public static RoutedUICommand TestGo = new RoutedUICommand();
        public static RoutedUICommand TestStop = new RoutedUICommand();



        public static bool GetCommandBinding(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowBorderProperty);
        }

        public static void SetCommandBinding(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowBorderProperty, value);
        }

        public static readonly DependencyProperty ShowBorderProperty =DependencyProperty.RegisterAttached("CommandBinding", typeof(CommandBinding), typeof(Commands), new PropertyMetadata(OnCommandBindingChanged));
        static void OnCommandBindingChanged(DependencyObject actionUI, DependencyPropertyChangedEventArgs args)
        {
            UIElement? element = actionUI as UIElement;
            CommandBinding? command = args.NewValue as CommandBinding;

            if (element != null && command != null) 
            {
                SetRoutedCommand(element, command.Command);
                element.CommandBindings.Add(command);
            }
        }

    static void SetRoutedCommand(UIElement element, ICommand command) 
    {
        System.Windows.Controls.Primitives.ButtonBase? buttonBase = element as System.Windows.Controls.Primitives.ButtonBase;
        if (buttonBase != null) 
        {
           buttonBase.Command = command;
           return;
        }

        System.Windows.Controls.MenuItem? menuItem = element as System.Windows.Controls.MenuItem;
        if (menuItem != null) 
        {
            menuItem.Command = command;
            return;
        }
    }

       //static  Window GetWindow(UIElement element) 
       // {
       //     Window window = null;
       //     if (element != null) 
       //     {
       //        window=window.Parent as Window;
       //     }

       //     if (window != null) 
       //     {
       //         return window;
       //     }
       //     else
       //     {
       //        window=GetWindow(element);
       //     }
       //     return window;
       // }
    }
}
