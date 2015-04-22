using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet.ProjectManagement
{
    /// <summary>
    /// project.json utils
    /// </summary>
    public static class ProjectJsonUtility
    {
        /// <summary>
        /// Read dependencies from a project.json file
        /// </summary>
        public static IEnumerable<PackageDependency> GetDependencies(JObject json)
        {
            JToken node = null;
            if (json.TryGetValue("dependencies", out node))
            {
                foreach (var dependency in node)
                {
                    yield return ParseDependency(dependency);
                }
            }

            yield break;
        }

        /// <summary>
        /// Convert a dependency entry into an id and version range
        /// </summary>
        public static PackageDependency ParseDependency(JToken dependencyToken)
        {
            var property = dependencyToken as JProperty;

            string id = property.Name;

            VersionRange range = null;

            if (dependencyToken.Type == JTokenType.Property)
            {
                range = VersionRange.Parse(((JProperty)dependencyToken).Value.ToString());
            }
            else
            {
                range = VersionRange.Parse(((JProperty)dependencyToken["version"]).Value.ToString());
            }

            return new PackageDependency(id, range);
        }

        /// <summary>
        /// Add a dependency to a project.json file
        /// </summary>
        public static void AddDependency(JObject json, PackageDependency dependency)
        {
            // Removing the older package if it exists
            RemoveDependency(json, dependency.Id);

            JObject dependencySet = null;

            JToken node = null;
            if (json.TryGetValue("dependencies", out node))
            {
                dependencySet = node as JObject;
            }

            if (dependencySet == null)
            {
                dependencySet = new JObject();
                json["dependencies"] = dependencySet;
            }

            var packageProperty = new JProperty(dependency.Id, dependency.VersionRange.MinVersion.ToNormalizedString());
            dependencySet.Add(packageProperty);
        }

        /// <summary>
        /// Remove a dependency from a project.json file
        /// </summary>
        public static void RemoveDependency(JObject json, string packageId)
        {
            JToken node = null;
            if (json.TryGetValue("dependencies", out node))
            {
                foreach (var dependency in node.ToArray())
                {
                    var dependencyProperty = dependency as JProperty;
                    if (StringComparer.OrdinalIgnoreCase.Equals(dependencyProperty.Name, packageId))
                    {
                        dependency.Remove();
                    }
                }
            }
        }
    }
}
