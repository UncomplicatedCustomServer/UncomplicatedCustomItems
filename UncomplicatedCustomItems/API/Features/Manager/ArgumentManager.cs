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
using MEC;

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

        private static readonly ConcurrentDictionary<string, Type> _typeResolutionCache = new(StringComparer.OrdinalIgnoreCase);

        internal static Dictionary<(Type, string), Delegate> CachedDelegates { get; } = [];

        internal static readonly ConcurrentDictionary<ICustomItem, Dictionary<string, object?>> _variables = new();

        private static readonly char[] _identifierDelimiters = ['.', '(', ')', '[', ']', ',', ' ', '\t'];

        private static readonly Random _random = new();

        internal static Dictionary<(Type, string), PropertyInfo> CachedProperties { get; } = [];
        
        internal static Dictionary<(Type, string), FieldInfo> CachedFields { get; } = [];

        internal static Dictionary<(Type, string), MethodInfo> CachedMethods { get; } = [];

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
            if (action.IndexOf('{') == -1) 
                return action;
            
            StringBuilder sb = new(action.Length + 32);
            int lastIndex = 0;
            int start;
            
            while ((start = action.IndexOf('{', lastIndex)) != -1)
            {
                int end = action.IndexOf('}', start);
                if (end == -1) 
                    break;

                sb.Append(action, lastIndex, start - lastIndex);
                string placeholder = action.Substring(start + 1, end - start - 1);
                string value = ResolvePlaceholder(placeholder, args);
                sb.Append(value);
                lastIndex = end + 1;
            }
            
            if (lastIndex < action.Length)
                sb.Append(action, lastIndex, action.Length - lastIndex);
            
            return sb.ToString();
        }

        private static bool TryResolveType(string fullName, out Type? found)
        {
            found = null;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                found = null;
                return false;
            }

            if (_typeResolutionCache.TryGetValue(fullName, out found))
                return found != null;

            if (TryGetCommonType(fullName, out found) && found != null)
            {
                _typeResolutionCache[fullName] = found;
                return true;
            }

            Type? t = Type.GetType(fullName, false, true);
            if (t != null)
            {
                _typeResolutionCache[fullName] = t;
                found = t;
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

            _typeResolutionCache[fullName] = null;
            return false;
        }
        
        private static string ExpandCommonTypeNamesInPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            bool anyKeyPresent = false;
            foreach (string key in _commonTypeMap.Keys)
            {
                if (path.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    anyKeyPresent = true;
                    break;
                }
            }

            if (!anyKeyPresent)
                return path;

            StringBuilder sb = new(path.Length + 64);
            sb.Append(path);

            foreach (KeyValuePair<string, Type> kvp in _commonTypeMap)
            {
                string shortName = kvp.Key;
                string fullName = kvp.Value.FullName ?? kvp.Value.Name;

                int startIndex = 0;
                while (startIndex < sb.Length)
                {
                    int idx = -1;
                    for (int i = startIndex; i <= sb.Length - shortName.Length; i++)
                    {
                        bool match = true;
                        for (int j = 0; j < shortName.Length; j++)
                        {
                            if (char.ToLowerInvariant(sb[i + j]) != char.ToLowerInvariant(shortName[j]))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx == -1)
                        break;

                    bool okBefore = (idx == 0) || Array.IndexOf(_identifierDelimiters, sb[idx - 1]) >= 0;
                    int afterIdx = idx + shortName.Length;
                    bool okAfter = (afterIdx >= sb.Length) || Array.IndexOf(_identifierDelimiters, sb[afterIdx]) >= 0;

                    if (okBefore && okAfter)
                    {
                        sb.Remove(idx, shortName.Length);
                        sb.Insert(idx, fullName);
                        startIndex = idx + fullName.Length;
                    }
                    else
                        startIndex = idx + shortName.Length;
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
                        if (TryResolveType(candidate, out Type? foundType))
                        {
                            current = foundType;
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

                    // Handle indexers and predicates
                    int openBracket = part.IndexOf('[');
                    if (openBracket >= 0)
                    {
                        int closeBracket = part.LastIndexOf(']');
                        if (closeBracket <= openBracket)
                            return null;

                        string baseName = part.Substring(0, openBracket).Trim();
                        string inner = part.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();

                        object? memberContainer = string.IsNullOrEmpty(baseName) ? current : GetMemberValueCached(current!, baseName);
                        if (memberContainer == null)
                            return null;

                        if (memberContainer is IEnumerable enumContainer and not string)
                        {
                            List<object?> list = enumContainer.Cast<object?>().ToList();

                            if (_collectionOperations.TryGetValue(inner, out var innerOp))
                            {
                                current = innerOp(list);
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
                    else
                    {
                        current = GetMemberValueCached(current!, part);
                        if (current == null)
                            return null;
                    }
                }

                return current;
            }
            catch
            {
                return null;
            }
        }

        private static object? GetMemberValueCached(object target, string memberName)
        {
            Type type = target.GetType();
            (Type, string) key = (type, memberName.ToLowerInvariant());

            if (CachedProperties.TryGetValue(key, out PropertyInfo? prop))
                return prop.GetValue(target);

            if (CachedFields.TryGetValue(key, out FieldInfo? field))
                return field.GetValue(target);

            if (CachedDelegates.TryGetValue(key, out Delegate? del))
                return del.DynamicInvoke(target);

            prop = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                CachedProperties[key] = prop;
                return prop.GetValue(target);
            }

            field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                CachedFields[key] = field;
                return field.GetValue(target);
            }

            MethodInfo? method = type.GetMethod(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, Type.EmptyTypes, null);
            if (method != null)
            {
                Type? funcType = typeof(Func<,>).MakeGenericType(type, method.ReturnType);
                Delegate? compiled = method.CreateDelegate(funcType);
                CachedDelegates[key] = compiled;

                return compiled.DynamicInvoke(target);
            }

            return null;
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

                if (item != null && _variables.TryGetValue(item, out var dict) && dict.TryGetValue(token, out var varValue))
                {
                    parameters.Add(varValue!);
                    continue;
                }

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

                switch (c)
                {
                    case '"' or '\'' when !inQuotes:
                        inQuotes = true;
                        quoteChar = c;
                        currentToken.Append(c);
                        break;

                    case var _ when inQuotes && c == quoteChar:
                        inQuotes = false;
                        currentToken.Append(c);
                        break;

                    case '(' when !inQuotes:
                        parenthesesDepth++;
                        currentToken.Append(c);
                        break;

                    case ')' when !inQuotes:
                        parenthesesDepth--;
                        currentToken.Append(c);
                        break;

                    case ',' when !inQuotes && parenthesesDepth == 0:
                        tokens.Add(currentToken.ToString().Trim());
                        currentToken.Clear();
                        break;

                    default:
                        currentToken.Append(c);
                        break;
                }
            }

            if (currentToken.Length > 0)
                tokens.Add(currentToken.ToString().Trim());

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

                    current = GetMemberValueCached(current, part);

                    if (current == null)
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

            if (TryHandleVariable(item, action, eventArgs))
                return;

            if (TryHandleDelayed(item, action, eventArgs))
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
                throw new ArgumentException(LogAndReturnWarn("Conditional statement must contain 'then' keyword"));

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
                thenPart = action.Substring(thenIndex + 6);

            result.ThenActions = thenPart.Split('&').Select(a => a.Trim()).ToArray();
            return result;
        }

        private static bool EvaluateCondition(string condition, EventArgs eventArgs)
        {
            try
            {
                // Note: condition should already have placeholders resolved when this is called
                return (condition, eventArgs) switch
                {
                    (string, EventArgs) c when condition.Contains(" contains ") => EvaluateConditions.EvaluateContainsCondition(condition, eventArgs),
                    (string, EventArgs) c when condition.Contains(" is ") => EvaluateConditions.EvaluateIsCondition(condition, eventArgs),
                    (string, EventArgs) c when condition.Contains(" equals ") => EvaluateConditions.EvaluateEqualsCondition(condition, eventArgs),
                    (string, EventArgs) c when condition.Contains(" > ") || condition.Contains(" < ") || condition.Contains(" >= ") || condition.Contains(" <= ") => EvaluateConditions.EvaluateNumericCondition(condition, eventArgs),
                    (string, EventArgs) c when condition.Contains(" != ") || condition.Contains(" == ") => EvaluateConditions.EvaluateEqualityCondition(condition, eventArgs),
                    _ => IsTruthy(condition),
                };
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(ArgumentManager)} Error evaluating condition '{condition}': {ex.Message}");
                return false;
            }
        }

        private static bool IsTruthy(string value)
        {
            return value switch
            {
                null or "" => false,
                "null" or "0" or "false" => false,
                string v when v.StartsWith("<error:") || v.StartsWith("<invalid:") => false,
                _ => true
            };
        }

        private static void HandleComplexAssignment(ICustomItem item, string propertyPath, string valueExpression, AssignmentOperator operatorType, EventArgs eventArgs)
        {
            try
            {
                object? resolvedValue = null;

                if (valueExpression.Contains('.') && !valueExpression.StartsWith("\"") && !valueExpression.StartsWith("'"))
                    resolvedValue = ResolveTargetObject(valueExpression, item, eventArgs);

                if (resolvedValue == null && valueExpression.Contains('(') && !valueExpression.StartsWith("\"") && !valueExpression.StartsWith("'"))
                    resolvedValue = ResolveTargetObject(valueExpression, item, eventArgs);

                if (resolvedValue == null)
                {
                    string resolvedValueExpression = ReplacePlaceholders(valueExpression, eventArgs);

                    if (!string.Equals(resolvedValueExpression, valueExpression, StringComparison.Ordinal))
                    {
                        if (resolvedValueExpression.Contains('.') || resolvedValueExpression.Contains('('))
                            resolvedValue = ResolveTargetObject(resolvedValueExpression, item, eventArgs);
                    }

                    if (resolvedValue == null)
                        resolvedValue = resolvedValueExpression;
                }

                object? currentValue = null;
                if (operatorType != AssignmentOperator.Assign)
                {
                    if (ShouldResolveFromEventArgs(propertyPath, eventArgs))
                    {
                        string currentValueStr = ResolvePlaceholder(propertyPath, eventArgs);
                        if (currentValueStr != "null" && !currentValueStr.StartsWith("<error:") && !currentValueStr.StartsWith("<invalid:"))
                            currentValue = currentValueStr;
                    }
                    else
                    {
                        object root = item != null ? (object)item : eventArgs;
                        currentValue = GetPropertyValue(root, propertyPath);
                    }
                }

                object? newValue;
                if (operatorType == AssignmentOperator.Assign)
                    newValue = resolvedValue;
                else
                {
                    string valueExprStr = resolvedValue?.ToString() ?? "null";
                    newValue = CalculateNewValue(currentValue, valueExprStr, operatorType, propertyPath);
                }

                if (ShouldResolveFromEventArgs(propertyPath, eventArgs))
                    SetProperty(eventArgs, propertyPath, newValue, eventArgs);
                else
                {
                    object root = item != null ? item : eventArgs;
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

            switch (action)
            {
                case string a when a.Contains("::"):
                    ExecuteMethodCall(item, action, eventArgs);
                    return;

                case string a when a.StartsWith("action ", StringComparison.OrdinalIgnoreCase) || a.StartsWith("execute ", StringComparison.OrdinalIgnoreCase) || a.StartsWith("run ", StringComparison.OrdinalIgnoreCase):
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
                    HandlePlaceholderPropertyAssignment(item, propertyPath, valueExpression, operatorType, eventArgs);
                else
                    HandleComplexAssignment(item, propertyPath, valueExpression, operatorType, eventArgs);

                return;
            }

            string resolvedAction = ReplacePlaceholders(action, eventArgs);

            string[] parts = resolvedAction.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string command = parts[0];
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : [];

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
                    ExecuteCustomAction(foundAction, eventArgs);
                else
                    LogManager.Error($"{nameof(ArgumentManager)}: CustomAction not found: {identifier}");
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
                    return (kvp.Value, index, kvp.Key.Length);
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
                    method = genericDefs.FirstOrDefault(m => 
                    {
                        ParameterInfo[]? methodParams = m.GetParameters();
                        int requiredParams = methodParams.Count(p => !p.HasDefaultValue);
                        return parameters.Length >= requiredParams && parameters.Length <= methodParams.Length;
                    });

                    if (method != null)
                        method = method.MakeGenericMethod(genericTypeArgs);
                }
                else
                {
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == parameters.Length);
                    
                    if (method == null)
                    {
                        method = methods.FirstOrDefault(m => 
                        {
                            ParameterInfo[]? methodParams = m.GetParameters();
                            int requiredParams = methodParams.Count(p => !p.HasDefaultValue);
                            return parameters.Length >= requiredParams && parameters.Length <= methodParams.Length;
                        });
                    }
                    
                    if (method == null)
                    {
                        Type[] parameterTypes = parameters.Select(p => p?.GetType() ?? typeof(string)).ToArray();
                        method = targetType.GetMethod(rawName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, parameterTypes, null);
                    }
                    
                    if (method == null)
                    {
                        method = methods.FirstOrDefault(m =>
                        {
                            ParameterInfo[] methodParams = m.GetParameters();
                            if (methodParams.Length < parameters.Length)
                                return false;

                            int requiredParams = methodParams.Count(p => !p.HasDefaultValue);
                            if (parameters.Length < requiredParams)
                                return false;
                            
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                Type expected = methodParams[i].ParameterType;
                                Type actual = parameters[i]?.GetType() ?? typeof(string);
                                
                                if (!expected.IsAssignableFrom(actual) && parameters[i] is not string)
                                    return false;
                            }

                            return true;
                        });
                    }
                }

                if (method == null)
                {
                    LogManager.Error($"Method '{methodName}' not found on type '{targetType.Name}' with {parameters.Length} parameters");
                    
                    LogManager.Debug($"Available methods named '{rawName}':");
                    foreach (MethodInfo? m in methods)
                    {
                        String? paramInfo = string.Join(", ", m.GetParameters().Select(p => 
                            $"{p.ParameterType.Name} {p.Name}" + (p.HasDefaultValue ? $" = {p.DefaultValue}" : "")));
                        LogManager.Debug($"  {m.Name}({paramInfo})");
                    }
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
                            convertedParams[i] = sVal;

                        else if (value is string sVal2)
                            convertedParams[i] = Convert.ChangeType(sVal2, expectedType);
                        else
                            convertedParams[i] = Convert.ChangeType(value, expectedType);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"Failed to convert parameter {i} for method '{methodName}': {ex.Message}");
                        convertedParams[i] = methodParams[i].HasDefaultValue ? methodParams[i].DefaultValue : null;
                    }
                }

                for (int i = parameters.Length; i < methodParams.Length; i++)
                {
                    convertedParams[i] = methodParams[i].HasDefaultValue ? methodParams[i].DefaultValue : null;
                }

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

        private static object? ResolveTargetObject(string objectPath, ICustomItem item, EventArgs eventArgs)
        {
            string originalPath = objectPath;
            LogManager.Debug($"Attempting to resolve: {originalPath}");

            LogManager.Debug($"EventArgs type: {eventArgs.GetType().Name}");
            LogManager.Debug($"EventArgs properties: {string.Join(", ", eventArgs.GetType().GetProperties().Select(p => p.Name))}");

            var availableProperties = GetEventArgsProperties(eventArgs);
            LogManager.Debug($"Available EventArgs properties: {string.Join(", ", availableProperties.Keys)}");

            string firstToken = originalPath.Split('.')[0];
            
            if (availableProperties.ContainsKey(firstToken) && originalPath == firstToken)
            {
                try
                {
                    PropertyInfo? propertyInfo = availableProperties[firstToken];
                    Object? directValue = propertyInfo.GetValue(eventArgs);
                    LogManager.Debug($"Direct property access for '{firstToken}': {directValue?.GetType()?.Name ?? "null"}");

                    if (directValue != null && directValue is not Type)
                    {
                        LogManager.Debug($"Resolved '{originalPath}' directly from EventArgs: {directValue.GetType().Name}");
                        return directValue;
                    }
                    else if (directValue is Type typeResult)
                        LogManager.Error($"Direct property '{firstToken}' returned a Type instead of an instance: {typeResult.Name}");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Error in direct property access for '{firstToken}': {ex.Message}");
                }

                object? resolvedFromArgs = ResolvePlaceholderToObject(originalPath, eventArgs);
                if (resolvedFromArgs != null)
                {
                    if (resolvedFromArgs is Type typeResult)
                    {
                        LogManager.Debug($"Got Type instead of instance for '{originalPath}': {typeResult.Name}");
                        foreach (var kvp in availableProperties)
                        {
                            try
                            {
                                object? propValue = kvp.Value.GetValue(eventArgs);
                                if (propValue != null && typeResult.IsAssignableFrom(propValue.GetType()))
                                {
                                    LogManager.Debug($"Found instance of {typeResult.Name} in property '{kvp.Key}': {propValue.GetType().Name}");
                                    return propValue;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogManager.Debug($"Error checking property '{kvp.Key}': {ex.Message}");
                            }
                        }
                        
                        LogManager.Error($"Property '{firstToken}' returned a Type instead of an instance, and no instance was found in other properties");
                        return null;
                    }
                    
                    LogManager.Debug($"Resolved '{originalPath}' from EventArgs: {resolvedFromArgs.GetType().Name}");
                    return resolvedFromArgs;
                }
                
                LogManager.Debug($"Expected to find '{firstToken}' in EventArgs but it wasn't found");
            }

            objectPath = ExpandCommonTypeNamesInPath(objectPath);
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

            int parenIndex = objectPath.IndexOf('(');
            if (parenIndex >= 0)
            {
                string firstTokenParen = objectPath.Substring(0, parenIndex).Split('.')[0].Trim();
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
                            suffix = remainder;

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

            if (!availableProperties.ContainsKey(firstToken))
            {
                if (TryResolveType(objectPath, out Type? type))
                {
                    LogManager.Debug($"Resolved '{objectPath}' as static type: {type.Name}");
                    return type;
                }
            }

            object? resolved = ResolvePlaceholderToObject(objectPath, eventArgs);
            if (resolved != null)
            {
                LogManager.Debug($"Resolved '{objectPath}' from EventArgs: {resolved.GetType().Name}");
                return resolved;
            }

            if (item != null)
            {
                resolved = ResolvePlaceholderToObject(objectPath, item);
                if (resolved != null)
                {
                    LogManager.Debug($"Resolved '{objectPath}' from CustomItem: {resolved.GetType().Name}");
                    return resolved;
                }
            }

            LogManager.Error($"Could not resolve object path: {objectPath}");
            return null;
        }

        private static Dictionary<string, PropertyInfo> GetEventArgsProperties(EventArgs eventArgs)
        {
            Dictionary<string, PropertyInfo> properties = new(StringComparer.OrdinalIgnoreCase);
            
            Type eventArgsType = eventArgs.GetType();
            PropertyInfo[] propertyInfos = eventArgsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (PropertyInfo prop in propertyInfos)
            {
                if (prop.GetIndexParameters().Length == 0)
                    properties[prop.Name] = prop;
            }
            
            return properties;
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
                if (string.IsNullOrEmpty(path) || root == null)
                    return null;

                object? current = root;
                string[] parts = path.Split('.');
                if (parts.Length == 0)
                    return null;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    if (current == null)
                        return null;

                    current = GetMemberValueCached(current, part);
                    if (current == null)
                    {
                        LogManager.Error($"Property, field, or method not found (or returned null) while resolving: {part} in path {path}");
                        return null;
                    }
                }

                if (current == null)
                    return null;

                string finalPart = parts[parts.Length - 1];
                Type finalType = current.GetType();
                (Type, string) key = (finalType, finalPart.ToLowerInvariant());

                CachedProperties.TryGetValue(key, out PropertyInfo? finalProp);
                CachedFields.TryGetValue(key, out FieldInfo? finalField);

                if (finalProp == null && finalField == null)
                {
                    finalProp = finalType.GetProperty(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (finalProp != null)
                        CachedProperties[key] = finalProp;
                    else
                    {
                        finalField = finalType.GetField(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (finalField != null)
                            CachedFields[key] = finalField;
                    }
                }

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
            if (customAction?.Actions == null || customAction.Actions.IsEmpty())
                return;

            LogManager.Debug($"{nameof(ArgumentManager)}: Executing CustomAction '{customAction.Name}' (ID: {customAction.Id})");

            foreach (string action in customAction.Actions)
            {
                if (!string.IsNullOrWhiteSpace(action))
                    ExecuteAction(null, action.Trim(), eventArgs);
            }
        }

        private static object? GetPropertyValue(object root, string propertyPath)
        {
            if (root == null)
                return null;

            if (string.IsNullOrEmpty(propertyPath))
                return root;

            object? current = root;
            string[] parts = propertyPath.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                if (current == null)
                    return null;

                string part = parts[i];
                current = GetMemberValueCached(current, part);
                if (current == null)
                    throw new ArgumentException(LogAndReturnWarn($"Property or field not found: {part}"));
            }

            return current;
        }

        private static object? CalculateNewValue(object? currentValue, string valueExpression, AssignmentOperator operatorType, string propertyPath)
        {
            return operatorType switch
            {
                AssignmentOperator.Assign => valueExpression,
                AssignmentOperator.Add when currentValue is string currentStr => currentStr + valueExpression,
                AssignmentOperator.Add or 
                AssignmentOperator.Subtract or 
                AssignmentOperator.Multiply or 
                AssignmentOperator.Divide or 
                AssignmentOperator.Modulo => CalculateNumericValue(currentValue, valueExpression, operatorType),
                AssignmentOperator.BitwiseAnd or 
                AssignmentOperator.BitwiseOr or 
                AssignmentOperator.BitwiseXor => CalculateBitwiseValue(currentValue, valueExpression, operatorType),
                _ => throw new InvalidOperationException(LogAndReturnWarn($"Unsupported operator '{operatorType}' for property '{propertyPath}'."))
            };
        }

        private static string LogAndReturnWarn(string msg)
        {
            LogManager.Warn(msg);
            return msg;
        }

        private static string LogAndReturnError(string msg)
        {
            LogManager.Error(msg);
            return msg;
        }

        private static bool IsNumericOperation(AssignmentOperator operatorType) =>
            operatorType is AssignmentOperator.Add or AssignmentOperator.Subtract or AssignmentOperator.Multiply or AssignmentOperator.Divide or AssignmentOperator.Modulo;


        private static bool IsBitwiseOperation(AssignmentOperator operatorType) =>
            operatorType is AssignmentOperator.BitwiseAnd or AssignmentOperator.BitwiseOr or AssignmentOperator.BitwiseXor;

        private static object? CalculateNumericValue(object? currentValue, string valueExpression, AssignmentOperator operatorType)
        {
            if (currentValue == null)
                throw new ArgumentException(LogAndReturnError($"Cannot perform numeric operation on null value"));

            if (!TryConvertToDouble(currentValue, out double current))
                throw new ArgumentException(LogAndReturnError($"Cannot convert current value to number: {currentValue}"));

            if (!double.TryParse(valueExpression, out double operand))
                throw new ArgumentException(LogAndReturnError($"Cannot convert operand to number: {valueExpression}"));

            double result = operatorType switch
            {
                AssignmentOperator.Add => current + operand,
                AssignmentOperator.Subtract => current - operand,
                AssignmentOperator.Multiply => current * operand,
                AssignmentOperator.Divide => operand != 0 ? current / operand : throw new DivideByZeroException(LogAndReturnWarn("Cannot divide by zero!")),
                AssignmentOperator.Modulo => operand != 0 ? current % operand : throw new DivideByZeroException(LogAndReturnWarn("Cannot divide by zero!")),
                _ => throw new NotSupportedException(LogAndReturnWarn($"Numeric operator {operatorType} not supported"))
            };

            return ConvertToOriginalType(result, currentValue.GetType());
        }

        private static object? CalculateBitwiseValue(object? currentValue, string valueExpression, AssignmentOperator operatorType)
        {
            if (currentValue == null)
                throw new ArgumentException(LogAndReturnWarn("Cannot perform bitwise operation on null value"));

            if (!TryConvertToLong(currentValue, out long current))
                throw new ArgumentException(LogAndReturnError($"Cannot convert current value to integer: {currentValue}"));

            if (!long.TryParse(valueExpression, out long operand))
                throw new ArgumentException(LogAndReturnError($"Cannot convert operand to integer: {valueExpression}"));

            long result = operatorType switch
            {
                AssignmentOperator.BitwiseAnd => current & operand,
                AssignmentOperator.BitwiseOr => current | operand,
                AssignmentOperator.BitwiseXor => current ^ operand,
                _ => throw new NotSupportedException(LogAndReturnWarn($"Bitwise operator {operatorType} not supported"))
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
                return targetType switch
                {
                    Type t when t == typeof(int) || t == typeof(int?) => Convert.ToInt32(value),
                    Type t when t == typeof(long) || t == typeof(long?) => Convert.ToInt64(value),
                    Type t when t == typeof(float) || t == typeof(float?) => Convert.ToSingle(value),
                    Type t when t == typeof(double) || t == typeof(double?) => Convert.ToDouble(value),
                    Type t when t == typeof(decimal) || t == typeof(decimal?) => Convert.ToDecimal(value),
                    Type t when t == typeof(byte) || t == typeof(byte?) => Convert.ToByte(value),
                    Type t when t == typeof(short) || t == typeof(short?) => Convert.ToInt16(value),
                    _ => value
                };
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
                        target = prop.GetValue(target) ?? throw new NullReferenceException(LogAndReturnError($"Property {part} is null"));
                        continue;
                    }

                    FieldInfo? field = target.GetType().GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                    {
                        target = field.GetValue(target) ?? throw new NullReferenceException(LogAndReturnError($"Field {part} is null"));
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

        private static object? TryConvertOrDefault(object? value, Type targetType, Type expected, ParameterInfo param)
        {
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                if (value != null && expected.IsAssignableFrom(value.GetType()))
                    return value;
                
                LogManager.Error($"Failed to convert constructor parameter from '{value?.GetType().Name}' to '{expected.Name}'");
                return param.HasDefaultValue ? param.DefaultValue : (expected.IsValueType ? Activator.CreateInstance(expected) : null);
            }
        }

        private static object? ConvertStringParameter(string stringValue, Type expected, ParameterInfo param)
        {
            try
            {
                return Convert.ChangeType(stringValue, expected);
            }
            catch
            {
                LogManager.Error($"Failed to convert string parameter to '{expected.Name}'");
                return param.HasDefaultValue ? param.DefaultValue : (expected.IsValueType ? Activator.CreateInstance(expected) : null);
            }
        }

        private static object?[] ConvertConstructorParameters(ParameterInfo[] ctorParams, object[] parameters)
        {
            object[] converted = new object[ctorParams.Length];

            for (int i = 0; i < ctorParams.Length; i++)
            {
                Type expected = ctorParams[i].ParameterType;
                object? value = parameters[i];

#pragma warning disable CS8601 // Possible null reference assignment.
                converted[i] = (value, expected) switch
                {
                    (null, _) => ctorParams[i].HasDefaultValue
                        ? ctorParams[i].DefaultValue
                        : (expected.IsValueType ? Activator.CreateInstance(expected) : null),

                    var (v, e) when e.IsAssignableFrom(v.GetType()) => v,

                    (string sVal, _) when (sVal.StartsWith("\"") && sVal.EndsWith("\"")) ||
                                        (sVal.StartsWith("'") && sVal.EndsWith("'"))
                        => ConvertStringParameter(sVal.Substring(1, sVal.Length - 2), expected, ctorParams[i]),

                    (_, Type t) when t == typeof(float) || t == typeof(float?) => TryConvertOrDefault(value, typeof(float), expected, ctorParams[i]),
                    (_, Type t) when t == typeof(double) || t == typeof(double?) => TryConvertOrDefault(value, typeof(double), expected, ctorParams[i]),
                    (_, Type t) when t == typeof(int) || t == typeof(int?) => TryConvertOrDefault(value, typeof(int), expected, ctorParams[i]),

                    _ => TryConvertOrDefault(value, expected, expected, ctorParams[i])
                };
#pragma warning restore CS8601 // Possible null reference assignment.
            }

            return converted;
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
                    }).All(b => b)) ?? ctors.FirstOrDefault(c => c.GetParameters().Length == parameters.Length);
                if (ctor == null)
                {
                    LogManager.Error($"No matching constructor found on type '{type.FullName}' with {parameters.Length} parameters");
                    return null;
                }

                ParameterInfo[] ctorParams = ctor.GetParameters();
                object[] converted = ConvertConstructorParameters(ctorParams, parameters);

                return ctor.Invoke(converted);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to create instance of type '{type.FullName}': {ex.Message}");
                return null;
            }
        }

        private static bool TryHandleVariable(ICustomItem item, string action, EventArgs eventArgs)
        {
            if (!action.StartsWith("let ", StringComparison.OrdinalIgnoreCase))
                return false;

            string[]? parts = action.Substring(4).Split('=', (char)2);
            if (parts.Length != 2)
                return true;

            string varName = parts[0].Trim();
            string expr = parts[1].Trim();

            object? value = ResolveTargetObject(expr, item, eventArgs) ?? ReplacePlaceholders(expr, eventArgs);

            if (!_variables.TryGetValue(item, out var dict))
                dict = _variables[item] = [];

            dict[varName] = value;
            return true;
        }

        private static bool TryHandleDelayed(ICustomItem item, string action, EventArgs eventArgs)
        {
            if (!action.StartsWith("after ", StringComparison.OrdinalIgnoreCase))
                return false;

            string[] parts = action.Split([" then "], StringSplitOptions.None);
            if (parts.Length != 2) 
                return true;

            string delayPart = parts[0].Substring(6).Trim();
            string delayedAction = parts[1].Trim();

            float seconds = ParseTime(delayPart);
            Timing.CallDelayed(seconds, () => ExecuteAction(item, delayedAction, eventArgs));
            return true;
        }

        private static float ParseTime(string input)
        {
            if (input.EndsWith("ms", StringComparison.OrdinalIgnoreCase))
                return float.Parse(input.Substring(0, input.Length - 2)) / 1000f;
            
            if (input.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                return float.Parse(input.Substring(0, input.Length - 1));

            if (input.EndsWith("m", StringComparison.OrdinalIgnoreCase))
                return float.Parse(input.Substring(0, input.Length - 1)) * 60f;

            return float.Parse(input);
        }
    }
}