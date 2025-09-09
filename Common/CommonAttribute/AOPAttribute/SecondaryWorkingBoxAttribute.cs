using AspectInjector.Broker;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.WorkingBoxThread.WB;
namespace NV.CT.CommonAttribute.AOPAttribute
{
    /// </summary>
    /// 基于Aspect-Injector实现AOP，将方法的实际执行过程route到指定线程中。
    /// </summary>
    [Aspect(Scope.Global)]
    [Injection(typeof(SecondaryWorkingBoxAttribute))]
    public class SecondaryWorkingBoxAttribute : Attribute
    {
        public string WorkingBoxName { get; private set; }
        public SecondaryWorkingBoxAttribute()
        {
            WorkingBoxName = "SecondaryWorkingBox";
        }

        [Advice(Kind.Around)]
        public object RouteAround([Argument(Source.Type)] Type type,
            [Argument(Source.Name)] string name,
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args)
        {
            Global.Logger?.LogDebug($"Try to route {name} from {Thread.CurrentThread.ManagedThreadId} to {WorkingBoxName}");

            WorkingBoxMessage message = new WorkingBoxMessage(
                () => {
                    Global.Logger?.LogDebug($"Perform {name} in {Thread.CurrentThread.ManagedThreadId}");
                    target.Invoke(args);
                    }, WorkingBoxName,name
                );
            WorkingBox.Route(message);
            
            return null;
        }
    }
}
