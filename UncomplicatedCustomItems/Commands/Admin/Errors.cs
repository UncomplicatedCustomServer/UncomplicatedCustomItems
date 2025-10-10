using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandSystem;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using YamlDotNet.Core;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Errors : ISubcommand
    {
        public string Name { get; } = "errors";

        public string Description { get; } = "Gets any errors that occured when loading a CustomItem";

        public string VisibleArgs { get; } = "[id]";

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.errors";

        public string[] Aliases { get; } = ["er"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            string requestedId = arguments?.FirstOrDefault();

            if (CustomItem.ErrorCustomItems.IsEmpty())
            {
                if (!string.IsNullOrEmpty(requestedId))
                    response = $"No errors were found for id '{requestedId}'.";
                else
                    response = "No errors were found! :D";

                return true;
            }

            IEnumerable<ErrorCustomItem> items = CustomItem.ErrorCustomItems.AsEnumerable();

            if (!string.IsNullOrEmpty(requestedId))
            {
                if (uint.TryParse(requestedId, out uint requestedNumeric))
                    items = items.Where(it => uint.TryParse(it.Id, out uint n) && n == requestedNumeric);
                else
                    items = items.Where(it => string.Equals(it.Id, requestedId, StringComparison.OrdinalIgnoreCase));

                if (!items.Any())
                {
                    int totalErrors = CustomItem.ErrorCustomItems.Count();
                    response = $"No errors were found for id '{requestedId}'. (Received argument: '{requestedId}'; total error items: {totalErrors})";
                    return true;
                }
            }

            List<ErrorCustomItem> sorted = items
                .OrderBy(item =>
                {
                    if (uint.TryParse(item.Id, out uint n))
                        return n;

                    return uint.MaxValue;
                })
                .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            response = string.Empty;
            if (!string.IsNullOrEmpty(requestedId))
                response += $"Showing errors for Id: {requestedId}\n";

            foreach (ErrorCustomItem errorItem in sorted)
            {
                response += $"\n<color=#FFFFFF>📄</color> <b>Id - Name:</b> {errorItem.Id} - {errorItem.Name}";
                response += "\n<color=#FFFFFF>\ud83d\udcc4</color> <b>File:</b> " + Path.GetFileName(errorItem.Path);

                Exception exception = errorItem.Exception;
                YamlException ex = (YamlException)(object)((exception is YamlException) ? exception : null);
                if (ex != null)
                    response += $"\n<color=#00FFFF>\ud83d\udd22</color> Line: {ex.Start.Line}, Column: {ex.Start.Column}";

                response += "\n<color=red>❌</color> Error: " + errorItem.Message;
                response += "\n<color=#FFFF00>\ud83d\udca1</color> Suggestion: " + GetSuggestionFromMessage(errorItem.Exception.Message) + "\n";
            }

            return true;
        }

        private static string GetSuggestionFromMessage(string message)
        {
            message = message?.ToLowerInvariant() ?? string.Empty;
            return message switch
            {
                string m when m.Contains("mapping values are not allowed") => "Make sure there is a space after the colon (e.g., `name: Beam` instead of `name:Beam`).",
                string m when m.Contains("expected 'mappingstart', got 'sequencestart'") => "Your YAML file begins with a list (`- item`) but should begin with a mapping. Try adding a top-level key before your list.",
                string m when m.Contains("while parsing a block mapping") => "Check indentation and YAML structure — something might be misaligned or nested incorrectly.",
                string m when m.Contains("expected <block end>, but found") => "Possibly missing a `-` for a list item or the element ends prematurely.",
                string m when m.Contains("did not find expected key") => "A key may be missing or misaligned — ensure all keys are followed by colons and correctly indented.",
                string m when m.Contains("unexpected end of stream") => "The file might be cut off unexpectedly — check for missing closing brackets or incomplete blocks.",
                string m when m.Contains("duplicate key") => "You may have defined the same key twice in the same block — YAML requires keys to be unique.",
                string m when m.Contains("found character that cannot start any token") => "There's probably an illegal character or wrong symbol — double-check for stray tabs or weird characters.",
                string m when m.Contains("found unexpected ':'") => "There might be a colon `:` in a value that should be quoted — try wrapping the value in quotes.",
                string m when m.Contains("anchor") && m.Contains("not defined") => "You're referencing an anchor (&value or *value) that hasn't been defined.",
                string m when m.Contains("alias") && m.Contains("not found") => "YAML alias (*) points to something that doesn't exist — check spelling or anchor placement.",
                string m when m.Contains("cannot convert") && m.Contains("to") => "A value might be of the wrong type — make sure it's in the correct format (e.g., number vs string).",
                string m when m.Contains("sequence entries are not allowed here") => "You're probably using a list (`- item`) in an invalid place — check indentation and nesting.",
                string m when m.Contains("unexpected key") || m.Contains("unexpected property") => "This key may be misplaced or invalid — double-check your schema or property names.",
                _ => "Check your YAML syntax near this location. Be sure indentation, colons, and types are correct.",
            };
        }
    }
}
