using System.Collections.Concurrent;

namespace NV.CT.WorkingBoxThread.WB
{
    /// <summary>
    /// 线程封装对象。
    /// 封装一个命名线程，并通过push方法将指定的封装消息推送至当前线程中。
    /// 初始化后即开启，不会主动停止当前线程。
    /// </summary>
    public class WorkingBoxTask
    {
        public string Name { get; private set; }        //获取当前线程封装对象名。

        public int WorkingBoxId { get; private set; }            //获取当前线程封装对象ID。

        private Thread myThread;

        private readonly AutoResetEvent myResetEvent;

        private ConcurrentQueue<WorkingBoxMessage> myQueue;

        private Action<Exception>? ExceptionHandler;

        public WorkingBoxTask(string name)
        {
            Name = name;
            myQueue = new ConcurrentQueue<WorkingBoxMessage>();
            myResetEvent = new AutoResetEvent(false);
            Start();
        }

        private void Start()
        {
            lock (this)
            {
                if (myThread is null)
                {
                    ActivateThread();
                }
            }
        }

        private void ActivateThread()
        {
            myThread = new Thread(RunWbLoop);
            myThread.Name = Name;
            myThread.IsBackground = true;
            myThread.Start();

            WorkingBoxId = myThread.ManagedThreadId;
        }

        //设置异常处理方法。
        public void SetExceptionHanlder(Action<Exception> exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
        }

        public int GetCurrentWBQueueCount()
        {
            return myQueue.Count;
        }

        public void Push(WorkingBoxMessage wbMessage)
        {
            if (myThread is null)
            {
                Start();
            }

            myQueue.Enqueue(wbMessage);
            myResetEvent.Set();
        }

        private void RunWbLoop()
        {
            while (true)
            {
                while(myQueue.Count > 0)
                {
                    if(myQueue.TryDequeue(out var wbm))
                    {
                        if(wbm is not null)
                        {
                            try
                            {
                                wbm.ActionValue.Invoke();                   //执行队列方法。
                            }
                            catch (Exception ex)
                            {
                                ExceptionHandler?.Invoke(ex);               //Handle异常
                            }
                        }
                    }
                }

                myResetEvent.WaitOne();
            }
        }

    }
}
