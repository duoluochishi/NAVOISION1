using AspectInjector.Broker;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.WorkingBoxThread.WB;
namespace NV.CT.CommonAttribute.AOPAttribute
{
    /// </summary>
    /// 基于Aspect-Injector实现AOP，将方法的实际执行过程route到指定线程中。
    /// 由于框架限制，无法在运行时修改Attribute内容，且构造函数不接受参数，只能不同Workingbox使用不同的Attribute。
    /// 当前提供Main，Secondary两个
    /// </summary>
    [Aspect(Scope.Global)]
    [Injection(typeof(MainWorkingBoxAttribute))]
    public class MainWorkingBoxAttribute : Attribute
    {
        public string WorkingBoxName { get; private set; }

        public MainWorkingBoxAttribute()
        {
            WorkingBoxName = "MainWorkingBox";
        }

        [Advice(Kind.Around)]
        public object RouteAround([Argument(Source.Type)] Type type,
            [Argument(Source.Name)] string name,
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args)
        {
            Global.Logger?.LogDebug($"Try to route {target.Method.DeclaringType}.{name} from {Thread.CurrentThread.ManagedThreadId} to {WorkingBoxName}");

            WorkingBoxMessage message = new WorkingBoxMessage(
                () => {
                    Global.Logger?.LogDebug($"Perform {target.Method.DeclaringType}.{name} in {Thread.CurrentThread.ManagedThreadId}");
                    target.Invoke(args);
                }, WorkingBoxName, name
                );
            WorkingBox.Route(message);
            return null;
        }
    }
}
