using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Formatting = Newtonsoft.Json.Formatting;

namespace ConfigurationTool
{
    public static class ConfigAssemblyReflectionHelper
    {
        /// <summary>
        /// Get all public static properties of Config assembly and serialize them to JSON
        /// </summary>
        /// <param name="assemblyPath">eg:The path of NV.MPS.Configuration.dll</param>
        /// <param name="formatting">JSON Formatting</param>
        /// <returns>JSON Content with Formatting</returns>
        public static string GetConfigurationAsJson(string assemblyPath, string typeName, Formatting formatting = Formatting.Indented)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                throw new ArgumentNullException(nameof(assemblyPath));
            ConfigAssemblyLoadContext loadContext = null;
            try
            {
                string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
                loadContext = new ConfigAssemblyLoadContext(assemblyDirectory);
                // 1. Load the assembly
                Assembly configAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);
                if (configAssembly == null)
                    throw new FileLoadException("无法加载配置程序集", assemblyPath);

                // 2. Get type
                Type configType = configAssembly.GetType(typeName);
                if (configType == null)
                    throw new TypeLoadException($"未找到{typeName}类型");

                // Validate if hte assembly is a static class
                if (!configType.IsAbstract || !configType.IsSealed)
                    throw new InvalidOperationException("找到的类型不是静态类");

                // 3. Get all public static properties
                PropertyInfo[] staticProperties = configType.GetProperties(
                    BindingFlags.Public | BindingFlags.Static);

                if (staticProperties.Length == 0)
                    throw new InvalidOperationException("未找到公共静态属性");

                // 4. Collect properties and values
                var configData = new Dictionary<string, object>();
                foreach (var property in staticProperties)
                {
                    // Get static property value（arg is null）
                    object value = property.GetValue(null);
                    configData[property.Name] = value;
                }

                // 5. Serialize
                return JsonConvert.SerializeObject(configData, formatting);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("GetConfigurationAsJson failed", ex);
            }
            finally
            {
                loadContext?.Unload();
            }
        }
    }

    public class ConfigAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly string _assemblyDirectory;

        public ConfigAssemblyLoadContext(string assemblyDirectory) : base(isCollectible: true)
        {
            _assemblyDirectory = assemblyDirectory;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = Path.Combine(_assemblyDirectory, assemblyName.Name + ".dll");

            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
