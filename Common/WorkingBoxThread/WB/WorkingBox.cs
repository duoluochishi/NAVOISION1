
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using System.Collections.Concurrent;

namespace NV.CT.WorkingBoxThread.WB
{
    /// <summary>
    /// 线程封装接口，提供命名线程的创建、获取等方法。
    /// </summary>
    public class WorkingBox
    {
        public static int WarningQueueCount = 50;              //事件队列中堆积事件过多报警阈值。

        private static WorkingBox myInstance;

        private ConcurrentDictionary<string, WorkingBoxTask> WorkingBoxTasks;


        private static WorkingBox Instance
        {
            get
            {
                if(myInstance is null)
                {
                    myInstance = new WorkingBox();
                }
                return myInstance;
            }
        }

        private WorkingBox()
        {
            WorkingBoxTasks = new ConcurrentDictionary<string, WorkingBoxTask>();            
        }

        public static void Route(WorkingBoxMessage message)
        {
            var currentThreadId = System.Environment.CurrentManagedThreadId;
            var wbt = Get(message.WorkingBoxName);

            if (currentThreadId == wbt.WorkingBoxId)           //若当前线程即为目标route线程，直接执行，防止入列后导致死锁。
            {
                //todo: 添加log，报警提示当前行为。
                Global.Logger?.LogDebug("route to the same workingbox from itself is not expected.");
                message.ActionValue.Invoke();           //直接在当前线程执行，不入列
                return;
            }

            if(Instance.WorkingBoxTasks.Any(x=>x.Value.WorkingBoxId == currentThreadId))
            {
                Global.Logger?.LogDebug("warn to route from managed WB to another.");              //线程之间互相route，理论上可能出现线程冲突或死锁，warning
            }

            if(wbt.GetCurrentWBQueueCount() > WarningQueueCount)
            {
                Global.Logger?.LogDebug($"Warning: Too many message to be handled in queue: {wbt.GetCurrentWBQueueCount()}");    //队列中消息过多，warning
            }

            wbt.Push(message);
        }

        /// <summary>
        /// 创建指定Workingbox。无需调用，因为在Route时会检测是否有命名WorkingBoxTask创建。没有会自动创建。
        /// </summary>
        /// <param name="workingBoxName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static WorkingBoxTask Create(string workingBoxName)
        {
            if(Instance.WorkingBoxTasks.ContainsKey(workingBoxName))
            {
                throw new InvalidOperationException($"WorkingBoxName：{workingBoxName}重复");      //不可以相同命名重复创建WorkingBox
            }

            var value = new WorkingBoxTask(workingBoxName);
            value.SetExceptionHanlder(HandleWBException);
            Instance.WorkingBoxTasks[workingBoxName] = value;

            return Instance.WorkingBoxTasks[workingBoxName];
        }

        public static WorkingBoxTask Get(string workingBoxName)
        {
            if (Instance.WorkingBoxTasks.ContainsKey(workingBoxName))
            {
                return Instance.WorkingBoxTasks[workingBoxName];
            }
            return Create(workingBoxName);
        }

        private static void HandleWBException(Exception ex)
        {
            Global.Logger?.LogError(ex.ToString());
        }
    }
}
