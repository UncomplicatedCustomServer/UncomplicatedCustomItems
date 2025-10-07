using System;
using System.Collections.Generic;
using System.IO;
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

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.errors";

        public string[] Aliases { get; } = ["er"];
        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (CustomItem.ErrorCustomItems.IsEmpty())
            {
                response = "No errors were found! :D";
                return true;
            }

            response = string.Empty;
            foreach (ErrorCustomItem errorItem in CustomItem.ErrorCustomItems)
            {
                response = response + "\n<color=#FFFFFF>\ud83d\udcc4</color> <b>File:</b> " + Path.GetFileName(errorItem.Path);
                Exception exception = errorItem.Exception;
                YamlException ex = (YamlException)(object)((exception is YamlException) ? exception : null);
                if (ex != null)
                    response += $"\n<color=#00FFFF>\ud83d\udd22</color> Line: {ex.Start.Line}, Column: {ex.Start.Column}";

                response = response + "\n<color=red>❌</color> Error: " + errorItem.Message;
                response = response + "\n<color=#FFFF00>\ud83d\udca1</color> Suggestion: " + GetSuggestionFromMessage(errorItem.Exception.Message) + "\n";
            }
            
            return true;
        }

        private static string GetSuggestionFromMessage(string message)
        {
            message = message.ToLowerInvariant();
            if (message.Contains("mapping values are not allowed"))
            {
                return "Make sure there is a space after the colon (e.g., `name: Beam` instead of `name:Beam`).";
            }
            if (message.Contains("expected 'mappingstart', got 'sequencestart'"))
            {
                return "Your YAML file begins with a list (`- item`) but should begin with a mapping. Try adding a top-level key before your list.";
            }
            if (message.Contains("while parsing a block mapping"))
            {
                return "Check indentation and YAML structure — something might be misaligned or nested incorrectly.";
            }
            if (message.Contains("expected <block end>, but found"))
            {
                return "Possibly missing a `-` for a list item or the element ends prematurely.";
            }
            if (message.Contains("did not find expected key"))
            {
                return "A key may be missing or misaligned — ensure all keys are followed by colons and correctly indented.";
            }
            if (message.Contains("unexpected end of stream"))
            {
                return "The file might be cut off unexpectedly — check for missing closing brackets or incomplete blocks.";
            }
            if (message.Contains("duplicate key"))
            {
                return "You may have defined the same key twice in the same block — YAML requires keys to be unique.";
            }
            if (message.Contains("found character that cannot start any token"))
            {
                return "There's probably an illegal character or wrong symbol — double-check for stray tabs or weird characters.";
            }
            if (message.Contains("found unexpected ':'"))
            {
                return "There might be a colon `:` in a value that should be quoted — try wrapping the value in quotes.";
            }
            if (message.Contains("anchor") && message.Contains("not defined"))
            {
                return "You're referencing an anchor (&value or *value) that hasn't been defined.";
            }
            if (message.Contains("alias") && message.Contains("not found"))
            {
                return "YAML alias (*) points to something that doesn't exist — check spelling or anchor placement.";
            }
            if (message.Contains("cannot convert") && message.Contains("to"))
            {
                return "A value might be of the wrong type — make sure it's in the correct format (e.g., number vs string).";
            }
            if (message.Contains("sequence entries are not allowed here"))
            {
                return "You're probably using a list (`- item`) in an invalid place — check indentation and nesting.";
            }
            if (message.Contains("unexpected key") || message.Contains("unexpected property"))
            {
                return "This key may be misplaced or invalid — double-check your schema or property names.";
            }
            return "Check your YAML syntax near this location. Be sure indentation, colons, and types are correct.";
        }
    }
}