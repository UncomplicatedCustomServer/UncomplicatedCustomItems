using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.Features.ArgumentHelpers
{
    internal class EvaluateConditions
    {
        private static readonly Regex PredicateRegex = new(@"^\s*(?<prop>[A-Za-z0-9_\.]+)\s*(==|=)\s*(?<val>.+)\s*$", RegexOptions.Compiled);

        public static bool EvaluateNumericCondition(string condition, EventArgs eventArgs)
        {
            string[] operators = [" >= ", " <= ", " > ", " < "];
            string? foundOperator = null;
            string[]? parts = null;

            foreach (string op in operators)
            {
                parts = condition.Split([op], StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    foundOperator = op.Trim();
                    break;
                }
            }

            if (foundOperator == null || parts == null)
                return false;

            string leftValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(parts[0].Trim(), eventArgs));
            string rightValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(parts[1].Trim(), eventArgs));

            if (!double.TryParse(leftValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double left) ||
                !double.TryParse(rightValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double right))
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

        public static bool EvaluateEqualityCondition(string condition, EventArgs eventArgs)
        {
            bool isNotEquals = condition.Contains(" != ");
            string op = isNotEquals ? " != " : " == ";
            int idx = condition.IndexOf(op, StringComparison.Ordinal);
            if (idx == -1)
                return false;

            string leftPart = condition.Substring(0, idx).Trim();
            string rightPart = condition.Substring(idx + op.Length).Trim();

            string leftValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(leftPart, eventArgs));
            string rightValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(rightPart, eventArgs));

            bool areEqual = string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
            return isNotEquals ? !areEqual : areEqual;
        }

        public static bool EvaluateContainsCondition(string condition, EventArgs eventArgs)
        {
            int idx = condition.IndexOf(" contains ", StringComparison.OrdinalIgnoreCase);
            if (idx == -1)
                return false;

            string leftPart = condition.Substring(0, idx).Trim();
            string rightPart = condition.Substring(idx + " contains ".Length).Trim();

            object? leftObj = ArgumentManager.ResolvePlaceholderToObject(leftPart, eventArgs) ?? (object)ArgumentManager.ReplacePlaceholders(leftPart, eventArgs);
            string rightLiteral = TrimQuotes(ArgumentManager.ReplacePlaceholders(rightPart, eventArgs));

            if (leftObj is string leftStr)
                return leftStr.IndexOf(rightLiteral, StringComparison.OrdinalIgnoreCase) >= 0;

            if (leftObj is IEnumerable leftEnum && leftObj is not string)
            {
                if (TryParsePredicate(rightPart, out string? predProp, out string? predOp, out string? predValue))
                {
                    foreach (object? el in leftEnum)
                    {
                        if (el == null)
                            continue;

                        if (TryGetMemberStringValue(el, predProp, out string? memberVal))
                        {
                            if (CompareStrings(memberVal, predValue, predOp))
                                return true;
                        }
                    }
                    return false;
                }
                else
                {
                    foreach (object? el in leftEnum)
                    {
                        if (el == null)
                            continue;

                        if (string.Equals(el.ToString(), rightLiteral, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    return false;
                }
            }

            return string.Equals(leftObj?.ToString(), rightLiteral, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EvaluateIsCondition(string condition, EventArgs eventArgs)
        {
            int idx = condition.IndexOf(" is ", StringComparison.OrdinalIgnoreCase);
            if (idx == -1)
                return false;

            string leftRaw = condition.Substring(0, idx).Trim();
            string rightRaw = condition.Substring(idx + 4).Trim();

            string leftValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(leftRaw, eventArgs));
            string rightValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(rightRaw, eventArgs));

            return string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EvaluateEqualsCondition(string condition, EventArgs eventArgs)
        {
            int idx = condition.IndexOf(" equals ", StringComparison.OrdinalIgnoreCase);
            if (idx == -1)
                return false;

            string leftRaw = condition.Substring(0, idx).Trim();
            string rightRaw = condition.Substring(idx + 8).Trim();

            string leftValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(leftRaw, eventArgs));
            string rightValue = TrimQuotes(ArgumentManager.ReplacePlaceholders(rightRaw, eventArgs));

            return string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
        }
        
        internal static string TrimQuotes(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (s.Length >= 2 && ((s.StartsWith("\"") && s.EndsWith("\"")) || (s.StartsWith("'") && s.EndsWith("'"))))
                return s.Substring(1, s.Length - 2);

            return s;
        }

        internal static bool TryParsePredicate(string token, out string property, out string op, out string value)
        {
            property = op = value = string.Empty;
            Match m = PredicateRegex.Match(token);
            if (!m.Success)
                return false;

            property = m.Groups["prop"].Value;
            op = m.Groups[2].Value;
            value = TrimQuotes(m.Groups["val"].Value.Trim());
            return true;
        }

        internal static bool TryGetMemberStringValue(object target, string memberPath, out string? value)
        {
            value = null;
            if (target == null || string.IsNullOrWhiteSpace(memberPath))
                return false;

            try
            {
                object? current = target;
                foreach (string part in memberPath.Split('.'))
                {
                    if (current == null)
                        return false;

                    Type t = current.GetType();
                    PropertyInfo? prop = t.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        current = prop.GetValue(current);
                        continue;
                    }

                    FieldInfo? field = t.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null)
                    {
                        current = field.GetValue(current);
                        continue;
                    }

                    return false;
                }

                value = current?.ToString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool CompareStrings(string? left, string right, string op)
        {
            if (left == null)
                return false;

            if (op == "==" || op == "=")
                return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

            return false;
        }
    }
}