using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using MEC;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.ArgumentHelpers;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features.Manager
{
    /// <summary>
    /// Manages the action system for <see cref="CustomItem"/>s
    /// </summary>
    public static class ArgumentManager
    {
        private static readonly Dictionary<string, Type> _commonTypeMap = BuildCommonTypeMap();

        internal static readonly ConcurrentDictionary<string, Action<ICustomItem, string[]>> _actionHandlers = new(StringComparer.OrdinalIgnoreCase);

        internal static readonly ConcurrentDictionary<Type, HashSet<string>> _eventArgPropertyCache = new();

        private static readonly ConcurrentDictionary<string, Type?> _typeResolutionCache = new(StringComparer.OrdinalIgnoreCase);

        internal static ConcurrentDictionary<(Type, string), Delegate> CachedDelegates { get; } = new();

        internal static readonly ConcurrentDictionary<ICustomItem, ConcurrentDictionary<string, object?>> _variables = new();

        private static readonly char[] _identifierDelimiters = ['.', '(', ')', '[', ']', ',', ' ', '\t'];

        private static readonly Random _random = new();

        internal static ConcurrentDictionary<(Type, string), PropertyInfo> CachedProperties { get; } = new();
        
        internal static ConcurrentDictionary<(Type, string), FieldInfo> CachedFields { get; } = new();

        internal static ConcurrentDictionary<(Type, string), MethodInfo> CachedMethods { get; } = new();

        private static readonly AsyncLocal<int> _executeActionDepth = new();
        
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
            if (customItem?.Arguments == null || customItem.Arguments.Count == 0)
                return;

            if (!customItem.Arguments.TryGetValue(type, out string? actionString) || string.IsNullOrWhiteSpace(actionString))
                return;

            LogManager.Debug($"Executing action: {actionString}");

            foreach (string raw in actionString.Split(['\n', ';'], StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = raw.Trim();
                if (trimmed.Length > 0)
                    ExecuteAction(customItem, trimmed, eventArgs);
            }
        }

        private static Dictionary<string, Type> BuildCommonTypeMap()
        {
            Dictionary<string, Type> map = new(StringComparer.OrdinalIgnoreCase);

            try
            {
                Type enumType = typeof(CommonTypes);
                foreach (FieldInfo field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    CommonTypesAttribute? attr = field.GetCustomAttribute<CommonTypesAttribute>();
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

        internal static string ReplacePlaceholders(string action, EventArgs args, ICustomItem? item = null)
        {
            if (string.IsNullOrEmpty(action) || action.IndexOf('{') == -1)
                return action;

            StringBuilder sb = new(action.Length + 32);
            int lastIndex = 0;

            while (true)
            {
                int start = action.IndexOf('{', lastIndex);
                if (start == -1)
                    break;

                int end = action.IndexOf('}', start);
                if (end == -1)
                    break;

                sb.Append(action, lastIndex, start - lastIndex);

                string placeholder = action.Substring(start + 1, end - start - 1);

                if (item != null && _variables.TryGetValue(item, out var dict) && dict.TryGetValue(placeholder, out var varVal))
                {
                    sb.Append(varVal?.ToString() ?? "null");
                }
                else
                {
                    sb.Append(ResolvePlaceholder(placeholder, args));
                }

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
                return false;

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
                _typeResolutionCache[fullName] = common2;
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
                        _typeResolutionCache[fullName] = t;
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
                        _typeResolutionCache[fullName] = t;
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

        internal static object? ResolvePlaceholderToObject(string path, object? root)
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

                    int openBracket = part.IndexOf('[');
                    if (openBracket >= 0)
                    {
                        int closeBracket = part.LastIndexOf(']');
                        if (closeBracket <= openBracket)
                            return null;

                        string baseName = part.Substring(0, openBracket).Trim();
                        string inner = part.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();

                        object? memberContainer = string.IsNullOrEmpty(baseName) ? current : GetMemberValueCached(current, baseName);
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

                            if (int.TryParse(inner, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx))
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
                        current = GetMemberValueCached(current, part);
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

        private static object? GetMemberValueCached(object? target, string memberName)
        {
            if (target == null)
                return null;

            Type type = target is Type tTarget ? tTarget : target.GetType();
            (Type, string) key = (type, memberName.ToLowerInvariant());

            if (CachedProperties.TryGetValue(key, out PropertyInfo? prop))
                return prop.GetValue(target is Type ? null : target);

            if (CachedFields.TryGetValue(key, out FieldInfo? field))
                return field.GetValue(target is Type ? null : target);

            if (CachedDelegates.TryGetValue(key, out Delegate? del))
                return del.DynamicInvoke(target);

            BindingFlags flags = BindingFlags.Public | BindingFlags.IgnoreCase | (target is Type ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static);

            prop = type.GetProperty(memberName, flags);
            if (prop != null)
            {
                CachedProperties[key] = prop;
                return prop.GetValue(target is Type ? null : target);
            }

            field = type.GetField(memberName, flags);
            if (field != null)
            {
                CachedFields[key] = field;
                return field.GetValue(target is Type ? null : target);
            }

            MethodInfo? method = type.GetMethod(memberName, flags, null, Type.EmptyTypes, null);
            if (method != null)
            {
                if (target is not Type)
                {
                    Type funcType = typeof(Func<,>).MakeGenericType(type, method.ReturnType);
                    Delegate compiled = method.CreateDelegate(funcType);
                    CachedDelegates[key] = compiled;
                    return compiled.DynamicInvoke(target);
                }
                return method.Invoke(null, null);
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

        private static object?[] ParseAndResolveParameters(string parametersPart, EventArgs eventArgs, ICustomItem? item)
        {
            if (string.IsNullOrWhiteSpace(parametersPart))
                return [];

            List<object?> parameters = [];
            List<string> paramTokens = ParseParameterTokens(parametersPart);

            foreach (string token in paramTokens)
            {
                string replaced = ExpandCommonTypeNamesInPath(ReplacePlaceholders(token, eventArgs, item));

                if (item != null && _variables.TryGetValue(item, out var dict) && dict.TryGetValue(token, out var varValue))
                {
                    parameters.Add(varValue);
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
                if (byte.TryParse(replaced, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte by))
                {
                    parameters.Add(by);
                    continue;
                }
                if (int.TryParse(replaced, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                {
                    parameters.Add(i);
                    continue;
                }
                if (long.TryParse(replaced, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
                {
                    parameters.Add(l);
                    continue;
                }
                if (float.TryParse(replaced, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                {
                    parameters.Add(f);
                    continue;
                }
                if (double.TryParse(replaced, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
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

        private static string ResolvePlaceholder(string path, object? root)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || root == null)
                    return "null";

                object? obj = ResolvePlaceholderToObject(path, root);
                if (obj != null)
                    return obj.ToString() ?? "null";

                return "null";
            }
            catch (Exception ex)
            {
                return $"<error:{ex.Message}>";
            }
        }

        private static void ExecuteAction(ICustomItem? item, string action, EventArgs eventArgs)
        {
            _executeActionDepth.Value = _executeActionDepth.Value + 1;
            if (_executeActionDepth.Value > Plugin.Instance.Config.MaxActionsExecutionDepth)
            {
                LogManager.Error($"Potential recursion/loop detected executing action '{action}' (depth > {Plugin.Instance.Config.MaxActionsExecutionDepth}). Aborting to avoid infinite loop. You can increase this limit in the plugin config.");
                _executeActionDepth.Value = 0;
                return;
            }

            try
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
            finally
            {
                _executeActionDepth.Value = Math.Max(0, _executeActionDepth.Value - 1);
            }
        }

        private static bool IsConditionalStatement(string action)
        {
            string trimmed = action.TrimStart();
            return trimmed.StartsWith("if ", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("unless ", StringComparison.OrdinalIgnoreCase);
        }

        private static void ExecuteConditional(ICustomItem? item, string action, EventArgs eventArgs)
        {
            try
            {
                ConditionalParts parts = ParseConditionalStatement(action);

                string resolvedCondition = ReplacePlaceholders(parts.Condition, eventArgs, item);
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

        private static bool IsTruthy(string? value)
        {
            return value switch
            {
                null or "" => false,
                "null" or "0" or "false" => false,
                string v when v.StartsWith("<error:") || v.StartsWith("<invalid:") => false,
                _ => true
            };
        }

        private static void HandleComplexAssignment(ICustomItem? item, string propertyPath, string valueExpression, AssignmentOperator operatorType, EventArgs eventArgs)
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
                    string resolvedValueExpression = ReplacePlaceholders(valueExpression, eventArgs, item);

                    if (!string.Equals(resolvedValueExpression, valueExpression, StringComparison.Ordinal))
                    {
                        if (resolvedValueExpression.Contains('.') || resolvedValueExpression.Contains('('))
                            resolvedValue = ResolveTargetObject(resolvedValueExpression, item, eventArgs);
                    }

                    resolvedValue ??= resolvedValueExpression;
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
                    object root = item != null ? item : eventArgs;
                    SetProperty(root, propertyPath, newValue, eventArgs);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to handle complex assignment '{propertyPath} = {valueExpression}': {ex.Message}");
            }
        }

        private static void ExecuteRegularAction(ICustomItem? item, string action, EventArgs eventArgs)
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

            (AssignmentOperator operatorType, int index, int length)? operatorInfo = FindAssignmentOperator(action);
            if (operatorInfo.HasValue)
            {
                (AssignmentOperator operatorType, int operatorIndex, int operatorLength) = operatorInfo.Value;

                string propertyPath = action.Substring(0, operatorIndex).Trim();
                string valueExpression = action.Substring(operatorIndex + operatorLength).Trim();

                if (propertyPath.Contains('{') && propertyPath.Contains('}'))
                    HandlePlaceholderPropertyAssignment(item, propertyPath, valueExpression, operatorType, eventArgs);
                else
                    HandleComplexAssignment(item, propertyPath, valueExpression, operatorType, eventArgs);

                return;
            }

            string resolvedAction = ReplacePlaceholders(action, eventArgs, item);

            string[] parts = resolvedAction.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string command = parts[0];
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : [];

            if (_actionHandlers.TryGetValue(command, out var handler))
                handler(item!, args);
            else
                LogManager.Error($"{nameof(ArgumentManager)} Unknown action: {command}");
        }

        private static void HandleCustomActionExecution(string action, EventArgs eventArgs)
        {
            try
            {
                string[] parts = action.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    LogManager.Error($"{nameof(ArgumentManager)}: Invalid action command format: {action}");
                    return;
                }

                string identifier = string.Join(" ", parts.Skip(1));
                identifier = ReplacePlaceholders(identifier, eventArgs);

                if (uint.TryParse(identifier, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint actionId))
                {
                    ExecuteCustomAction(actionId, eventArgs);
                    return;
                }

                ICustomAction? foundAction = CustomAction.List.FirstOrDefault(a => string.Equals(a.Name, identifier, StringComparison.OrdinalIgnoreCase));

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
            if (string.IsNullOrEmpty(action))
                return null;

            int bestIndex = -1;
            int bestLength = 0;
            AssignmentOperator bestType = AssignmentOperator.Assign;

            foreach (KeyValuePair<string, AssignmentOperator> kvp in _compoundOperators)
            {
                int index = action.IndexOf(kvp.Key, StringComparison.Ordinal);
                if (index <= 0)
                    continue;

                if (kvp.Value == AssignmentOperator.Assign)
                {
                    char prev = action[index - 1];
                    char next = (index + 1 < action.Length) ? action[index + 1] : '\0';

                    if (prev == '=' || prev == '!' || prev == '<' || prev == '>' || next == '=')
                        continue;
                }

                if (bestIndex == -1 || index < bestIndex || (index == bestIndex && kvp.Key.Length > bestLength))
                {
                    bestIndex = index;
                    bestLength = kvp.Key.Length;
                    bestType = kvp.Value;
                }
            }

            return bestIndex == -1 ? null : (bestType, bestIndex, bestLength);
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

        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsAssignableFrom(value.GetType()))
                return value;

            if (underlying.IsEnum)
            {
                if (value is string strEnum)
                    return Enum.Parse(underlying, strEnum, ignoreCase: true);

                return Enum.ToObject(underlying, Convert.ChangeType(value, Enum.GetUnderlyingType(underlying), CultureInfo.InvariantCulture));
            }

            if (value is string strVal)
            {
                if (underlying == typeof(bool))
                    return bool.Parse(strVal);
                if (underlying == typeof(Guid))
                    return Guid.Parse(strVal);

                return Convert.ChangeType(strVal, underlying, CultureInfo.InvariantCulture);
            }

            return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
        }

        private static void ExecuteMethod(object targetObject, string methodName, object?[] parameters)
        {
            try
            {
                Type targetType = targetObject is Type tTarget ? tTarget : targetObject.GetType();
                string rawName = methodName;
                Type[]? genericTypeArgs = null;

                int genOpen = methodName.IndexOf('[');
                int genClose = methodName.LastIndexOf(']');
                if (genOpen >= 0 && genClose > genOpen)
                {
                    string genSpec = methodName.Substring(genOpen + 1, genClose - genOpen - 1);
                    rawName = methodName.Substring(0, genOpen);

                    string[] genTypeNames = genSpec.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
                    List<Type> genTypes = [];
                    foreach (string tname in genTypeNames)
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

                BindingFlags flags = BindingFlags.Public | BindingFlags.IgnoreCase | (targetObject is Type ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static);

                MethodInfo[] methods = targetType.GetMethods(flags)
                                .Where(m => string.Equals(m.Name, rawName, StringComparison.OrdinalIgnoreCase))
                                .ToArray();

                MethodInfo? method = null;
                if (genericTypeArgs != null)
                {
                    MethodInfo[] genericDefs = methods.Where(m => m.IsGenericMethodDefinition && m.GetGenericArguments().Length == genericTypeArgs.Length).ToArray();
                    method = genericDefs.FirstOrDefault(m => 
                    {
                        ParameterInfo[] methodParams = m.GetParameters();
                        int requiredParams = methodParams.Count(p => !p.HasDefaultValue);
                        return parameters.Length >= requiredParams && parameters.Length <= methodParams.Length;
                    });

                    if (method != null)
                        method = method.MakeGenericMethod(genericTypeArgs);
                }
                else
                {
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == parameters.Length) ?? methods.FirstOrDefault(m => 
                    {
                        ParameterInfo[] methodParams = m.GetParameters();
                        return parameters.Length >= methodParams.Count(p => !p.HasDefaultValue) && parameters.Length <= methodParams.Length;
                    });

                    if (method == null)
                    {
                        Type[] parameterTypes = parameters.Select(p => p?.GetType() ?? typeof(string)).ToArray();
                        method = targetType.GetMethod(rawName, flags, null, parameterTypes, null);
                    }
                }

                if (method == null)
                {
                    LogManager.Error($"Method '{methodName}' not found on type '{targetType.Name}' with {parameters.Length} parameters");
                    return;
                }

                ParameterInfo[] finalMethodParams = method.GetParameters();
                object?[] convertedParams = new object?[finalMethodParams.Length];

                for (int i = 0; i < finalMethodParams.Length; i++)
                {
                    if (i < parameters.Length)
                    {
                        try
                        {
                            convertedParams[i] = ConvertValue(parameters[i], finalMethodParams[i].ParameterType);
                        }
                        catch (Exception ex)
                        {
                            LogManager.Error($"Failed to convert parameter {i} for method '{methodName}': {ex.Message}");
                            convertedParams[i] = finalMethodParams[i].HasDefaultValue ? finalMethodParams[i].DefaultValue : null;
                        }
                    }
                    else
                    {
                        convertedParams[i] = finalMethodParams[i].HasDefaultValue ? finalMethodParams[i].DefaultValue : null;
                    }
                }

                object? instance = targetObject is Type ? null : targetObject;
                object? result = method.Invoke(instance, convertedParams);
                LogManager.Debug($"Successfully executed method '{methodName}' on {targetType.Name}. Result: {result?.ToString() ?? "null"}");
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to execute method '{methodName}': {ex.Message}");
            }
        }
        
        private static void ExecuteMethodCall(ICustomItem? item, string methodCall, EventArgs eventArgs)
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

                object?[] parameters = ParseAndResolveParameters(parametersPart, eventArgs, item);

                ExecuteMethod(targetObject, methodName, parameters);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error executing method call '{methodCall}': {ex.Message}");
            }
        }

        private static object? ResolveTargetObject(string objectPath, ICustomItem? item, EventArgs eventArgs)
        {
            string firstToken = objectPath.Split('.')[0].Trim();

            if (item != null && _variables.TryGetValue(item, out var varDict))
            {
                if (varDict.TryGetValue(firstToken, out object? varValue))
                {
                    if (objectPath.Length == firstToken.Length)
                        return varValue;

                    return ResolvePlaceholderToObject(objectPath.Substring(firstToken.Length + 1), varValue);
                }
            }

            string originalPath = objectPath;
            Dictionary<string, PropertyInfo> availableProperties = GetEventArgsProperties(eventArgs);

            if (availableProperties.ContainsKey(firstToken) && originalPath == firstToken)
            {
                try
                {
                    PropertyInfo propertyInfo = availableProperties[firstToken];
                    object? directValue = propertyInfo.GetValue(eventArgs);

                    if (directValue != null && directValue is not Type)
                        return directValue;
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
                        foreach (KeyValuePair<string, PropertyInfo> kvp in availableProperties)
                        {
                            try
                            {
                                object? propValue = kvp.Value.GetValue(eventArgs);
                                if (propValue != null && typeResult.IsAssignableFrom(propValue.GetType()))
                                    return propValue;
                            }
                            catch
                            {
                            }
                        }
                        return null;
                    }
                    
                    return resolvedFromArgs;
                }
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
                int closeParen = FindClosingParen(objectPath, parenIndex);
                if (closeParen > parenIndex)
                {
                    string beforeParen = objectPath.Substring(0, parenIndex).Trim();
                    string paramsPart = objectPath.Substring(parenIndex + 1, closeParen - parenIndex - 1);
                    string remainder = objectPath.Length > closeParen + 1 ? objectPath.Substring(closeParen + 1) : "";

                    string[] nameParts = beforeParen.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
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
                    return type;
            }

            object? resolved = ResolvePlaceholderToObject(objectPath, eventArgs);
            if (resolved != null)
                return resolved;

            if (item != null)
            {
                resolved = ResolvePlaceholderToObject(objectPath, item);
                if (resolved != null)
                    return resolved;
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

        private static void HandlePlaceholderPropertyAssignment(ICustomItem? item, string propertyPathPlaceholder, string valueExpression, AssignmentOperator operatorType, EventArgs eventArgs)
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

                string resolvedValueExpression = ReplacePlaceholders(valueExpression, eventArgs, item);

                object? newValue = CalculateNewValue(currentValue, resolvedValueExpression, operatorType, placeholderContent);

                propertyTarget.SetValue(newValue);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to handle placeholder property assignment '{propertyPathPlaceholder}': {ex.Message}");
            }
        }

        private static PropertyTarget? ResolvePropertyTarget(string path, object? root)
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
                Type finalType = current is Type t ? t : current.GetType();
                (Type, string) key = (finalType, finalPart.ToLowerInvariant());

                CachedProperties.TryGetValue(key, out PropertyInfo? finalProp);
                CachedFields.TryGetValue(key, out FieldInfo? finalField);

                BindingFlags flags = BindingFlags.Public | BindingFlags.IgnoreCase | (current is Type ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static);

                if (finalProp == null && finalField == null)
                {
                    finalProp = finalType.GetProperty(finalPart, flags);
                    if (finalProp != null)
                        CachedProperties[key] = finalProp;
                    else
                    {
                        finalField = finalType.GetField(finalPart, flags);
                        if (finalField != null)
                            CachedFields[key] = finalField;
                    }
                }

                if (finalProp != null || finalField != null)
                {
                    return new PropertyTarget
                    {
                        Target = current is Type ? null : current,
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
            if (!CustomAction.CustomActions.TryGetValue(actionId, out ICustomAction? customAction))
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
        public static void ExecuteCustomAction(ICustomAction? customAction, EventArgs eventArgs)
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

        private static object? GetPropertyValue(object? root, string propertyPath)
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
                current = GetMemberValueCached(current, part) ?? throw new ArgumentException(LogAndReturnWarn($"Property or field not found: {part}"));
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

        private static object? CalculateNumericValue(object? currentValue, string valueExpression, AssignmentOperator operatorType)
        {
            if (currentValue == null)
                throw new ArgumentException(LogAndReturnError($"Cannot perform numeric operation on null value"));

            if (!TryConvertToDouble(currentValue, out double current))
                throw new ArgumentException(LogAndReturnError($"Cannot convert current value to number: {currentValue}"));

            if (!double.TryParse(valueExpression, NumberStyles.Float, CultureInfo.InvariantCulture, out double operand))
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

            if (!long.TryParse(valueExpression, NumberStyles.Integer, CultureInfo.InvariantCulture, out long operand))
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
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
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
                result = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static object? ConvertToOriginalType(object? value, Type targetType)
        {
            try
            {
                Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
                return underlying switch
                {
                    Type t when t == typeof(int) => Convert.ToInt32(value, CultureInfo.InvariantCulture),
                    Type t when t == typeof(long) => Convert.ToInt64(value, CultureInfo.InvariantCulture),
                    Type t when t == typeof(float) => Convert.ToSingle(value, CultureInfo.InvariantCulture),
                    Type t when t == typeof(double) => Convert.ToDouble(value, CultureInfo.InvariantCulture),
                    Type t when t == typeof(decimal) => Convert.ToDecimal(value, CultureInfo.InvariantCulture),
                    Type t when t == typeof(byte) => Convert.ToByte(value, CultureInfo.InvariantCulture),
                    Type t when t == typeof(short) => Convert.ToInt16(value, CultureInfo.InvariantCulture),
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
                    object? convertedValue = ConvertValue(newValue, finalProp.PropertyType);
                    finalProp.SetValue(target, convertedValue);
                    return;
                }

                FieldInfo? finalField = target.GetType().GetField(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (finalField != null)
                {
                    object? convertedValue = ConvertValue(newValue, finalField.FieldType);
                    finalField.SetValue(target, convertedValue);
                    return;
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
                {
                    depth++;
                }
                else if (s[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }

            return -1;
        }

        private static object?[] ConvertConstructorParameters(ParameterInfo[] ctorParams, object?[] parameters)
        {
            object?[] converted = new object?[ctorParams.Length];

            for (int i = 0; i < ctorParams.Length; i++)
            {
                Type expected = ctorParams[i].ParameterType;
                object? value = i < parameters.Length ? parameters[i] : null;

                try
                {
                    converted[i] = ConvertValue(value, expected);
                }
                catch
                {
                    converted[i] = ctorParams[i].HasDefaultValue ? ctorParams[i].DefaultValue : (expected.IsValueType ? Activator.CreateInstance(expected) : null);
                }
            }

            return converted;
        }

        private static object? CreateInstanceFromType(Type type, string parametersPart, EventArgs eventArgs, ICustomItem? item)
        {
            try
            {
                object?[] parameters = ParseAndResolveParameters(parametersPart, eventArgs, item);
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
                object?[] converted = ConvertConstructorParameters(ctorParams, parameters);

                return ctor.Invoke(converted);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to create instance of type '{type.FullName}': {ex.Message}");
                return null;
            }
        }

        private static bool TryHandleVariable(ICustomItem? item, string action, EventArgs eventArgs)
        {
            if (!action.StartsWith("var ", StringComparison.OrdinalIgnoreCase) && !action.StartsWith("var\t", StringComparison.OrdinalIgnoreCase))
                return false;

            if (item == null)
            {
                LogManager.Warn("'var' variable used outside of a CustomItem context. ignored.");
                return true;
            }

            string[] parts = action.Substring(4).Split(['='], 2);
            if (parts.Length != 2)
                return true;

            string varName = parts[0].Trim();
            string expr = parts[1].Trim();

            object? value = ResolveTargetObject(expr, item, eventArgs) ?? (object)ReplacePlaceholders(expr, eventArgs, item);

            ConcurrentDictionary<string, object?> dict = _variables.GetOrAdd(item, _ => new ConcurrentDictionary<string, object?>(StringComparer.OrdinalIgnoreCase));

            dict[varName] = value;
            LogManager.Debug($"Variable '{varName}' set to: {value?.ToString() ?? "null"}");
            return true;
        }

        private static bool TryHandleDelayed(ICustomItem? item, string action, EventArgs eventArgs)
        {
            if (!action.StartsWith("after ", StringComparison.OrdinalIgnoreCase))
                return false;

            string[] parts = action.Split([" then "], 2, StringSplitOptions.None);
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
            input = input.Trim();
            if (input.EndsWith("ms", StringComparison.OrdinalIgnoreCase))
            {
                if (float.TryParse(input.Substring(0, input.Length - 2).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float ms))
                    return ms / 1000f;
            }
            else if (input.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                if (float.TryParse(input.Substring(0, input.Length - 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float s))
                    return s;
            }
            else if (input.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                if (float.TryParse(input.Substring(0, input.Length - 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float m))
                    return m * 60f;
            }

            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float sec))
                return sec;

            return 0f;
        }
    }
}