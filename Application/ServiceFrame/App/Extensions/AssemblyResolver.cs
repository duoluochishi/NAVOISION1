using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.DependencyModel;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System;
using System.IO;
using System.Linq;

namespace NV.CT.ServiceFrame.Extensions;

public class AssemblyResolver
{
    private readonly ICompilationAssemblyResolver assemblyResolver;
    private readonly DependencyContext dependencyContext;
    private readonly AssemblyLoadContext loadContext;

    public AssemblyResolver(string path)
    {
        Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        dependencyContext = DependencyContext.Load(Assembly);

        assemblyResolver = new CompositeCompilationAssemblyResolver(
            new ICompilationAssemblyResolver[] {
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                new ReferenceAssemblyPathResolver(),
                new PackageCompilationAssemblyResolver()
            });

        loadContext = AssemblyLoadContext.GetLoadContext(this.Assembly);

        loadContext.Resolving += OnResolving;
    }

    public Assembly Assembly { get; }

    public void Dispose()
    {
        loadContext.Resolving -= this.OnResolving;
    }

    private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
    {
        bool NamesMatch(RuntimeLibrary runtime)
        {
            return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
        }

        RuntimeLibrary library = dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
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
            assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
            if (assemblies.Count > 0)
            {
                return this.loadContext.LoadFromAssemblyPath(assemblies[0]);
            }
        }

        return null;
    }
}
