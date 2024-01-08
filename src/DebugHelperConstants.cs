using System;

namespace DebugHelper
{
    public class DebugHelperConstants
    {
        public static readonly Guid CommandSet = new Guid("7b60b658-3867-4350-b719-337dc4a02d5b");
        public static readonly int MaxDepthValue = 15;
        public const string CsharpName = "C#";
        public const string ConsoleName = "Console";
        public const string JsonName = "Json";
        public const string DotNet7Directory = "net7.0";
        public const string DotNet6Directory = "net6.0";
        public const string DotNetStandardDirectory = "netstandard2.0";
        public const string DotNetFrameworkDirectory = "net48";
        public const int DebuggerExpressionTimeoutMilliseconds = 20000;
    }
}
