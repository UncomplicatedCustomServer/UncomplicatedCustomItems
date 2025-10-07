using System;
using System.IO;
using System.Linq;
using YamlDotNet.Core;

namespace UncomplicatedCustomItems.API.Features
{
    public class ErrorCustomItem
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
        public static string GetRoleFileElement(string content, string rowPart, bool removeSpaces = true)
        {
            return GetRoleFileElement(content.Split([Environment.NewLine], StringSplitOptions.None), rowPart, removeSpaces);
        }

        public static string GetRoleFileElement(string[] pieces, string rowPart, bool removeSpaces = true)
        {
            string text = pieces.FirstOrDefault(l => l.Contains(rowPart)) ?? "N/D";
            if (removeSpaces)
                text.Replace(" ", string.Empty);

            return text.Replace(rowPart + " ", string.Empty).Replace(rowPart, string.Empty);
        }

        public string Path { get; }

        public string[] Content { get; }

        public Exception Exception { get; }

        public string Message { get; }

        public string Id => GetRoleFileElement(Content, "id:");

        internal ErrorCustomItem(string path, string[] content, Exception exception, string message = null)
        {
            Path = path;
            Content = content;
            Exception = exception;
            Message = message ?? HandleErrorString(exception, showErrorName: true);
        }
    }
}