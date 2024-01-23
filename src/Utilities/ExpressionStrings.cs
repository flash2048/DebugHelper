namespace DebugHelper.Utilities
{
    public class ExpressionStrings
    {
        public static string GetVariableType(string variableName)
        {
            return $"{variableName}.GetType().ToString()";
        }

        public static string GetStringForSave(string variableType, string variableName)
        {
            switch (variableType)
            {
                case "System.String":
                    return $"{variableName}"; 
                case "System.IO.MemoryStream":
                    return $"Convert.ToBase64String({variableName}.ToArray())";
                case "System.Byte[]":
                    return $"Convert.ToBase64String({variableName})";
                default: return $"{variableName}.ToString()";
            }
        }

        public static string GetSaveString(string path, string variableType, string variableName)
        {
            return $"System.IO.File.WriteAllText(@\"{path}\", {GetStringForSave(variableType, variableName)})";
        }
    }
}
