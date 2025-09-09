using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.Service.Common.Framework
{
    public class ProxyData : Freezable
    {
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(ProxyData), new PropertyMetadata());

        protected override Freezable CreateInstanceCore()
        {
            return new ProxyData();
        }
    }
}