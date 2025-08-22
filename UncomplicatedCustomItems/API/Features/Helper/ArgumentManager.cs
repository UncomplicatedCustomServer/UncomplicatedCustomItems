using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features.Helper
{
#nullable enable
    public static class ArgumentManager
    {
        private class ConditionalParts
        {
            public bool IsUnless { get; set; }
            public string Condition { get; set; } = string.Empty;
            public string[] ThenActions { get; set; } = [];
            public string[]? ElseActions { get; set; }
        }

        private class PropertyTarget
        {
            public object Target { get; set; }
            public PropertyInfo? PropertyInfo { get; set; }
            public FieldInfo? FieldInfo { get; set; }
            public string MemberName { get; set; } = string.Empty;

            public object? GetValue()
            {
                if (PropertyInfo != null)
                    return PropertyInfo.GetValue(Target);
                if (FieldInfo != null)
                    return FieldInfo.GetValue(Target);
                return null;
            }

            public void SetValue(object? value)
            {
                if (PropertyInfo != null)
                {
                    object? convertedValue = Convert.ChangeType(value, PropertyInfo.PropertyType);
                    PropertyInfo.SetValue(Target, convertedValue);
                }
                else if (FieldInfo != null)
                {
                    object? convertedValue = Convert.ChangeType(value, FieldInfo.FieldType);
                    FieldInfo.SetValue(Target, convertedValue);
                }
            }

            public Type? GetMemberType()
            {
                return PropertyInfo?.PropertyType ?? FieldInfo?.FieldType;
            }
        }

        internal static readonly Dictionary<string, Action<ICustomItem, string[]>> _actionHandlers = new(StringComparer.OrdinalIgnoreCase);

        internal static readonly ConcurrentDictionary<Type, HashSet<string>> _eventArgPropertyCache = new();

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

        public static void Register(string name, Action<ICustomItem, string[]> handler) => _actionHandlers[name] = handler;

        public static void Trigger(ICustomItem customItem, ArgumentType type, EventArgs eventArgs)
        {
            if (customItem.Arguments == null || customItem.Arguments.Count == 0)
                return;

            if (!customItem.Arguments.TryGetValue(type, out string? actionString))
                return;

            foreach (string raw in actionString.Split(['\n', ';'], StringSplitOptions.RemoveEmptyEntries))
            {
                ExecuteAction(customItem, raw.Trim(), eventArgs);
            }
        }

        internal static bool TryGetHandler(string command, out Action<ICustomItem, string[]> handler) => _actionHandlers.TryGetValue(command, out handler);

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

            ExecuteRegularAction(item, action, eventArgs);
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
                    return IsTruthy(condition);
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

        private static void ExecuteRegularAction(ICustomItem item, string action, EventArgs eventArgs)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            if (action.StartsWith("action ", StringComparison.OrdinalIgnoreCase))
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
                    string resolvedValueExpression = ReplacePlaceholders(valueExpression, eventArgs);
                    object root = item != null ? (object)item : eventArgs;
                    SetPropertyWithOperator(root, propertyPath, resolvedValueExpression, operatorType, eventArgs);
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

        private static void SetPropertyWithOperator(object root, string propertyPath, string valueExpression, AssignmentOperator operatorType, EventArgs eventArgs)
        {
            try
            {
                if (ShouldResolveFromEventArgs(propertyPath, eventArgs))
                {
                    object? currentValue = null;
                    if (operatorType != AssignmentOperator.Assign)
                    {
                        string currentValueStr = ResolvePlaceholder(propertyPath, eventArgs);
                        if (currentValueStr != "null" && !currentValueStr.StartsWith("<error:") && !currentValueStr.StartsWith("<invalid:"))
                        {
                            if (double.TryParse(currentValueStr, out double numValue))
                                currentValue = numValue;
                            else
                                currentValue = currentValueStr;
                        }
                    }

                    object? newValue = CalculateNewValue(currentValue, valueExpression, operatorType, propertyPath);

                    SetProperty(eventArgs, propertyPath, newValue, eventArgs);
                    return;
                }

                object? currentValue2 = null;
                if (operatorType != AssignmentOperator.Assign)
                {
                    currentValue2 = GetPropertyValue(root, propertyPath);
                }

                object? newValue2 = CalculateNewValue(currentValue2, valueExpression, operatorType, propertyPath);

                SetProperty(root, propertyPath, newValue2, eventArgs);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to execute compound assignment '{propertyPath}': {ex.Message}");
            }
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

                foreach (Type iface in t.GetInterfaces())
                {
                    foreach (PropertyInfo p in iface.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!string.IsNullOrWhiteSpace(p.Name))
                            set.Add(p.Name);
                    }
                }

                return set;
            });

            return names.Contains(firstPart);
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
                {
                    currentValue = propertyTarget.GetValue();
                }

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

        public static void ExecuteCustomAction(uint actionId, EventArgs eventArgs)
        {
            if (!CustomAction.CustomActions.TryGetValue(actionId, out ICustomAction customAction))
            {
                LogManager.Error($"{nameof(ArgumentManager)}: CustomAction with ID {actionId} not found");
                return;
            }

            ExecuteCustomAction(customAction, eventArgs);
        }

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

        private static object? CalculateNumericValue(object? currentValue, string valueExpression,
            AssignmentOperator operatorType)
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
                    object? convertedValue;
                    if (newValue is string stringValue)
                    {
                        convertedValue = Convert.ChangeType(stringValue, finalProp.PropertyType);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(newValue, finalProp.PropertyType);
                    }
                    finalProp.SetValue(target, convertedValue);
                    return;
                }

                FieldInfo? finalField = target.GetType().GetField(finalPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (finalField != null)
                {
                    object? convertedValue;
                    if (newValue is string stringValue)
                    {
                        convertedValue = Convert.ChangeType(stringValue, finalField.FieldType);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(newValue, finalField.FieldType);
                    }
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
    }
}