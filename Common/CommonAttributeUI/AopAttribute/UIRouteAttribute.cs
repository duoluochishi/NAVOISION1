using System.Diagnostics;
using AspectInjector.Broker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS;
using System.Windows;

namespace NV.CT.CommonAttributeUI.AOPAttribute
{

    [Aspect(Scope.Global)]
    [Injection(typeof(UIRouteAttribute))]
    public class UIRouteAttribute : Attribute
    {
        public UIRouteAttribute() { }

        [Advice(Kind.Around)]
        public object RouteAround([Argument(Source.Type)] Type type,
            [Argument(Source.Name)] string name,
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args)
        {
            var sw = Stopwatch.StartNew();
            var profile = false;
            try
            {
                if (Application.Current is null)
                {
                    Global.Logger?.LogDebug($"No UI Current Application exist, perform the action{target.Method.Name}");
                    target.Invoke(args);
                }
                else if (Application.Current?.Dispatcher.Thread.ManagedThreadId ==
                         System.Environment.CurrentManagedThreadId)
                {
                    profile = true;
                    target.Invoke(args);
                }
                else
                {
                    profile = true;
                    //Global.Logger?.LogInformation($"{target.Method.DeclaringType}.{target.Method.Name}");
                    Global.Logger?.LogDebug(
                        $"Route {target.Method.DeclaringType}.{name} from {Thread.CurrentThread.ManagedThreadId} to UI Main Thread");
                    Application.Current?.Dispatcher?.Invoke(
                        () => target.Invoke(args)
                    );
                }
            }
            finally
            {
                if (profile)
                {
                    sw.Stop();

                    /*
                     * UI操作                         16ms     超过会直接掉帧
                     * 普通viewmodel调用               50ms    用户不会感觉到问题
                     * 后台加载,IO响应但会通过UI更新    100ms    超过就需要用async/await
                     * 调用比较重的服务或CPU密集操作    >200ms   需要后台线程或Task.Run
                     */
                    if (sw.ElapsedMilliseconds >= 200)
                    {
                        Global.Logger?.LogWarning(
                            $"[UIRoute][1] slow execution : {target.Method.DeclaringType}.{name} took {sw.ElapsedMilliseconds} ms");
                    }
                    else if (sw.ElapsedMilliseconds is >= 100 and < 200)
                    {
                        Global.Logger?.LogWarning(
                            $"[UIRoute][2] need to care : {target.Method.DeclaringType}.{name} took {sw.ElapsedMilliseconds} ms");
                    }
                    else if (sw.ElapsedMilliseconds is >= 50 and < 100)
                    {
                        Global.Logger?.LogWarning(
                            $"[UIRoute][3] user sensitive delay : {target.Method.DeclaringType}.{name} took {sw.ElapsedMilliseconds} ms");
                    }
                }
            }

            return null;
        }
    }
}
