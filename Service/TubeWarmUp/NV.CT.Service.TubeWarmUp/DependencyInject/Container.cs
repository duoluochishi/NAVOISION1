using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.DependencyInject
{
    public enum Lifetime
    {
        Singleton,
        Transient
    }

    public class Container
    {
        private List<TypeDescription> _typeDescriptions;
        private Dictionary<Type, object> _typeInstances;

        public Container()
        {
            _typeDescriptions = new List<TypeDescription>();
            _typeInstances = new Dictionary<Type, object>();
        }

        public void AddSingleton<I, T>()
        {
            var td = new TypeDescription(Lifetime.Singleton, typeof(I), typeof(T));
            _typeDescriptions.Add(td);
        }

        public void AddSingleton<T>()
        {
            var td = new TypeDescription(Lifetime.Singleton, typeof(T), typeof(T));
            _typeDescriptions.Add(td);
        }

        public void AddSingleton<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("Can not be null");
            }
            var td = new TypeDescription(Lifetime.Singleton, typeof(T), typeof(T), () => (object)instance);
        }

        public void AddSingleton<T>(Func<object> factory)
        {
            var td = new TypeDescription(Lifetime.Singleton, typeof(T), typeof(T), factory);
            _typeDescriptions.Add(td);
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        private object GetService(Type serviceType)
        {
            try
            {
                var td = _typeDescriptions.First(p => p.SameType(serviceType));
                if (td.Lifetime == Lifetime.Singleton)
                {
                    if (_typeInstances.ContainsKey(serviceType))
                    {
                        return _typeInstances[serviceType];
                    }
                    else
                    {
                        var fromType = td.From;
                        var destType = td.To;
                        object instance = null;
                        if (td.Factory != null)
                        {
                            instance = td.Factory();
                        }
                        else
                        {
                            var constructors = destType.GetConstructors();
                            if (constructors.Length != 1)
                            {
                                throw new InvalidOperationException($"count of {destType} must be 1, but it is {constructors.Length}");
                            }
                            var constructorInfo = constructors[0];
                            var paraInfos = constructorInfo.GetParameters();
                            object[] paras = new object[paraInfos.Length];
                            for (int i = 0; i < paraInfos.Length; i++)
                            {
                                //未处理数组，集合等类型
                                paras[i] = GetService(paraInfos[i].ParameterType);
                            }
                            instance = Activator.CreateInstance(destType, paras);
                            //var instance = constructors[0].Invoke(paraInfos);
                        }
                        _typeInstances.Add(fromType, instance);
                        return instance;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"Container not register {serviceType}");
            }
            return null;
        }
    }
}