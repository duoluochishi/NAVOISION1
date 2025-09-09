namespace NV.CT.RGT.Extensions;

public static class ContainerBuilderExtension
{
    public static void AddRgtAppContainer(this ContainerBuilder builder)
    {
        var app = $"{nameof(NV)}.{nameof(CT)}.{nameof(RGT)}";

        var layoutNamespace = $"{app}.{nameof(Layout)}";
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .PublicOnly()
        .Where(t => t.FullName != null &&
                    t.FullName.StartsWith(layoutNamespace) && t.BaseType == typeof(UserControl)).SingleInstance();

        ////ViewModel都是 transient 的
        //var viewModelNamespace = $"{app}.{nameof(ViewModel)}";
        //builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        //    .PublicOnly()
        //    .Where(t => t.FullName != null &&
        //                t.FullName.StartsWith(viewModelNamespace) && t.BaseType == typeof(BaseViewModel)).SingleInstance();
    }
}