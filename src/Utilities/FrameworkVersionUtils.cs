using System;
using System.IO;

namespace DebugHelper.Utilities
{
    public class FrameworkVersionUtils
    {
        public static string GetObjectDumpingDllPath(string targetFrameworkString)
        {
            var (success, directoryName) = GetFrameworkVersionDirectoryName(targetFrameworkString);
            if (!success)
                throw new ArgumentException(directoryName, nameof(targetFrameworkString));

            var dllLocation = Path.GetDirectoryName(new Uri(typeof(DebugHelperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);
            if (dllLocation == null)
                throw new ArgumentException("Cannot get the location of the ObjectDumping.dll", nameof(targetFrameworkString));

            return Path.Combine(dllLocation, "Libs", directoryName, "ObjectDumping.dll");
        }
        public static (bool success, string directoryName) GetFrameworkVersionDirectoryName(string targetFrameworkString)
        {
            var strings = targetFrameworkString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length != 2)
                return (false, $"Invalid framework name '{nameof(targetFrameworkString)}'");

            var frameworkName = strings[0].Trim();
            var vPosition = strings[1].IndexOf("=v", StringComparison.OrdinalIgnoreCase);
            if (vPosition <= 0)
                return (false, $"Invalid framework name '{nameof(targetFrameworkString)}'");

            if (!Version.TryParse(strings[1].Substring(vPosition + 2).Trim(), out var version))
                return (false, $"Invalid framework name '{nameof(targetFrameworkString)}'");

            switch (frameworkName.ToLowerInvariant())
            {
                case ".netcoreapp":
                    if (version >= new Version(7, 0))
                        return (true, "net7.0");

                    if (version >= new Version(6, 0))
                        return (true, "net6.0");

                    return (false, $"The .NET Core with a version '{version}' is not supported.");

                case ".netstandard":
                    return version < new Version(2, 0)
                        ? (false, "The .NET Standard with a version lower than 2.0 is not supported.")
                        : (true, "netstandard2.0");

                case ".netframework":
                    return version < new Version(4, 8)
                        ? (false, "The .NET Framework with a version lower than 4.5 is not supported.")
                        : (true, "net48");
                default:
                    return (false, $"Unsupported Framework: {targetFrameworkString}");
            }
        }
    }
}
