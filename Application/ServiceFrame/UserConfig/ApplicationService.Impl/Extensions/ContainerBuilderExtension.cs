using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.UserConfig.ApplicationService.Impl.Extensions
{
    public static class ContainerBuilderExtension
    {
        public static void AddApplicationServiceContainer(this ContainerBuilder builder)
        {
            builder.RegisterModule<ApplicationServiceModule>();
        }
    }
}
