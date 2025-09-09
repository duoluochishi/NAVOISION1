using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Controls.Extension;
using System;

namespace NV.CT.Service.Common.Controls
{
    class IocContainer
    {
        private static Lazy<IocContainer> _instance = new Lazy<IocContainer>(() => new IocContainer());
        public static IocContainer Instance { get => _instance.Value; }
        public IServiceProvider Services { get; private set; }

        // 私有构造函数，防止外部实例化
        private IocContainer()
        {
            Init();
        }

        private readonly object _locker = new();
        private bool _firstInit = true;

        /// <summary>
        /// 初始化代理
        /// </summary>
        /// <param name="info"></param>
        /// <param name="cfg"></param>
        private void Init()
        {
            if (!_firstInit)
            {
                return;
            }

            lock (_locker)
            {
                if (!_firstInit)
                {
                    LogService.Instance.Info(ServiceCategory.Common, $"IocContainer has initialized, can not be initialized again.");
                    return;
                }

                /** 初始化IOC **/
                ConfigureServices();
                /** 记录 **/
                LogService.Instance.Info(ServiceCategory.Common, $"IocContainer initialized successfully.");


                _firstInit = false;
            }
        }

        private void ConfigureServices()
        {
            /** 注入Services **/
            Services = Configure();
        }

        private IServiceProvider Configure()
        {
            ServiceCollection services = new ServiceCollection();

            /** Models & Viewmodels **/
            services.AddModelToViewModelMappings();
            /** Services **/
            services.AddServices();
            /** Configuration **/
            services.AddConfiguration();

            return services.BuildServiceProvider();
        }
    }
}
