using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using DebugHelper.Utilities;
using Westwind.Scripting;

namespace DebugHelper.Tests
{
    [TestClass]
    public class RunCodeFromStringTests
    {
        [TestMethod]
        public async Task VariableTypeTestsAsync()
        {
            var str = Constants.ImageBase64;
            var array = Convert.FromBase64String(str);

            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            var code = $@"
byte[] array = (byte[]) @0;
return {ExpressionStrings.GetVariableType("array")};
";
            var result = await script.ExecuteCodeAsync(code, array);
            Assert.AreEqual("System.Byte[]", result);

            script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            code = $@"
var str = (string) @0;
return {ExpressionStrings.GetVariableType("str")};
";
            result = await script.ExecuteCodeAsync(code, str);
            Assert.AreEqual("System.String", result);
        }

        [TestMethod]
        public async Task SavedStringDataTestsAsync()
        {
            var str = "test";
            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            var code = $@"
var str = (string) @0;
return {ExpressionStrings.GetVariableType("str")};
";
            var result = await script.ExecuteCodeAsync(code, str);
            Assert.AreEqual("System.String", result);

            script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            code = $@"
var str = (string) @0;
return {ExpressionStrings.GetStringForSave(result.ToString(), "str")};
";
            result = await script.ExecuteCodeAsync(code, str);
            Assert.AreEqual($"{str}", result);
        }

        [TestMethod]
        public async Task SavedArrayDataTestsAsync()
        {
            var array = new byte[] { 1, 2, 3, 4, 5 };
            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            var code = $@"
var array = (byte[]) @0;
return {ExpressionStrings.GetVariableType("array")};
";
            var result = await script.ExecuteCodeAsync(code, array);
            Assert.AreEqual("System.Byte[]", result);

            script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            code = $@"
var array = (byte[]) @0;
return {ExpressionStrings.GetStringForSave(result.ToString(), "array")};
";
            result = await script.ExecuteCodeAsync(code, array);
            Assert.AreEqual($"{Convert.ToBase64String(array)}", result);
        }

        [TestMethod]
        public async Task CheckImageFromBase64TestAsync()
        {
            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            var code = $@"
var str = (string) @0;
return {ExpressionStrings.GetVariableType("str")};
";
            var objectType = await script.ExecuteCodeAsync<string>(code, Constants.ImageBase64);
            Assert.AreEqual("System.String", objectType);

            using (var temporaryFile = new TemporaryFile())
            {
                script = new CSharpScriptExecution();
                script.AddDefaultReferencesAndNamespaces();
                code = $@"
var str = (string) @0;
{ExpressionStrings.GetSaveString(temporaryFile.FileName, objectType, "str")};
                ";
                await script.ExecuteCodeAsync(code, Constants.ImageBase64);
                var result = temporaryFile.ReadAllText();

                Assert.AreEqual(Constants.ImageBase64, result);
            }
        }

        [TestMethod]
        public async Task CheckImageFromByteArrayAsync()
        {
            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            var code = $@"
var array = (byte[]) @0;
return {ExpressionStrings.GetVariableType("array")};
";
            var array = Convert.FromBase64String(Constants.ImageBase64);

            var objectType = await script.ExecuteCodeAsync<string>(code, array);
            Assert.AreEqual("System.Byte[]", objectType);

            using (var temporaryFile = new TemporaryFile())
            {
                script = new CSharpScriptExecution();
                script.AddDefaultReferencesAndNamespaces();
                code = $@"
var array = (byte[]) @0;
{ExpressionStrings.GetSaveString(temporaryFile.FileName, objectType, "array")};
                ";
                await script.ExecuteCodeAsync(code, array);
                var result = temporaryFile.ReadAllText();

                Assert.AreEqual(Constants.ImageBase64, result);
            }
        }

        [TestMethod]
        public async Task CheckImageFromUrlStringTestAsync()
        {
            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            var urlString = "https://github.com/flash2048/DebugHelper/raw/main/assets/DebugHelper256.png";
            var code = $@"
var str = (string) @0;
return {ExpressionStrings.GetVariableType("str")};
";
            var objectType = await script.ExecuteCodeAsync<string>(code, urlString);
            Assert.AreEqual("System.String", objectType);

            using (var temporaryFile = new TemporaryFile())
            {
                script = new CSharpScriptExecution();
                script.AddDefaultReferencesAndNamespaces();
                code = $@"
var str = (string) @0;
{ExpressionStrings.GetSaveString(temporaryFile.FileName, objectType, "str")};
                ";
                await script.ExecuteCodeAsync(code, urlString);
                var result = temporaryFile.ReadAllText();

                Assert.AreEqual(urlString, result);
            }
        }

        [TestMethod]
        public async Task CheckImageFromUrlTestAsync()
        {
            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();
            var urlString = new Uri("https://github.com/flash2048/DebugHelper/raw/main/assets/DebugHelper256.png");
            var code = $@"
var uri = (Uri) @0;
return {ExpressionStrings.GetVariableType("uri")};
";
            var objectType = await script.ExecuteCodeAsync<string>(code, urlString);
            Assert.AreEqual("System.Uri", objectType);

            using (var temporaryFile = new TemporaryFile())
            {
                script = new CSharpScriptExecution();
                script.AddDefaultReferencesAndNamespaces();
                code = $@"
var uri = (Uri) @0;
{ExpressionStrings.GetSaveString(temporaryFile.FileName, objectType, "uri")};
                ";
                await script.ExecuteCodeAsync(code, urlString);
                var result = temporaryFile.ReadAllText();

                Assert.AreEqual(urlString, result);
            }
        }

        [TestMethod]
        public async Task CheckImageFromMemoryStreamTestAsync()
        {
            var script = new CSharpScriptExecution();
            script.AddDefaultReferencesAndNamespaces();

            var ms = new System.IO.MemoryStream(Convert.FromBase64String(Constants.ImageBase64));

            var code = $@"
var ms = (System.IO.MemoryStream) @0;
return {ExpressionStrings.GetVariableType("ms")};
";
            var objectType = await script.ExecuteCodeAsync<string>(code, ms);
            Assert.AreEqual("System.IO.MemoryStream", objectType);

            using (var temporaryFile = new TemporaryFile())
            {
                script = new CSharpScriptExecution();
                script.AddDefaultReferencesAndNamespaces();
                code = $@"
var ms = (System.IO.MemoryStream) @0;
{ExpressionStrings.GetSaveString(temporaryFile.FileName, objectType, "ms")};
                ";
                await script.ExecuteCodeAsync(code, ms);
                var result = temporaryFile.ReadAllText();

                Assert.AreEqual(Constants.ImageBase64, result);
            }
        }
    }
}
