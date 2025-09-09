using AspectInjector.Broker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS;

namespace NV.CT.CommonAttribute.AOPAttribute
{
    [Aspect(Scope.Global)]
    [Injection(typeof(LogEntranceAttribute))]
    public class LogEntranceAttribute : Attribute    
    {
        public LogEntranceAttribute() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        [Advice(Kind.Before)]
        public void LogBefore([Argument(Source.Name)] string methodName,
            [Argument(Source.Type)] Type instanceType,
            [Argument(Source.Arguments)] object[] arguments)
        {
            Global.Logger?.LogInformation($"[{instanceType.Name}.{methodName}] Enterred with argument: ({JsonConvert.SerializeObject(arguments)})");
        }
        
        [Advice(Kind.After)]
        public void LogAfter([Argument(Source.Name)] string methodName,
            [Argument(Source.Type)] Type instanceType,
            [Argument(Source.ReturnValue)] object returnValue )
        {
            Global.Logger?.LogInformation($"[{instanceType.Name}.{methodName}] Exit with return value:{JsonConvert.SerializeObject(returnValue)}");
        }
    }
}
