using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Features.ArgumentHelpers;
using System.Text;
using System.Collections;
using UncomplicatedCustomItems.API.Attributes;

namespace UncomplicatedCustomItems.API.Features.Helper
{
#nullable enable
    /// <summary>
    /// Manages the action system for <see cref="CustomItem"/>s
    /// </summary>
    public static class ArgumentManager
    {
        private static readonly Dictionary<string, Type> _commonTypeMap = BuildCommonTypeMap();

        internal static readonly Dictionary<string, Action<ICustomItem, string[]>> _actionHandlers = new(StringComparer.OrdinalIgnoreCase);

        internal static readonly ConcurrentDictionary<Type, HashSet<string>> _eventArgPropertyCache = new();

        private static readonly Random _random = new();

        private static readonly Dictionary<string, Func<IEnumerable, object?>> _collectionOperations = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Random", GetRandomFromCollection },
            { "First", collection => collection.Cast<object?>().FirstOrDefault() },
            { "Last", collection => collection.Cast<object?>().LastOrDefault() },
            { "Count", collection => collection.Cast<object?>().Count() },
            { "Any", collection => collection.Cast<object?>().Any() }
        };

        private static readonly Dictionary<string, AssignmentOperator> _compoundOperators = new()
        {
            { "+=", AssignmentOperator.Add },
            { "-=", AssignmentOperator.Subtract },
            { "*=", AssignmentOperator.Multiply },
            { "/=", AssignmentOperator.Divide },
            { "%=", AssignmentOperator.Modulo },
            { "&=", AssignmentOperator.BitwiseAnd },
            { "|=", AssignmentOperator.BitwiseOr },
            { "^=", AssignmentOperator.BitwiseXor },
            { "=", AssignmentOperator.Assign }
        };

        /// <summary>
        /// Registers actions from <see cref="CustomItem"/>s
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public static void Register(string name, Action<ICustomItem, string[]> handler) => _actionHandlers[name] = handler;

        /// <summary>
        /// Triggers the actions registered in <see cref="CustomItem"/>s
        /// </summary>
        /// <param name="customItem"></param>
        /// <param name="type"></param>
        /// <param name="eventArgs"></param>
        public static void Trigger(ICustomItem customItem, ArgumentType type, EventArgs eventArgs)
        {
            if (customItem.Arguments == null || customItem.Arguments.Count == 0)
                return;

            if (!customItem.Arguments.TryGetValue(type, out string? actionString))
                return;

            LogManager.Debug($"Executing action: {actionString}");

            foreach (string raw in actionString.Split(['\n', ';'], StringSplitOptions.RemoveEmptyEntries))
                ExecuteAction(customItem, raw.Trim(), eventArgs);
        }

