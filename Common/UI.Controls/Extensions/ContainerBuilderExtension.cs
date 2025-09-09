using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.UI.Controls.Extensions
{
    public static class ContainerBuilderExtension
    {
        public static void AddUIControlContainer(this ContainerBuilder builder)
        {
            builder.RegisterModule<UIControlModule>();
        }
    }
}
