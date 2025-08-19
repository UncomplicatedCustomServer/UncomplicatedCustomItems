using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public static class ArgumentManager
    {
        private class ConditionalParts
        {
            public bool IsUnless { get; set; }
            public string Condition { get; set; } = string.Empty;
            public string[] ThenActions { get; set; } = [];
            public string[]? ElseActions { get; set; }
        }
        
        private static readonly Dictionary<string, Action<ICustomItem, string[]>> _actionHandlers = new(StringComparer.OrdinalIgnoreCase);

        public static void Register(string name, Action<ICustomItem, string[]> handler) => _actionHandlers[name] = handler;

        public static void Trigger(ICustomItem customItem, ArgumentType type, EventArgs eventArgs)
        {
            if (customItem.Arguments == null || customItem.Arguments.Count == 0)
                return;

            if (!customItem.Arguments.TryGetValue(type, out string? actionString))
                return;

            foreach (string raw in actionString.Split(['\n', ';'], StringSplitOptions.RemoveEmptyEntries))
            {
                string resolved = ReplacePlaceholders(raw.Trim(), eventArgs);
                ExecuteAction(customItem, resolved, eventArgs);
            }
        }

        private static string ReplacePlaceholders(string action, EventArgs args)
        {
            int start;
            while ((start = action.IndexOf('{')) != -1)
            {
                int end = action.IndexOf('}', start);
                if (end == -1)
                    break;

                string placeholder = action.Substring(start + 1, end - start - 1);
                string value = ResolvePlaceholder(placeholder, args);

                action = action.Substring(0, start) + value + action.Substring(end + 1);
            }
            return action;
        }

        private static string ResolvePlaceholder(string path, object root)
        {
            try
            {
                object? current = root;

                foreach (string part in path.Split('.'))
                {
                    if (current == null)
                        return "null";

                    Type type = current.GetType();

                    PropertyInfo prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        current = prop.GetValue(current);
                        continue;
                    }

                    FieldInfo field = type.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                    {
                        current = field.GetValue(current);
                        continue;
                    }

                    MethodInfo method = type.GetMethod(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, Type.EmptyTypes, null);
                    if (method != null)
                    {
                        current = method.Invoke(current, null);
                        continue;
                    }

                    return $"<invalid:{part}>";
                }

                return current?.ToString() ?? "null";
            }
            catch (Exception ex)
            {
                return $"<error:{ex.Message}>";
            }
        }

        private static void ExecuteAction(ICustomItem item, string action, EventArgs eventArgs)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            if (IsConditionalStatement(action))
            {
                ExecuteConditional(item, action, eventArgs);
                return;
            }

            ExecuteRegularAction(item, action);
        }

        private static bool IsConditionalStatement(string action)
        {
            string trimmed = action.TrimStart();
            return trimmed.StartsWith("if ", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("unless ", StringComparison.OrdinalIgnoreCase);
        }

        private static void ExecuteConditional(ICustomItem item, string action, EventArgs eventArgs)
        {
            try
            {
                ConditionalParts parts = ParseConditionalStatement(action);
                bool conditionMet = EvaluateCondition(parts.Condition, eventArgs);
                
                if (parts.IsUnless)
                    conditionMet = !conditionMet;

                if (conditionMet)
                {
                    foreach (string thenAction in parts.ThenActions)
                    {
                        string resolved = ReplacePlaceholders(thenAction.Trim(), eventArgs);
                        ExecuteAction(item, resolved, eventArgs);
                    }
                }
                else if (parts.ElseActions?.Any() == true)
                {
                    foreach (string elseAction in parts.ElseActions)
                    {
                        string resolved = ReplacePlaceholders(elseAction.Trim(), eventArgs);
                        ExecuteAction(item, resolved, eventArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(ArgumentManager)} Error executing conditional: {ex.Message}");
            }
        }

        private static ConditionalParts ParseConditionalStatement(string action)
        {
            ConditionalParts result = new();
            
            bool isUnless = action.TrimStart().StartsWith("unless ", StringComparison.OrdinalIgnoreCase);
            result.IsUnless = isUnless;
            
            string keyword = isUnless ? "unless" : "if";
            int keywordIndex = action.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            
            int thenIndex = action.IndexOf(" then ", keywordIndex, StringComparison.OrdinalIgnoreCase);
            if (thenIndex == -1)
                throw new ArgumentException("Conditional statement must contain 'then' keyword");
            
            result.Condition = action.Substring(keywordIndex + keyword.Length, thenIndex - keywordIndex - keyword.Length).Trim();
            
            int elseIndex = action.IndexOf(" else ", thenIndex, StringComparison.OrdinalIgnoreCase);

            string thenPart;
            if (elseIndex != -1)
            {
                thenPart = action.Substring(thenIndex + 6, elseIndex - thenIndex - 6);
                string elsePart = action.Substring(elseIndex + 6);
                result.ElseActions = elsePart.Split('&').Select(a => a.Trim()).ToArray();
            }
            else
            {
                thenPart = action.Substring(thenIndex + 6);
            }
            
            result.ThenActions = thenPart.Split('&').Select(a => a.Trim()).ToArray();
            
            return result;
        }

        private static bool EvaluateCondition(string condition, EventArgs eventArgs)
        {
            try
            {
                if (condition.Contains(" contains "))
                {
                    return EvaluateContainsCondition(condition, eventArgs);
                }
                else if (condition.Contains(" is "))
                {
                    return EvaluateIsCondition(condition, eventArgs);
                }
                else if (condition.Contains(" equals "))
                {
                    return EvaluateEqualsCondition(condition, eventArgs);
                }
                else if (condition.Contains(" > ") || condition.Contains(" < ") || 
                         condition.Contains(" >= ") || condition.Contains(" <= "))
                {
                    return EvaluateNumericCondition(condition, eventArgs);
                }
                else if (condition.Contains(" != ") || condition.Contains(" == "))
                {
                    return EvaluateEqualityCondition(condition, eventArgs);
                }
                else
                {
                    string resolved = ReplacePlaceholders(condition, eventArgs);
                    return IsTruthy(resolved);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(ArgumentManager)} Error evaluating condition '{condition}': {ex.Message}");
                return false;
            }
        }

        private static bool EvaluateContainsCondition(string condition, EventArgs eventArgs)
        {
            string[] parts = condition.Split(new[] { " contains " }, (int)StringSplitOptions.None, (StringSplitOptions)StringComparison.OrdinalIgnoreCase);
            if (parts.Length != 2) return false;

            string leftValue = ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ReplacePlaceholders(parts[1].Trim().Trim('"', '\''), eventArgs);

            return leftValue.Contains(rightValue);
        }

        private static bool EvaluateIsCondition(string condition, EventArgs eventArgs)
        {
            string[] parts = condition.Split(new[] { " is " }, (int)StringSplitOptions.None, (StringSplitOptions)StringComparison.OrdinalIgnoreCase);
            if (parts.Length != 2) return false;

            string leftValue = ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = parts[1].Trim().Trim('"', '\'');

            return string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EvaluateEqualsCondition(string condition, EventArgs eventArgs)
        {
            string[] parts = condition.Split(new[] { " equals " }, (int)StringSplitOptions.None, (StringSplitOptions)StringComparison.OrdinalIgnoreCase);
            if (parts.Length != 2) return false;

            string leftValue = ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ReplacePlaceholders(parts[1].Trim().Trim('"', '\''), eventArgs);

            return string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EvaluateNumericCondition(string condition, EventArgs eventArgs)
        {
            string[] operators = { " >= ", " <= ", " > ", " < " };
            string? foundOperator = null;
            string[]? parts = null;

            foreach (string op in operators)
            {
                parts = condition.Split(new[] { op }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    foundOperator = op.Trim();
                    break;
                }
            }

            if (foundOperator == null || parts == null) return false;

            string leftValue = ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ReplacePlaceholders(parts[1].Trim(), eventArgs);

            if (!double.TryParse(leftValue, out double left) || !double.TryParse(rightValue, out double right))
                return false;

            return foundOperator switch
            {
                ">" => left > right,
                "<" => left < right,
                ">=" => left >= right,
                "<=" => left <= right,
                _ => false
            };
        }

        private static bool EvaluateEqualityCondition(string condition, EventArgs eventArgs)
        {
            bool isNotEquals = condition.Contains(" != ");
            string[] parts = condition.Split(new[] { isNotEquals ? " != " : " == " }, StringSplitOptions.None);
            if (parts.Length != 2) return false;

            string leftValue = ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ReplacePlaceholders(parts[1].Trim().Trim('"', '\''), eventArgs);

            bool areEqual = string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
            return isNotEquals ? !areEqual : areEqual;
        }

        private static bool IsTruthy(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null" || value == "0" || value == "false")
                return false;

            if (value.StartsWith("<error:") || value.StartsWith("<invalid:"))
                return false;

            return true;
        }

        private static void ExecuteRegularAction(ICustomItem item, string action)
        {
            string[] parts = action.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string command = parts[0];
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : [];

            if (_actionHandlers.TryGetValue(command, out var handler))
                handler(item, args);
            else
                LogManager.Error($"{nameof(ArgumentManager)} Unknown action: {command}");
        }
    }
}