//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/13 11:27:14       V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public sealed class WorkingBox
{
    private ConcurrentQueue<Action> _actions;
    private AutoResetEvent _autoReset = new AutoResetEvent(false);

    public WorkingBox()
    {
        _actions = new ConcurrentQueue<Action>();
        Task.Run(() => Handling());
    }


    public void Raise(Action action)
    {
        _actions.Enqueue(action);
        _autoReset.Set();
    }

    private void Handling()
    {
        while (true)
        {
            _autoReset.WaitOne();
            while (true)
            {
                Action? action;
                var isTry = _actions.TryDequeue(out action);
                if (!isTry) break;
                //TODO: 异常暂有事件调用方处理
                action?.Invoke();
            }
        }
    }
}
