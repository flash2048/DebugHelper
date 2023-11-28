using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DebugHelper.Utilities;

namespace DebugHelper.Tests
{
    [TestClass]
    public class DllInjectTests
    {
        [TestMethod]
        [DataRow(".NETFramework,Version=v4.8", true, "net48")]
        [DataRow(".NETFramework,Version=v4.7.2", true, "net48")]
        [DataRow(".NETFramework,Version=v4.3", false, "The .NET Framework with a version lower than 4.5 is not supported.")]
        [DataRow(".NETCoreApp,Version=v7.0", true, "net7.0")]
        [DataRow(".NETCoreApp,Version=v7.1", true, "net7.0")]
        [DataRow(".NETCoreApp,Version=v6.0", true, "net6.0")]
        [DataRow(".NETCoreApp,Version=v6.1", true, "net6.0")]
        [DataRow(".NETCoreApp,Version=v5.0", false, "The .NET Core with a version '5.0' is not supported.")]
        [DataRow(".NETStandard,Version=v2.0", true, "netstandard2.0")]
        [DataRow(".NETStandard,Version=v2.1", true, "netstandard2.0")]
        [DataRow(".NETStandard,Version=v1.0", false, "The .NET Standard with a version lower than 2.0 is not supported.")]
        public void TestFrameworkVersionDirectoryName(string frameworkString, bool success, string directoryName)
        {
            Assert.AreEqual(FrameworkVersionUtils.GetFrameworkVersionDirectoryName(frameworkString), (success, directoryName));
        }

        [TestMethod]
        [DataRow(".NETFramework,Version=v4.8")]
        [DataRow(".NETCoreApp,Version=v7.0")]
        [DataRow(".NETCoreApp,Version=v7.1")]
        [DataRow(".NETCoreApp,Version=v6.0")]
        [DataRow(".NETCoreApp,Version=v6.1")]
        [DataRow(".NETStandard,Version=v2.0")]
        [DataRow(".NETStandard,Version=v2.1")]
        public void TestObjectDumpingDllPath(string frameworkString)
        {
            var path = FrameworkVersionUtils.GetObjectDumpingDllPath(frameworkString);
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        [DataRow(".NETFramework,Version=v4.8")]
        [DataRow(".NETCoreApp,Version=v7.0")]
        [DataRow(".NETCoreApp,Version=v7.1")]
        [DataRow(".NETCoreApp,Version=v6.0")]
        [DataRow(".NETCoreApp,Version=v6.1")]
        [DataRow(".NETStandard,Version=v2.0")]
        [DataRow(".NETStandard,Version=v2.1")]
        public void TestSystemTextJsonDllPath(string frameworkString)
        {
            var path = FrameworkVersionUtils.GetSystemTextJsonDllPath(frameworkString);
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        [DataRow(".NETFramework,Version=v4.8")]
        [DataRow(".NETCoreApp,Version=v7.0")]
        [DataRow(".NETCoreApp,Version=v7.1")]
        [DataRow(".NETCoreApp,Version=v6.0")]
        [DataRow(".NETCoreApp,Version=v6.1")]
        [DataRow(".NETStandard,Version=v2.0")]
        [DataRow(".NETStandard,Version=v2.1")]
        public void TestLoadDll(string frameworkString)
        {
            // System.Text.Json.JsonSerializer
            var path = FrameworkVersionUtils.GetSystemTextJsonDllPath(frameworkString);
            var assembly = System.Reflection.Assembly.LoadFile($"{path}");
            Assert.AreEqual(assembly.GetName().Name, "System.Text.Json");

            // ObjectDumping
            path = FrameworkVersionUtils.GetObjectDumpingDllPath(frameworkString);
            assembly = System.Reflection.Assembly.LoadFile($"{path}");
            Assert.AreEqual(assembly.GetName().Name, "ObjectDumping");
        }
    }
}
