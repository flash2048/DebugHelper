using System.Collections.Generic;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace DebugHelper.Extensions
{
    public static class Dte2Extension
    {
        public static Expressions GetLocals(this DTE2 dte2)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return dte2.Debugger.CurrentStackFrame.Locals;
        }

        public static List<string> GetLocalNames(this DTE2 dte2)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = new List<string>(dte2.Debugger.CurrentStackFrame.Locals.Count);
            foreach (Expression expression in dte2.Debugger.CurrentStackFrame.Locals)
            {
                result.Add(expression.Name);
            }
            return result;
        }

        public static string GetFrameworkVersionString(this DTE2 dte2)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return dte2.GetExpressionResultString("((System.Runtime.Versioning.TargetFrameworkAttribute)System.Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Runtime.Versioning.TargetFrameworkAttribute)))?.FrameworkName");
        }

        public static string GetExpressionResultString(this DTE dte, string expression)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = dte.Debugger.GetExpression(expression, Timeout: DebugHelperConstants.DebuggerExpressionTimeoutMilliseconds);
            return Regex.Unescape(result.Value.Trim('"'));
        }
    }
}
