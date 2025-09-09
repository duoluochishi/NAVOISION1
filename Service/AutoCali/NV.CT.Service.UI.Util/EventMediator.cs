using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.UI.Util
{
    /// <summary>
    /// 泛型事件参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TEventArgs<T> : EventArgs
    {
        public T Value { get; set; }
    }
    /// <summary>
    /// 事件中介，避免不同对象之间因为消息事件被迫耦合
    /// </summary>
    public static class EventMediator
    {
        private static IDictionary<string, EventHandler> events = new Dictionary<string, EventHandler>();

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="key">事件名称</param>
        /// <param name="aEvent">事件响应</param>
        public static void Register(string key, EventHandler<EventArgs> aEvent)
        {
            if (events.ContainsKey(key))
            {
                Console.WriteLine($"{key}事件已注册，更新注册响应");//TODO:LOG
                events[key] = new EventHandler(aEvent);//更新为最后一次注册的
                return;
            }

            events.Add(key, new EventHandler(aEvent));
        }

        /// <summary>
        /// 卸载注册事件
        /// </summary>
        /// <param name="key">事件名称</param>
        public static void UnRegister(string key)
        {
            if (!events.Remove(key))
            {
                Console.WriteLine($"{key}事件删除失败");//TODO:LOG
                return;
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="key">事件名称</param>
        /// <param name="eventArgs">事件参数</param>
        public static void Raise(string key, EventArgs eventArgs)
        {
            if (!events.TryGetValue(key, out var handler) || null == handler)
            {
                Console.WriteLine($"无法触发未注册的事件：{key}！");//TODO:LOG
                return;
            }

            handler(null, eventArgs);
        }
    }
}
