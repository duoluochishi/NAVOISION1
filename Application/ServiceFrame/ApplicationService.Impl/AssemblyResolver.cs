
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using System.Reflection;
using System.Runtime.Loader;

namespace NV.CT.ServiceFrame.ApplicationService.Impl;

internal sealed class AssemblyResolver : IDisposable
{
    private readonly ICompilationAssemblyResolver assemblyResolver;
    private readonly DependencyContext dependencyContext;
    private readonly AssemblyLoadContext loadContext;

    public AssemblyResolver(string path)
    {
        this.Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        this.dependencyContext = DependencyContext.Load(this.Assembly);

        this.assemblyResolver = new CompositeCompilationAssemblyResolver
                                (new ICompilationAssemblyResolver[]
        {
        new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
        new ReferenceAssemblyPathResolver(),
        new PackageCompilationAssemblyResolver()
        });

        this.loadContext = AssemblyLoadContext.GetLoadContext(this.Assembly);

        ResolveDependency(this.Assembly);//加载运行时所依赖的dll

        this.loadContext.Resolving += OnResolving;
    }

    public Assembly Assembly { get; }

    public void Dispose()
    {
        this.loadContext.Resolving -= this.OnResolving;
    }

    private void ResolveDependency(Assembly hostAssembly)
    {
        AssemblyName name = hostAssembly.GetName();
        foreach (var library in this.dependencyContext.RuntimeLibraries)
        {
            if (library == null)
            {
                continue;
            }

            var wrapper = new CompilationLibrary(
                library.Type,
                library.Name,
                library.Version,
                library.Hash,
                library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                library.Dependencies,
                library.Serviceable);

            var assemblies = new List<string>();
            this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
            if (assemblies.Count < 1)
            {
                continue;
            }
            string assemblyPath = assemblies[0];
            this.loadContext.LoadFromAssemblyPath(assemblyPath);
        }
    }

    private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
    {
        bool NamesMatch(RuntimeLibrary runtime)
        {
            return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
        }

        RuntimeLibrary library =
            this.dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
        if (library != null)
        {
            var wrapper = new CompilationLibrary(
                library.Type,
                library.Name,
                library.Version,
                library.Hash,
                library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                library.Dependencies,
                library.Serviceable);

            var assemblies = new List<string>();
            this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
            if (assemblies.Count > 0)
            {
                return this.loadContext.LoadFromAssemblyPath(assemblies[0]);
            }
        }

        return null;
    }
}
