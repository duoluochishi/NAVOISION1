using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.DependencyInject
{
    public class ServiceLocator
    {
        private static Lazy<ServiceLocator> _instance = new Lazy<ServiceLocator>(() => new ServiceLocator());
        public static ServiceLocator Instance => _instance.Value;
        private Container _container;

        private ServiceLocator()
        {
            _container = new Container();
        }

        public void AddSingleton<T>()
        {
            _container.AddSingleton<T>();
        }

        public void AddSingleton<I, T>()
        {
            _container.AddSingleton<I, T>();
        }

        public void AddSingleton<T>(T instance)
        {
            _container.AddSingleton<T>(instance);
        }

        public void AddSingleton<T>(Func<object> factory)
        {
            _container.AddSingleton<T>(factory);
        }

        public T GetService<T>()
        {
            return _container.GetService<T>();
        }
    }
}