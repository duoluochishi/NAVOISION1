using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompleteSolution
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            var root = Environment.GetEnvironmentVariable("NANO_ROOT");
#else
            var location = Assembly.GetEntryAssembly().Location;
            var root = Path.Combine(location.Substring(0, location.LastIndexOf("\\")), "..\\");
#endif
            var projectFullNames = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories).ToList();
            var projects = new List<(string, string, string, string)>();
            if (projectFullNames.Any())
            {
                foreach (var fullName in projectFullNames)
                {
                    var projectFile = fullName.Replace(root, "");
                    if (projectFile.Contains("CompilingManager\\") || projectFile.Contains("NonProduct")) continue;
                    var projectName = fullName.Substring(fullName.LastIndexOf("\\") + 1).Replace(".csproj", "");
                    var projectGuid = Guid.NewGuid().ToString().ToUpper();
                    var moduleName = projectFile.Substring(0, projectFile.IndexOf("\\"));
                    projects.Add((projectName, projectFile, projectGuid, moduleName));
                }
            }

            if (projects.Any(p => p.Item4 == "Skin"))
            {
                CreateSolution(root, "Skin", projects.Where(p => p.Item4 == "Skin").ToList());
            }

            if (projects.Any(p => p.Item4 != "Service" && p.Item4 != "Skin"))
            {
                CreateSolution(root, "MCS", projects.Where(p => p.Item4 != "Service" && p.Item4 != "Skin").ToList());
            }

            if (projects.Any(p => p.Item4 == "Service"))
            {
                CreateSolution(root, "Service", projects.Where(p => p.Item4 == "Service").ToList());
            }
            //Console.WriteLine("End.");
        }

        public static void CreateSolution(string path, string slnName, List<(string, string, string, string)> projects)
        {
            var sbSlnContent = new StringBuilder();
            var slnGuid = Guid.NewGuid().ToString().ToUpper();
            sbSlnContent.AppendLine();
            sbSlnContent.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            sbSlnContent.AppendLine("# Visual Studio Version 17");
            sbSlnContent.AppendLine("VisualStudioVersion = 17.2.32630.192");
            sbSlnContent.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
            foreach (var project in projects)
            {
                //Console.WriteLine(project);
                sbSlnContent.AppendLine($"Project(\"{{9A19103F-16F7-4668-BE54-9A1E7A4F7556}}\") = \"{project.Item1}\", \"{project.Item2}\", \"{{{project.Item3}}}\"");
                sbSlnContent.AppendLine("EndProject");
            }
            sbSlnContent.AppendLine("Global");
            sbSlnContent.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            sbSlnContent.AppendLine($"\t\tDebug|Any CPU = Debug|Any CPU");
            sbSlnContent.AppendLine($"\t\tRelease|Any CPU = Release|Any CPU");
            sbSlnContent.AppendLine("\tEndGlobalSection");
            sbSlnContent.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var project in projects)
            {
                sbSlnContent.AppendLine($"\t\t{{{project.Item3}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                sbSlnContent.AppendLine($"\t\t{{{project.Item3}}}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                sbSlnContent.AppendLine($"\t\t{{{project.Item3}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                sbSlnContent.AppendLine($"\t\t{{{project.Item3}}}.Release|Any CPU.Build.0 = Release|Any CPU");
            }
            sbSlnContent.AppendLine("\tEndGlobalSection");
            sbSlnContent.AppendLine("\tGlobalSection(ExtensibilityGlobals) = postSolution");
            sbSlnContent.AppendLine($"\t\tSolutionGuid = {{{slnGuid}}}");
            sbSlnContent.AppendLine("\tEndGlobalSection");
            sbSlnContent.AppendLine("EndGlobal");
            File.WriteAllText(Path.Combine(path, $"Complete{slnName}.sln"), sbSlnContent.ToString(), Encoding.UTF8);
        }
    }
}
