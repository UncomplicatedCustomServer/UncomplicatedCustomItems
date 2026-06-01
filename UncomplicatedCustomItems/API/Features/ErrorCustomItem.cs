using System;
using System.Linq;

namespace UncomplicatedCustomItems.API.Features
{
    internal class ErrorCustomItem
    {
        public static string HandleErrorString(Exception ex, bool showErrorName = false)
        {
            string text = (showErrorName ? (ex.GetType().Name + " ") : string.Empty) + ex.Message;

            if (ex.InnerException != null)
                text = text + " -> " + ex.InnerException.Message;

            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                text = text + " -> " + ex.InnerException.InnerException.Message;
                
            return text;
        }
        
        public static string GetRoleFileElement(string[] pieces, string rowPart, bool removeSpaces = true)
        {
            string line = pieces?.FirstOrDefault(l => l?.IndexOf(rowPart, StringComparison.OrdinalIgnoreCase) >= 0) ?? "N/D";
            if (line == "N/D")
                return line;

            int pos = line.IndexOf(rowPart, StringComparison.OrdinalIgnoreCase);
            if (pos < 0)
                return "N/D";

            string value = line.Substring(pos + rowPart.Length).Trim();

            return removeSpaces ? value.Replace(" ", string.Empty) : value;
        }

        public string Path { get; }

        public string[] Content { get; }

        public Exception Exception { get; }

        public string Message { get; }

        public string Id => GetRoleFileElement(Content, "id:");

        public string Name => GetRoleFileElement(Content, "name:");

        internal ErrorCustomItem(string path, string[] content, Exception exception, string? message = null)
        {
            Path = path;
            Content = content;
            Exception = exception;
            Message = message ?? HandleErrorString(exception, showErrorName: true);
        }
    }
}
