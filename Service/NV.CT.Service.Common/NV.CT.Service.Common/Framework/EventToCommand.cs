using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.Service.Common.Framework
{
    public class EventToCommand
    {
        public static string GetEventName(DependencyObject obj)
        {
            return (string)obj.GetValue(EventNameProperty);
        }

        public static void SetEventName(DependencyObject obj, string value)
        {
            obj.SetValue(EventNameProperty, value);
        }

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.RegisterAttached("EventName", typeof(string), typeof(EventToCommand), new PropertyMetadata("", OnEventNameChanged));

        public static void OnEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(OnCommandChanged));


        public static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            EventInfo eventInfo = d.GetType().GetEvent(GetEventName(d));
            var eventHandlerMethodInfo = typeof(EventToCommand).GetMethod("Invoke", BindingFlags.Static | BindingFlags.Public);
            eventInfo.AddEventHandler(d, Delegate.CreateDelegate(eventInfo.EventHandlerType, eventHandlerMethodInfo));

        }
        public static void Invoke(object sender, EventArgs e)
        {
            var ele = sender as FrameworkElement;
            var command = GetCommand(ele);
            command.Execute(ele.DataContext);
        }
        #region command2 it is invoked even when the routed event is marked handled in its event data
        public static string GetEventName2(DependencyObject obj)
        {
            return (string)obj.GetValue(EventName2Property);
        }

        public static void SetEventName2(DependencyObject obj, string value)
        {
            obj.SetValue(EventName2Property, value);
        }

        public static readonly DependencyProperty EventName2Property =
            DependencyProperty.RegisterAttached("EventName2", typeof(string), typeof(EventToCommand), new PropertyMetadata(""));


        public static ICommand GetCommand2(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(Command2Property);
        }

        public static void SetCommand2(DependencyObject obj, ICommand value)
        {
            obj.SetValue(Command2Property, value);
        }

        public static readonly DependencyProperty Command2Property =
            DependencyProperty.RegisterAttached("Command2", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(OnCommandChanged2));

        public static void OnCommandChanged2(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            EventInfo eventInfo = d.GetType().GetEvent(GetEventName2(d));
            if (eventInfo == null) return;
            var eventHandlerMethodInfo = typeof(EventToCommand).GetMethod("Invoke2", BindingFlags.Static | BindingFlags.Public);
            var routeEvent = EventManager.GetRoutedEventsForOwner(element.GetType()).First(p => p.Name == eventInfo.Name);
            element.AddHandler(routeEvent, Delegate.CreateDelegate(eventInfo.EventHandlerType, eventHandlerMethodInfo), true);
        }
        public static void Invoke2(object sender, EventArgs e)
        {
            var command = GetCommand2(sender as FrameworkElement);
            command.Execute(e);
        }
        #endregion
    }
}
