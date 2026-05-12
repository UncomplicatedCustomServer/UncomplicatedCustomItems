using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class BackupUploadRequest : APIRequest
    {
        public override string Name => nameof(BackupUploadRequest);

        public override string Endpoint => "backup/upload";

        public override RequestType Type => RequestType.Post;

        public override Dictionary<string, string> Headers { get; set; } = new()
        {
            ["Content-Type"] = "application/yaml",
            ["Token"] = BackupSystem.Key?.BackupCode ?? string.Empty
        };

        public override byte[] RawBody { get; set; } = Encoding.UTF8.GetBytes(ParseItems());

        private static string ParseItems()
        {
            string content = string.Empty;
            foreach (string fileName in Plugin.Instance.FileConfig.List())
            {
                try
                {
                    string fileContent = File.ReadAllText(fileName);
                    if (!Plugin.Instance.FileConfig.IsActionFile(fileContent))
                        content += fileContent + Environment.NewLine;
                }
                catch(Exception ex)
                {
                    LogManager.Error($"Failed to run {nameof(BackupUploadRequest)}.{nameof(ParseItems)} {ex}");
                }
            }

            return content;
        }
    }
}