using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.DependencyInject
{
    public class TypeDescription
    {
        public Lifetime Lifetime { get; private set; }
        public Type From { get; private set; }
        public Type To { get; private set; }
        public Func<object> Factory { get; private set; }

        public TypeDescription(Lifetime lifetime, Type to) : this(lifetime, to, to)
        {
        }

        public TypeDescription(Lifetime lifetime, Type from, Type to) : this(lifetime, from, to, null)
        {
        }

        public TypeDescription(Lifetime lifetime, Type from, Type to, Func<object> factory)
        {
            Lifetime = lifetime;
            From = from;
            To = to;
            Factory = factory;
        }

        public bool SameType(Type from)
        {
            return From == from;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not TypeDescription) return false;
            var td = obj as TypeDescription;
            return From == td.From && To == td.To;
        }

        public override int GetHashCode()
        {
            return From.GetHashCode() + To.GetHashCode() + Lifetime.GetHashCode();
        }
    }
}