        private static Dictionary<string, Type> BuildCommonTypeMap()
        {
            Dictionary<string, Type> map = new(StringComparer.OrdinalIgnoreCase);

            try
            {
                Type enumType = typeof(CommonTypes);
                foreach (FieldInfo? field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    CommonTypesAttribute attr = field.GetCustomAttribute<CommonTypesAttribute>();
                    if (attr?.CommonType != null)
                    {
                        string name = field.Name;
                        if (!map.ContainsKey(name))
                            map[name] = attr.CommonType;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to build common type map: {ex.Message}");
            }

            return map;
        }

        private static bool TryGetCommonType(string name, out Type? type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                type = null;
                return false;
            }

            return _commonTypeMap.TryGetValue(name.Trim(), out type);
        }

        internal static string ReplacePlaceholders(string action, EventArgs args)
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

        private static bool TryResolveType(string fullName, out Type? found)
        {
            found = null;
            if (string.IsNullOrWhiteSpace(fullName))
                return false;

            Type? t = Type.GetType(fullName, false, true);
            if (t != null)
            {
                found = t;
                return true;
            }

            if (TryGetCommonType(fullName, out Type? common) && common != null)
            {
                found = common;
                return true;
            }

            string lastToken = fullName.Split('.').LastOrDefault() ?? fullName;
            if (TryGetCommonType(lastToken, out Type? common2) && common2 != null)
            {
                found = common2;
                return true;
            }

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetType(fullName, false, true);
                    if (t != null)
                    {
                        found = t;
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetTypes().FirstOrDefault(tt => string.Equals(tt.Name, fullName, StringComparison.OrdinalIgnoreCase));
                    if (t != null)
                    {
                        found = t;
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        private static readonly char[] _identifierDelimiters = ['.', '(', ')', '[', ']', ',', ' ', '\t'];

        private static string ExpandCommonTypeNamesInPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            bool anyKeyPresent = _commonTypeMap.Keys.Any(k => path.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!anyKeyPresent)
                return path;

            StringBuilder sb = new(path);
            foreach (var kvp in _commonTypeMap)
            {
                string shortName = kvp.Key;
                string fullName = kvp.Value.FullName ?? kvp.Value.Name;

                int startIndex = 0;
                while (true)
                {
                    int idx = sb.ToString().IndexOf(shortName, startIndex, StringComparison.OrdinalIgnoreCase);
                    if (idx == -1)
                        break;

                    bool okBefore = (idx == 0) || _identifierDelimiters.Contains(sb[idx - 1]);
                    int afterIdx = idx + shortName.Length;
                    bool okAfter = (afterIdx >= sb.Length) || _identifierDelimiters.Contains(sb[afterIdx]);

                    if (okBefore && okAfter)
                    {
                        sb.Remove(idx, shortName.Length);
                        sb.Insert(idx, fullName);
                        startIndex = idx + fullName.Length;
                    }
                    else
                    {
                        startIndex = idx + shortName.Length;
                    }
                }
            }

            return sb.ToString();
        }


        internal static object? ResolvePlaceholderToObject(string path, object root)
        {
            if (string.IsNullOrWhiteSpace(path) || root == null)
                return null;

            try
            {
                object? current = root;
                string[] parts = path.Split('.').Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];

                    // Handle fully qualified types like "My.Namespace.TypeName.SomeStaticMember"
                    if (i == 0)
                    {
                        string candidate = parts[0];
                        int lastCandidateIndex = 0;
                        Type? foundType = null;
                        if (TryResolveType(candidate, out foundType))
                        {
                            current = foundType;
                            i = 0;
                        }
                        else
                        {
                            for (int j = 1; j < parts.Length; j++)
                            {
                                candidate = string.Join(".", parts.Take(j + 1));
                                if (TryResolveType(candidate, out foundType))
                                {
                                    current = foundType;
                                    lastCandidateIndex = j;
                                    i = lastCandidateIndex;
                                    break;
                                }
                            }
                        }
                        if (current is Type)
                            continue;
                    }

                    if (current is IEnumerable enumerable && current is not string && _collectionOperations.TryGetValue(part, out var op))
                    {
                        current = op(enumerable);
                        continue;
                    }

                    int openBracket = part.IndexOf('[');
                    if (openBracket >= 0)
                    {
                        int closeBracket = part.LastIndexOf(']');
                        if (closeBracket <= openBracket)
                            return null;

                        string baseName = part.Substring(0, openBracket).Trim();
                        string inner = part.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();

                        object? memberContainer = string.IsNullOrEmpty(baseName) ? current : GetMemberValue(current!, baseName);
                        if (memberContainer == null)
                            return null;

                        if (memberContainer is IEnumerable enumContainer && !(memberContainer is string))
                        {
                            List<object?> list = enumContainer.Cast<object?>().ToList();

                            if (_collectionOperations.ContainsKey(inner))
                            {
                                current = _collectionOperations[inner](list);
                                continue;
                            }

                            if (int.TryParse(inner, out int idx))
                            {
                                current = (idx >= 0 && idx < list.Count) ? list[idx] : null;
                                continue;
                            }

                            if (EvaluateConditions.TryParsePredicate(inner, out string? predProp, out string _, out string? predVal))
                            {
                                current = list.FirstOrDefault(el =>
                                {
                                    if (el == null)
                                        return false;
                                    if (EvaluateConditions.TryGetMemberStringValue(el, predProp, out string? memberVal))
                                        return string.Equals(memberVal, predVal, StringComparison.OrdinalIgnoreCase);
                                    return false;
                                });
                                continue;
                            }

                            string literal = EvaluateConditions.TrimQuotes(inner);
                            current = list.FirstOrDefault(el => string.Equals(el?.ToString(), literal, StringComparison.OrdinalIgnoreCase));
                            continue;
                        }
                        else
                            current = memberContainer;
                    }

                    current = GetMemberValue(current!, part);
                    if (current == null)
                        return null;
                }

                return current;
            }
            catch
            {
                return null;
            }
        }

        private static object? GetMemberValue(object targetOrType, string name)
        {
            if (string.IsNullOrEmpty(name))
                return targetOrType;

            BindingFlags instanceFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            BindingFlags staticFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

            if (targetOrType is Type asType)
            {
                PropertyInfo? prop = asType.GetProperty(name, staticFlags);
                if (prop != null)
                    return prop.GetValue(null);

                FieldInfo? field = asType.GetField(name, staticFlags);
                if (field != null)
                    return field.GetValue(null);

                MethodInfo? method = asType.GetMethod(name, staticFlags, null, Type.EmptyTypes, null);
                if (method != null)
                    return method.Invoke(null, null);

                var methods = asType.GetMethods(staticFlags).Where(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
                MethodInfo? zero = methods.FirstOrDefault(m => m.GetParameters().Length == 0);
                if (zero != null)
                    return zero.Invoke(null, null);

                return null;
            }
            else
            {
                Type? t = targetOrType.GetType();

                PropertyInfo? prop = t.GetProperty(name, instanceFlags);
                if (prop != null)
                    return prop.GetValue(targetOrType);

                FieldInfo? field = t.GetField(name, instanceFlags);
                if (field != null)
                    return field.GetValue(targetOrType);

                MethodInfo? method = t.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, Type.EmptyTypes, null);
                if (method != null)
                    return method.Invoke(targetOrType, null);

                prop = t.GetProperty(name, staticFlags);
                if (prop != null)
                    return prop.GetValue(null);

                field = t.GetField(name, staticFlags);
                if (field != null)
                    return field.GetValue(null);

                return null;
            }
        }

        private static object? GetRandomFromCollection(IEnumerable collection)
        {
            List<object?> list = collection.Cast<object?>().ToList();
            if (list.Count == 0)
                return null;

            int randomIndex = _random.Next(list.Count);
            return list[randomIndex];
        }

        private static object[] ParseAndResolveParameters(string parametersPart, EventArgs eventArgs, ICustomItem? item)
        {
            if (string.IsNullOrWhiteSpace(parametersPart))
                return [];

            List<object> parameters = [];
            List<string> paramTokens = ParseParameterTokens(parametersPart);

            foreach (string token in paramTokens)
            {
                string replaced = ReplacePlaceholders(token, eventArgs);
                replaced = ExpandCommonTypeNamesInPath(replaced); 

                if ((replaced.Length >= 2) && ((replaced.StartsWith("\"") && replaced.EndsWith("\"")) || (replaced.StartsWith("'") && replaced.EndsWith("'"))))
                {
                    parameters.Add(replaced.Substring(1, replaced.Length - 2));
                    continue;
                }

                if (bool.TryParse(replaced, out bool b))
                {
                    parameters.Add(b);
                    continue;
                }
                if (byte.TryParse(replaced, out byte by))
                {
                    parameters.Add(by);
                    continue;
                }
                if (int.TryParse(replaced, out int i))
                {
                    parameters.Add(i);
                    continue;
                }
                if (long.TryParse(replaced, out long l))
                {
                    parameters.Add(l);
                    continue;
                }
                if (float.TryParse(replaced, out float f))
                {
                    parameters.Add(f);
                    continue;
                }
                if (double.TryParse(replaced, out double d))
                {
                    parameters.Add(d);
                    continue;
                }

                object? resolvedObj = ResolveTargetObject(token, item, eventArgs);
                if (resolvedObj != null)
                {
                    parameters.Add(resolvedObj);
                    continue;
                }

                if (replaced != token)
                {
                    resolvedObj = ResolveTargetObject(replaced, item, eventArgs);
                    if (resolvedObj != null)
                    {
                        parameters.Add(resolvedObj);
                        continue;
                    }
                }

                parameters.Add(replaced);
            }

            return parameters.ToArray();
        }


        private static List<string> ParseParameterTokens(string parametersPart)
        {
            List<string> tokens = [];
            StringBuilder currentToken = new();
            int parenthesesDepth = 0;
            bool inQuotes = false;
            char quoteChar = '\0';

            for (int i = 0; i < parametersPart.Length; i++)
            {
                char c = parametersPart[i];

                if (!inQuotes && (c == '"' || c == '\''))
                {
                    inQuotes = true;
                    quoteChar = c;
                    currentToken.Append(c);
                }
                else if (inQuotes && c == quoteChar)
                {
                    inQuotes = false;
                    currentToken.Append(c);
                }
                else if (!inQuotes && c == '(')
                {
                    parenthesesDepth++;
                    currentToken.Append(c);
                }
                else if (!inQuotes && c == ')')
                {
                    parenthesesDepth--;
                    currentToken.Append(c);
                }
                else if (!inQuotes && c == ',' && parenthesesDepth == 0)
                {
                    tokens.Add(currentToken.ToString().Trim());
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }

            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString().Trim());
            }

            return tokens;
        }

