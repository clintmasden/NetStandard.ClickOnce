using ExtensionMethods;
using MageUINetStandard.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace MageUINetStandard
{
    internal class ManifestRoutines
    {
        private enum DependencyType
        {
            install,
            preRequisite,
        };

        private Assembly _appAssembly = Assembly.GetExecutingAssembly();

        public void UpdateManifest(string solutionManifest)
        {
            string projectManifest = File.ReadAllText(solutionManifest);
            var projectDependencies = GetDependenciesIdentifiers(projectManifest).Where(d => d.Type == DependencyType.preRequisite.ToString()).ToList();

            string standardNetManifest = string.Empty;

            using (Stream stream = _appAssembly.GetManifestResourceStream("MageUINetStandard.Files.standard.manifest"))
            using (StreamReader reader = new StreamReader(stream))
            {
                standardNetManifest = reader.ReadToEnd();
            }

            var standardNetDependencies = GetDependenciesIdentifiers(standardNetManifest).Where(d => d.Type == DependencyType.install.ToString()).ToList(); ;
            projectDependencies.StandardDependencies(standardNetDependencies);

            string projectUpdateManifest = UpdateManifestDependencies(projectManifest, projectDependencies);

            string manifestOriginal = $"{solutionManifest}-original";
            File.Copy(solutionManifest, manifestOriginal, true);

            File.WriteAllText(solutionManifest, projectUpdateManifest);
        }

        public void CopyNetStandardDlls(string publishDirectory)
        {
            List<string> resourceNames = _appAssembly.GetManifestResourceNames()
                .Where(r => r.ToLower().EndsWith(".dll.deploy")).ToList();

            foreach (string resourceName in resourceNames)
            {
                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    //Embedded Resource Namespace Fix
                    string fileNameWithOutNamespace = resourceName.ToLower().Replace("mageuinetstandard.files.dlls.", "");
                    string filePath = Path.Combine(publishDirectory, fileNameWithOutNamespace);

                    using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }
            }
        }

        public void UpdateCertificates(string certifcatePath, string solutionManifest, string applicationManifest)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "mageui.exe");

            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("MageUINetStandard.Files.mageui.exe"))
            {
                using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(@"mageui.exe");
            //startInfo.WindowStyle = ProcessWindowStyle.Minimized;

            //Internal error, please try again. Index was outside the bounds of the array.
            //startInfo.Arguments = $"-Sign {solutionManifest} - CertFile {certifcatePath} - Password";

            //Error, not signing properly
            //startInfo.Arguments = $"\"{solutionManifest}\"";
            //Process.Start(startInfo).WaitForExit();

            //Select Solution Manifest on Application Reference Screen
            //MessageBox.Show("Select Solution Manifest on Application Reference Screen");
            //startInfo.Arguments = $"\"{applicationManifest}\"";

            Process.Start(startInfo).WaitForExit();

            File.Delete(filePath);
        }

        private static List<Dependency> GetDependenciesIdentifiers(string manifest)
        {
            List<Dependency> dependencies = new List<Dependency>();

            Regex regDependency = new Regex(@"<dependency>(.*?)<\/dependency>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            Regex regAssemblyIdentity = new Regex(@".*dependencyType=""([a-z]*)""[^<]*<.*name=""(.[^ ""]+)""", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

            MatchCollection dependencyMatches = regDependency.Matches(manifest);
            foreach (Match dependencyMatch in dependencyMatches)
            {
                if (dependencyMatch.Success)
                {
                    var attrMatch = regAssemblyIdentity.Match(dependencyMatch.Value);

                    if (attrMatch.Success)
                    {
                        dependencies.Add(
                            new Dependency
                            {
                                Name = attrMatch.Groups[2].ToString(),
                                Type = attrMatch.Groups[1].ToString(),
                                Manifest = dependencyMatch.Value,
                                Index = dependencyMatch.Index,
                                Length = dependencyMatch.Length
                            });
                    }
                }
            }

            return dependencies;
        }

        private static string UpdateManifestDependencies(string manifest, List<Dependency> dependencies)
        {
            //Manifest Index
            dependencies.Reverse();

            foreach (var dependency in dependencies)
            {
                manifest = manifest.Replace(dependency.Index, dependency.Length, dependency.Manifest);
            }

            return manifest;
        }
    }
}

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static string Replace(this string s, int index, int length, string replacement)
        {
            var builder = new StringBuilder();
            builder.Append(s.Substring(0, index));
            builder.Append(replacement);
            builder.Append(s.Substring(index + length));
            return builder.ToString();
        }

        public static List<Dependency> StandardDependencies(this List<Dependency> projectDependencies, List<Dependency> standardDependencies)
        {
            foreach (var dependency in projectDependencies)
            {
                var standardNetDependency = standardDependencies.Where(d => d.Name == dependency.Name).SingleOrDefault();

                if (standardNetDependency != null)
                {
                    dependency.Manifest = standardNetDependency.Manifest;
                }
            }

            return projectDependencies;
        }
    }
}