        private static string ResolvePlaceholder(string path, object root)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return "null";

                object? obj = ResolvePlaceholderToObject(path, root);
                if (obj != null)
                    return obj?.ToString() ?? "null";

                object? current = root;
                foreach (string part in path.Split('.'))
                {
                    if (current == null)
                        return "null";

                    Type type = current.GetType();

                    PropertyInfo? prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        current = prop.GetValue(current);
                        continue;
                    }

                    FieldInfo? field = type.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                    {
                        current = field.GetValue(current);
                        continue;
                    }

                    MethodInfo? method = type.GetMethod(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, Type.EmptyTypes, null);
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

            ExecuteRegularAction(item, action, eventArgs);
        }

        private static bool IsConditionalStatement(string action)
        {
            string trimmed = action.TrimStart();
            return trimmed.StartsWith("if ", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("unless ", StringComparison.OrdinalIgnoreCase);
        }

        private static void ExecuteConditional(ICustomItem item, string action, EventArgs eventArgs)
        {
            try
            {
                ConditionalParts parts = ParseConditionalStatement(action);

                string resolvedCondition = ReplacePlaceholders(parts.Condition, eventArgs);
                bool conditionMet = EvaluateCondition(resolvedCondition, eventArgs);

                if (parts.IsUnless)
                    conditionMet = !conditionMet;

                if (conditionMet)
                {
                    foreach (string thenAction in parts.ThenActions)
                    {
                        ExecuteAction(item, thenAction.Trim(), eventArgs);
                    }
                }
                else if (parts.ElseActions?.Any() == true)
                {
                    foreach (string elseAction in parts.ElseActions)
                    {
                        ExecuteAction(item, elseAction.Trim(), eventArgs);
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
                // Note: condition should already have placeholders resolved when this is called
                if (condition.Contains(" contains "))
                {
                    return EvaluateConditions.EvaluateContainsCondition(condition, eventArgs);
                }
                else if (condition.Contains(" is "))
                {
                    return EvaluateConditions.EvaluateIsCondition(condition, eventArgs);
                }
                else if (condition.Contains(" equals "))
                {
                    return EvaluateConditions.EvaluateEqualsCondition(condition, eventArgs);
                }
                else if (condition.Contains(" > ") || condition.Contains(" < ") || condition.Contains(" >= ") || condition.Contains(" <= "))
                {
                    return EvaluateConditions.EvaluateNumericCondition(condition, eventArgs);
                }
                else if (condition.Contains(" != ") || condition.Contains(" == "))
                {
                    return EvaluateConditions.EvaluateEqualityCondition(condition, eventArgs);
                }
                else
                {
                    return IsTruthy(condition);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(ArgumentManager)} Error evaluating condition '{condition}': {ex.Message}");
                return false;
            }
        }

        private static bool IsTruthy(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null" || value == "0" || value == "false")
                return false;

            if (value.StartsWith("<error:") || value.StartsWith("<invalid:"))
                return false;

            return true;
        }

        private static void HandleComplexAssignment(ICustomItem item, string propertyPath, string valueExpression, AssignmentOperator operatorType, EventArgs eventArgs)
        {
            try
            {
                object? resolvedValue = null;

                if (valueExpression.Contains('.') && !valueExpression.StartsWith("\"") && !valueExpression.StartsWith("'"))
                {
                    resolvedValue = ResolveTargetObject(valueExpression, item, eventArgs);
                }

                if (resolvedValue == null && valueExpression.Contains('(') && !valueExpression.StartsWith("\"") && !valueExpression.StartsWith("'"))
                {
                    resolvedValue = ResolveTargetObject(valueExpression, item, eventArgs);
                }

                if (resolvedValue == null)
                {
                    string resolvedValueExpression = ReplacePlaceholders(valueExpression, eventArgs);

                    if (!string.Equals(resolvedValueExpression, valueExpression, StringComparison.Ordinal))
                    {
                        if (resolvedValueExpression.Contains('.') || resolvedValueExpression.Contains('('))
                        {
                            resolvedValue = ResolveTargetObject(resolvedValueExpression, item, eventArgs);
                        }
                    }

                    if (resolvedValue == null)
                    {
                        resolvedValue = resolvedValueExpression;
                    }
                }

                object? currentValue = null;
                if (operatorType != AssignmentOperator.Assign)
                {
                    if (ShouldResolveFromEventArgs(propertyPath, eventArgs))
                    {
                        string currentValueStr = ResolvePlaceholder(propertyPath, eventArgs);
                        if (currentValueStr != "null" && !currentValueStr.StartsWith("<error:") && !currentValueStr.StartsWith("<invalid:"))
                        {
                            currentValue = currentValueStr;
                        }
                    }
                    else
                    {
                        object root = item != null ? (object)item : eventArgs;
                        currentValue = GetPropertyValue(root, propertyPath);
                    }
                }

                object? newValue;
                if (operatorType == AssignmentOperator.Assign)
                {
                    newValue = resolvedValue;
                }
                else
                {
                    string valueExprStr = resolvedValue?.ToString() ?? "null";
                    newValue = CalculateNewValue(currentValue, valueExprStr, operatorType, propertyPath);
                }

                if (ShouldResolveFromEventArgs(propertyPath, eventArgs))
                {
                    SetProperty(eventArgs, propertyPath, newValue, eventArgs);
                }
                else
                {
                    object root = item != null ? (object)item : eventArgs;
                    SetProperty(root, propertyPath, newValue, eventArgs);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to handle complex assignment '{propertyPath} = {valueExpression}': {ex.Message}");
            }
        }

        private static void ExecuteRegularAction(ICustomItem item, string action, EventArgs eventArgs)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            if (action.Contains("::"))
            {
                ExecuteMethodCall(item, action, eventArgs);
                return;
            }

            if (action.StartsWith("action ", StringComparison.OrdinalIgnoreCase) || action.StartsWith("execute ", StringComparison.OrdinalIgnoreCase) || action.StartsWith("run ", StringComparison.OrdinalIgnoreCase))
            {
                HandleCustomActionExecution(action, eventArgs);
                return;
            }

            var operatorInfo = FindAssignmentOperator(action);
            if (operatorInfo.HasValue)
            {
                var (operatorType, operatorIndex, operatorLength) = operatorInfo.Value;

                string propertyPath = action.Substring(0, operatorIndex).Trim();
                string valueExpression = action.Substring(operatorIndex + operatorLength).Trim();

                if (propertyPath.Contains('{') && propertyPath.Contains('}'))
                {
                    HandlePlaceholderPropertyAssignment(item, propertyPath, valueExpression, operatorType, eventArgs);
                }
                else
                {
                    HandleComplexAssignment(item, propertyPath, valueExpression, operatorType, eventArgs);
                }
                return;
            }

            string resolvedAction = ReplacePlaceholders(action, eventArgs);

            string[] parts = resolvedAction.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string command = parts[0];
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

            if (_actionHandlers.TryGetValue(command, out var handler))
                handler(item, args);
            else
                LogManager.Error($"{nameof(ArgumentManager)} Unknown action: {command}");
        }

        private static void HandleCustomActionExecution(string action, EventArgs eventArgs)
        {
            try
            {
                string[] parts = action.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    LogManager.Error($"{nameof(ArgumentManager)}: Invalid action command format: {action}");
                    return;
                }

                string identifier = string.Join(" ", parts.Skip(1));
                identifier = ReplacePlaceholders(identifier, eventArgs);

                if (uint.TryParse(identifier, out uint actionId))
                {
                    ExecuteCustomAction(actionId, eventArgs);
                    return;
                }

                ICustomAction foundAction = CustomAction.List.FirstOrDefault(a => string.Equals(a.Name, identifier, StringComparison.OrdinalIgnoreCase));

                if (foundAction != null)
                {
                    ExecuteCustomAction(foundAction, eventArgs);
                }
                else
                {
                    LogManager.Error($"{nameof(ArgumentManager)}: CustomAction not found: {identifier}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(ArgumentManager)}: Error executing custom action '{action}': {ex.Message}");
            }
        }

        private static (AssignmentOperator operatorType, int index, int length)? FindAssignmentOperator(string action)
        {
            foreach (var kvp in _compoundOperators)
            {
                int index = action.IndexOf(kvp.Key);
                if (index > 0)
                {
                    return (kvp.Value, index, kvp.Key.Length);
                }
            }

            return null;
        }

        private static bool ShouldResolveFromEventArgs(string propertyPath, object? eventArgs)
        {
            if (eventArgs == null || string.IsNullOrWhiteSpace(propertyPath))
                return false;

            string firstPart = propertyPath.Split('.')[0].Trim();
            if (firstPart.Length == 0)
                return false;

            Type evtType = eventArgs.GetType();

            HashSet<string> names = _eventArgPropertyCache.GetOrAdd(evtType, t =>
            {
                HashSet<string> set = new(StringComparer.OrdinalIgnoreCase);

                foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    set.Add(p.Name);

                return set;
            });

            return names.Contains(firstPart);
        }

        private static void ExecuteMethod(object targetObject, string methodName, object[] parameters)
        {
            try
            {
                Type targetType = targetObject.GetType();
                string rawName = methodName;
                Type[]? genericTypeArgs = null;

                int genOpen = methodName.IndexOf('[');
                int genClose = methodName.LastIndexOf(']');
                if (genOpen >= 0 && genClose > genOpen)
                {
                    string genSpec = methodName.Substring(genOpen + 1, genClose - genOpen - 1);
                    rawName = methodName.Substring(0, genOpen);

                    string[]? genTypeNames = genSpec.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
                    List<Type> genTypes = [];
                    foreach (string? tname in genTypeNames)
                    {
                        if (!TryResolveType(tname, out Type? resolved) || resolved == null)
                        {
                            LogManager.Error($"Generic type '{tname}' could not be resolved for method '{methodName}'.");
                            return;
                        }
                        genTypes.Add(resolved);
                    }
                    genericTypeArgs = genTypes.ToArray();
                }

                MethodInfo[]? methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                                .Where(m => string.Equals(m.Name, rawName, StringComparison.OrdinalIgnoreCase))
                                .ToArray();

                MethodInfo? method = null;

                if (genericTypeArgs != null)
                {
                    MethodInfo[]? genericDefs = methods.Where(m => m.IsGenericMethodDefinition && m.GetGenericArguments().Length == genericTypeArgs.Length).ToArray();
                    method = genericDefs.FirstOrDefault(m => m.GetParameters().Length == parameters.Length);

                    if (method != null)
                        method = method.MakeGenericMethod(genericTypeArgs);
                }
                else
                {
                    Type[] parameterTypes = parameters.Select(p => p?.GetType() ?? typeof(string)).ToArray();
                    method = targetType.GetMethod(rawName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, parameterTypes, null);

                    if (method == null)
                    {
                        MethodInfo[]? byCount = methods.Where(m => m.GetParameters().Length == parameters.Length).ToArray();
                        if (byCount.Length == 1)
                            method = byCount[0];
                        else if (byCount.Length > 1)
                        {
                            method = byCount.FirstOrDefault(m =>
                            {
                                ParameterInfo[]? methodParams = m.GetParameters();
                                for (int i = 0; i < methodParams.Length; i++)
                                {
                                    Type? expected = methodParams[i].ParameterType;
                                    Type? actual = parameters[i]?.GetType() ?? typeof(string);
                                    if (!expected.IsAssignableFrom(actual) && parameters[i] is not string)
                                        return false;
                                }
                                return true;
                            }) ?? byCount.First();
                        }
                    }
                }

                if (method == null)
                {
                    LogManager.Error($"Method '{methodName}' not found on type '{targetType.Name}' with {parameters.Length} parameters");
                    return;
                }

                ParameterInfo[] methodParams = method.GetParameters();
                object[] convertedParams = new object[methodParams.Length];

                for (int i = 0; i < methodParams.Length && i < parameters.Length; i++)
                {
                    try
                    {
                        Type expectedType = methodParams[i].ParameterType;
                        object? value = parameters[i];

                        if (value != null && expectedType.IsAssignableFrom(value.GetType()))
                        {
                            convertedParams[i] = value;
                            continue;
                        }

                        if (value is string stringValue && stringValue.Length > 0 && ((stringValue.StartsWith("\"") && stringValue.EndsWith("\"")) || (stringValue.StartsWith("'") && stringValue.EndsWith("'"))))
                            stringValue = stringValue.Substring(1, stringValue.Length - 2);

                        if (value is string sVal && expectedType == typeof(string))
                        {
                            convertedParams[i] = sVal;
                        }
                        else if (value is string sVal2)
                        {
                            convertedParams[i] = Convert.ChangeType(sVal2, expectedType);
                        }
                        else
                        {
                            convertedParams[i] = Convert.ChangeType(value, expectedType);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"Failed to convert parameter {i} for method '{methodName}': {ex.Message}");
                        convertedParams[i] = methodParams[i].HasDefaultValue ? methodParams[i].DefaultValue : null;
                    }
                }

                for (int i = parameters.Length; i < methodParams.Length; i++)
                    convertedParams[i] = methodParams[i].HasDefaultValue ? methodParams[i].DefaultValue : null;

                object? result = method.Invoke(targetObject, convertedParams);
                LogManager.Debug($"Successfully executed method '{methodName}' on {targetType.Name}. Result: {result?.ToString() ?? "null"}");
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to execute method '{methodName}': {ex.Message}");
            }
        }

        private static void ExecuteMethodCall(ICustomItem item, string methodCall, EventArgs eventArgs)
        {
            try
            {
                methodCall = ExpandCommonTypeNamesInPath(methodCall);

                if (!methodCall.Contains("::"))
                {
                    LogManager.Error($"Invalid method call syntax. Expected 'ObjectPath::MethodName(params)': {methodCall}");
                    return;
                }

                string[] parts = methodCall.Split(["::"], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    LogManager.Error($"Invalid method call format: {methodCall}");
                    return;
                }

                string objectPath = parts[0].Trim();
                string methodCallPart = parts[1].Trim();

                int openParen = methodCallPart.IndexOf('(');
                int closeParen = methodCallPart.LastIndexOf(')');

                if (openParen == -1 || closeParen == -1 || closeParen <= openParen)
                {
                    LogManager.Error($"Invalid method call syntax. Missing parentheses: {methodCall}");
                    return;
                }

                string methodName = methodCallPart.Substring(0, openParen).Trim();
                string parametersPart = methodCallPart.Substring(openParen + 1, closeParen - openParen - 1).Trim();

                object? targetObject = ResolveTargetObject(objectPath, item, eventArgs);
                if (targetObject == null)
                {
                    LogManager.Error($"Could not resolve target object: {objectPath}");
                    return;
                }

                object[] parameters = ParseAndResolveParameters(parametersPart, eventArgs, item);

                ExecuteMethod(targetObject, methodName, parameters);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error executing method call '{methodCall}': {ex.Message}");
            }
        }

        private static object ResolveTargetObject(string objectPath, ICustomItem item, EventArgs eventArgs)
        {
            objectPath = ExpandCommonTypeNamesInPath(objectPath);
            LogManager.Debug($"Attempting to resolve: {objectPath}");

            LogManager.Debug($"EventArgs type: {eventArgs.GetType().Name}");
            LogManager.Debug($"EventArgs properties: {string.Join(", ", eventArgs.GetType().GetProperties().Select(p => p.Name))}");

            int newIndex = objectPath.IndexOf(".new(", StringComparison.OrdinalIgnoreCase);
            if (newIndex >= 0)
            {
                string typeName = objectPath.Substring(0, newIndex);
                int openParen = objectPath.IndexOf('(', newIndex);
                int closeParen = FindClosingParen(objectPath, openParen);
                if (closeParen > openParen)
                {
                    string paramsPart = objectPath.Substring(openParen + 1, closeParen - openParen - 1);
                    string remainder = objectPath.Length > closeParen + 1 ? objectPath.Substring(closeParen + 1) : "";

                    if (TryResolveType(typeName, out Type? resolvedType) && resolvedType != null)
                    {
                        object? instance = CreateInstanceFromType(resolvedType, paramsPart, eventArgs, item);
                        if (instance == null)
                            return null;

                        if (!string.IsNullOrWhiteSpace(remainder))
                        {
                            string suffix = remainder.Trim();
                            if (suffix.StartsWith(".")) suffix = suffix.Substring(1);
                            return ResolvePlaceholderToObject(suffix, instance);
                        }

                        return instance;
                    }
                }
            }

            // God help me if I have to update this.
            HashSet<string> knownObjectTypes = new(StringComparer.OrdinalIgnoreCase)
            {
                "Player", "Target", "Attacker", "Item", "NewItem", "OldItem", "Pickup",
                "ThrowableItem", "UsableItem", "FirearmItem", "KeycardItem", "Revolver",
                "RadioItem", "JailbirdItem", "LightItem", "DamageHandler", "ShootingTarget",
                "Tesla", "CoinItem", "Interactable", "Door", "Generator", "Locker", "Chamber",
                "NewRoom", "OldRoom", "Hazard", "Window", "Rigidbody", "ProjectileSettings",
                "AmmoPickup", "BodyArmorPickup", "CandyPickup", "CandyItem", "Role", "OldRole",
                "NewRole", "Effect", "Group", "Issuer", "Sender", "Message"
            };

            int parenIndex = objectPath.IndexOf('(');
            if (parenIndex >= 0)
            {
                string firstToken = objectPath.Substring(0, parenIndex).Split('.')[0].Trim();
                int closeParen = FindClosingParen(objectPath, parenIndex);
                if (closeParen > parenIndex)
                {
                    string beforeParen = objectPath.Substring(0, parenIndex).Trim();
                    string paramsPart = objectPath.Substring(parenIndex + 1, closeParen - parenIndex - 1);
                    string remainder = objectPath.Length > closeParen + 1 ? objectPath.Substring(closeParen + 1) : "";

                    string[] nameParts = beforeParen.Split('.', (char)StringSplitOptions.RemoveEmptyEntries);
                    Type? resolvedType = null;
                    int matchedParts = 0;

                    for (int j = nameParts.Length; j >= 1; j--)
                    {
                        string candidate = string.Join(".", nameParts.Take(j));
                        if (TryResolveType(candidate, out Type? t) && t != null)
                        {
                            resolvedType = t;
                            matchedParts = j;
                            break;
                        }
                    }

                    if (resolvedType != null)
                    {
                        object? instance = CreateInstanceFromType(resolvedType, paramsPart, eventArgs, item);
                        if (instance == null)
                            return null;

                        string suffix = "";
                        if (matchedParts < nameParts.Length)
                        {
                            suffix = string.Join(".", nameParts.Skip(matchedParts));
                            if (!string.IsNullOrWhiteSpace(remainder))
                                suffix += remainder.StartsWith(".") ? remainder : "." + remainder;
                        }
                        else if (!string.IsNullOrWhiteSpace(remainder))
                        {
                            suffix = remainder;
                        }

                        if (!string.IsNullOrWhiteSpace(suffix))
                        {
                            suffix = suffix.Trim();
                            if (suffix.StartsWith(".")) suffix = suffix.Substring(1);
                            return ResolvePlaceholderToObject(suffix, instance);
                        }

                        return instance;
                    }
                }
            }


            if (knownObjectTypes.Contains(objectPath))
            {
                object resolvedFromArgs = ResolvePlaceholderToObject(objectPath, eventArgs);
                if (resolvedFromArgs != null)
                    return resolvedFromArgs;
            }

            if (TryResolveType(objectPath, out Type? type))
                return type;

            object? resolved = ResolvePlaceholderToObject(objectPath, eventArgs);
            if (resolved != null)
                return resolved;

            if (item != null)
            {
                resolved = ResolvePlaceholderToObject(objectPath, item);
                if (resolved != null)
                    return resolved;
            }

            return null;
        }

        private static void HandlePlaceholderPropertyAssignment(ICustomItem item, string propertyPathPlaceholder, string valueExpression, AssignmentOperator operatorType, EventArgs eventArgs)
        {
            try
            {
                string placeholderContent = propertyPathPlaceholder.Trim('{', '}');

                PropertyTarget? propertyTarget = ResolvePropertyTarget(placeholderContent, eventArgs);

                if (propertyTarget == null)
                {
                    LogManager.Error($"Could not resolve target object from placeholder: {propertyPathPlaceholder}");
                    return;
                }

                object? currentValue = null;
                if (operatorType != AssignmentOperator.Assign)
                    currentValue = propertyTarget.GetValue();

                string resolvedValueExpression = ReplacePlaceholders(valueExpression, eventArgs);

                object? newValue = CalculateNewValue(currentValue, resolvedValueExpression, operatorType, placeholderContent);

                propertyTarget.SetValue(newValue);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to handle placeholder property assignment '{propertyPathPlaceholder}': {ex.Message}");
            }
        }

        private static PropertyTarget? ResolvePropertyTarget(string path, object root)
        {
            try
            {
                object? current = root;
                string[] parts = path.Split('.');

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    if (current == null)
                        return null;

                    Type type = current.GetType();

                    PropertyInfo? prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        current = prop.GetValue(current);
                        continue;
                    }

                    FieldInfo? field = type.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                    {
                        current = field.GetValue(current);
                        continue;
                    }

                    MethodInfo? method = type.GetMethod(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, Type.EmptyTypes, null);
                    if (method != null)
                    {
                        current = method.Invoke(current, null);
                        continue;
                    }

                    LogManager.Error($"Property, field, or method not found: {part} in path {path}");
                    return null;
                }

                if (current == null || parts.Length == 0)
                    return null;

                string finalPart = parts[parts.Length - 1];
                Type finalType = current.GetType();

                PropertyInfo? finalProp = finalType.GetProperty(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                FieldInfo? finalField = finalType.GetField(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (finalProp != null || finalField != null)
                {
                    return new PropertyTarget
                    {
                        Target = current,
                        PropertyInfo = finalProp,
                        FieldInfo = finalField,
                        MemberName = finalPart
                    };
                }

                LogManager.Error($"Final property or field not found: {finalPart} on type {finalType.Name}");
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error resolving property target for path '{path}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Executes a <see cref="CustomAction"/> by the action Id
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="eventArgs"></param>
        public static void ExecuteCustomAction(uint actionId, EventArgs eventArgs)
        {
            if (!CustomAction.CustomActions.TryGetValue(actionId, out ICustomAction customAction))
            {
                LogManager.Error($"{nameof(ArgumentManager)}: CustomAction with ID {actionId} not found");
                return;
            }

            ExecuteCustomAction(customAction, eventArgs);
        }

        /// <summary>
        /// Executes a <see cref="CustomAction"/> by the action
        /// </summary>
        /// <param name="customAction"></param>
        /// <param name="eventArgs"></param>
        public static void ExecuteCustomAction(ICustomAction customAction, EventArgs eventArgs)
        {
            if (customAction?.Actions == null || customAction.Actions.Length == 0)
                return;

            LogManager.Debug($"{nameof(ArgumentManager)}: Executing CustomAction '{customAction.Name}' (ID: {customAction.Id})");

            foreach (string action in customAction.Actions)
            {
                if (!string.IsNullOrWhiteSpace(action))
                {
                    ExecuteAction(null, action.Trim(), eventArgs);
                }
            }
        }

        private static object? GetPropertyValue(object root, string propertyPath)
        {
            object? current = root;
            foreach (string part in propertyPath.Split('.'))
            {
                if (current == null)
                    return null;

                Type type = current.GetType();

                PropertyInfo? prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    current = prop.GetValue(current);
                    continue;
                }

                FieldInfo? field = type.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }

                throw new ArgumentException($"Property or field not found: {part}");
            }

            return current;
        }

        private static object? CalculateNewValue(object? currentValue, string valueExpression, AssignmentOperator operatorType, string propertyPath)
        {
            if (operatorType == AssignmentOperator.Assign)
            {
                return valueExpression;
            }

            if (operatorType == AssignmentOperator.Add && currentValue is string currentStr)
            {
                return currentStr + valueExpression;
            }

            if (IsNumericOperation(operatorType))
            {
                return CalculateNumericValue(currentValue, valueExpression, operatorType);
            }

            if (IsBitwiseOperation(operatorType))
            {
                return CalculateBitwiseValue(currentValue, valueExpression, operatorType);
            }

            throw new NotSupportedException($"Operator {operatorType} not supported for property {propertyPath}");
        }

        private static bool IsNumericOperation(AssignmentOperator operatorType)
        {
            return operatorType == AssignmentOperator.Add ||
                   operatorType == AssignmentOperator.Subtract ||
                   operatorType == AssignmentOperator.Multiply ||
                   operatorType == AssignmentOperator.Divide ||
                   operatorType == AssignmentOperator.Modulo;
        }

        private static bool IsBitwiseOperation(AssignmentOperator operatorType)
        {
            return operatorType == AssignmentOperator.BitwiseAnd ||
                   operatorType == AssignmentOperator.BitwiseOr ||
                   operatorType == AssignmentOperator.BitwiseXor;
        }

        private static object? CalculateNumericValue(object? currentValue, string valueExpression, AssignmentOperator operatorType)
        {
            if (currentValue == null)
                throw new ArgumentException("Cannot perform numeric operation on null value");

            if (!TryConvertToDouble(currentValue, out double current))
                throw new ArgumentException($"Cannot convert current value to number: {currentValue}");

            if (!double.TryParse(valueExpression, out double operand))
                throw new ArgumentException($"Cannot convert operand to number: {valueExpression}");

            double result = operatorType switch
            {
                AssignmentOperator.Add => current + operand,
                AssignmentOperator.Subtract => current - operand,
                AssignmentOperator.Multiply => current * operand,
                AssignmentOperator.Divide => operand != 0 ? current / operand : throw new DivideByZeroException(),
                AssignmentOperator.Modulo => operand != 0 ? current % operand : throw new DivideByZeroException(),
                _ => throw new NotSupportedException($"Numeric operator {operatorType} not supported")
            };

            return ConvertToOriginalType(result, currentValue.GetType());
        }

        private static object? CalculateBitwiseValue(object? currentValue, string valueExpression, AssignmentOperator operatorType)
        {
            if (currentValue == null)
                throw new ArgumentException("Cannot perform bitwise operation on null value");

            if (!TryConvertToLong(currentValue, out long current))
                throw new ArgumentException($"Cannot convert current value to integer: {currentValue}");

            if (!long.TryParse(valueExpression, out long operand))
                throw new ArgumentException($"Cannot convert operand to integer: {valueExpression}");

            long result = operatorType switch
            {
                AssignmentOperator.BitwiseAnd => current & operand,
                AssignmentOperator.BitwiseOr => current | operand,
                AssignmentOperator.BitwiseXor => current ^ operand,
                _ => throw new NotSupportedException($"Bitwise operator {operatorType} not supported")
            };

            return ConvertToOriginalType(result, currentValue.GetType());
        }

        private static bool TryConvertToDouble(object value, out double result)
        {
            result = 0;
            try
            {
                result = Convert.ToDouble(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryConvertToLong(object value, out long result)
        {
            result = 0;
            try
            {
                result = Convert.ToInt64(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static object? ConvertToOriginalType(object value, Type targetType)
        {
            try
            {
                if (targetType == typeof(int) || targetType == typeof(int?))
                    return Convert.ToInt32(value);
                if (targetType == typeof(long) || targetType == typeof(long?))
                    return Convert.ToInt64(value);
                if (targetType == typeof(float) || targetType == typeof(float?))
                    return Convert.ToSingle(value);
                if (targetType == typeof(double) || targetType == typeof(double?))
                    return Convert.ToDouble(value);
                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    return Convert.ToDecimal(value);
                if (targetType == typeof(byte) || targetType == typeof(byte?))
                    return Convert.ToByte(value);
                if (targetType == typeof(short) || targetType == typeof(short?))
                    return Convert.ToInt16(value);

                return value;
            }
            catch
            {
                return value;
            }
        }

        private static void SetProperty(object root, string propertyPath, object? newValue, EventArgs eventArgs)
        {
            try
            {
                object target = root;
                string[] parts = propertyPath.Split('.');

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];

                    PropertyInfo? prop = target.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        target = prop.GetValue(target) ?? throw new NullReferenceException($"Property {part} is null");
                        continue;
                    }

                    FieldInfo? field = target.GetType().GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                    {
                        target = field.GetValue(target) ?? throw new NullReferenceException($"Field {part} is null");
                        continue;
                    }

                    LogManager.Error($"Property or field not found: {part}");
                    return;
                }

                string finalPart = parts.Last();

                PropertyInfo? finalProp = target.GetType().GetProperty(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (finalProp != null)
                {
                    if (newValue == null)
                    {
                        finalProp.SetValue(target, null);
                        return;
                    }

                    Type propType = finalProp.PropertyType;

                    if (newValue != null && propType.IsAssignableFrom(newValue.GetType()))
                    {
                        finalProp.SetValue(target, newValue);
                        return;
                    }

                    if (newValue is string stringValue)
                    {
                        object convertedValue = Convert.ChangeType(stringValue, propType);
                        finalProp.SetValue(target, convertedValue);
                        return;
                    }

                    try
                    {
                        object convertedValue = Convert.ChangeType(newValue, propType);
                        finalProp.SetValue(target, convertedValue);
                        return;
                    }
                    catch
                    {
                        if (newValue != null && propType.IsAssignableFrom(newValue.GetType()))
                        {
                            finalProp.SetValue(target, newValue);
                            return;
                        }

                        LogManager.Error($"Failed to convert value of type '{newValue?.GetType().Name}' to property type '{propType.Name}' for {finalPart}");
                        return;
                    }
                }

                FieldInfo? finalField = target.GetType().GetField(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (finalField != null)
                {
                    if (newValue == null)
                    {
                        finalField.SetValue(target, null);
                        return;
                    }

                    Type fieldType = finalField.FieldType;

                    if (newValue != null && fieldType.IsAssignableFrom(newValue.GetType()))
                    {
                        finalField.SetValue(target, newValue);
                        return;
                    }

                    if (newValue is string sVal)
                    {
                        object convertedValue = Convert.ChangeType(sVal, fieldType);
                        finalField.SetValue(target, convertedValue);
                        return;
                    }

                    try
                    {
                        object convertedValue = Convert.ChangeType(newValue, fieldType);
                        finalField.SetValue(target, convertedValue);
                        return;
                    }
                    catch
                    {
                        if (newValue != null && fieldType.IsAssignableFrom(newValue.GetType()))
                        {
                            finalField.SetValue(target, newValue);
                            return;
                        }

                        LogManager.Error($"Failed to convert value of type '{newValue?.GetType().Name}' to field type '{fieldType.Name}' for {finalPart}");
                        return;
                    }
                }

                LogManager.Error($"Final property or field not found: {finalPart}");
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to set property '{propertyPath}': {ex.Message}");
            }
        }

        private static int FindClosingParen(string s, int openIndex)
        {
            int depth = 0;
            for (int i = openIndex; i < s.Length; i++)
            {
                if (s[i] == '(')
                    depth++;
                else if (s[i] == ')')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }

            return -1;
        }

        private static object? CreateInstanceFromType(Type type, string parametersPart, EventArgs eventArgs, ICustomItem? item)
        {
            try
            {
                object[] parameters = ParseAndResolveParameters(parametersPart, eventArgs, item);

                ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                ConstructorInfo? ctor = ctors.FirstOrDefault(c => c.GetParameters().Length == parameters.Length &&
                    c.GetParameters().Select((p, i) =>
                    {
                        Type expected = p.ParameterType;
                        object? actual = parameters[i];
                        if (actual == null)
                            return !expected.IsValueType || (Nullable.GetUnderlyingType(expected) != null);

                        return expected.IsAssignableFrom(actual.GetType()) || (actual is string);
                    }).All(b => b));

                if (ctor == null)
                {
                    ctor = ctors.FirstOrDefault(c => c.GetParameters().Length == parameters.Length);
                }

                if (ctor == null)
                {
                    LogManager.Error($"No matching constructor found on type '{type.FullName}' with {parameters.Length} parameters");
                    return null;
                }

                ParameterInfo[] ctorParams = ctor.GetParameters();
                object[] converted = new object[ctorParams.Length];

                for (int i = 0; i < ctorParams.Length; i++)
                {
                    Type expected = ctorParams[i].ParameterType;
                    object? value = parameters[i];

                    if (value == null)
                    {
                        converted[i] = ctorParams[i].HasDefaultValue ? ctorParams[i].DefaultValue : (expected.IsValueType ? Activator.CreateInstance(expected) : null);
                        continue;
                    }

                    if (expected.IsAssignableFrom(value.GetType()))
                    {
                        converted[i] = value;
                        continue;
                    }

                    if (value is string sVal && ((sVal.StartsWith("\"") && sVal.EndsWith("\"")) || (sVal.StartsWith("'") && sVal.EndsWith("'"))))
                        sVal = sVal.Substring(1, sVal.Length - 2);

                    try
                    {
                        if (expected == typeof(float) || expected == typeof(float?))
                            converted[i] = Convert.ChangeType(value, typeof(float));
                        else if (expected == typeof(double) || expected == typeof(double?))
                            converted[i] = Convert.ChangeType(value, typeof(double));
                        else if (expected == typeof(int) || expected == typeof(int?))
                            converted[i] = Convert.ChangeType(value, typeof(int));
                        else
                            converted[i] = Convert.ChangeType(value, expected);
                    }
                    catch
                    {
                        if (expected.IsAssignableFrom(value.GetType()))
                            converted[i] = value;
                        else
                        {
                            LogManager.Error($"Failed to convert constructor parameter {i} from '{value?.GetType().Name}' to '{expected.Name}' for type '{type.Name}'");
                            converted[i] = ctorParams[i].HasDefaultValue ? ctorParams[i].DefaultValue : (expected.IsValueType ? Activator.CreateInstance(expected) : null);
                        }
                    }
                }

                return ctor.Invoke(converted);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to create instance of type '{type.FullName}': {ex.Message}");
                return null;
            }
        }
    }
}