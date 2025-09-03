using System;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.Features.ArgumentHelpers
{
    #nullable enable
    internal class EvaluateConditions
    {
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

            string leftValue = ArgumentManager.ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ArgumentManager.ReplacePlaceholders(parts[1].Trim(), eventArgs);

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

        public static bool EvaluateEqualityCondition(string condition, EventArgs eventArgs)
        {
            bool isNotEquals = condition.Contains(" != ");
            string[] parts = condition.Split([isNotEquals ? " != " : " == "], StringSplitOptions.None);
            if (parts.Length != 2)
                return false;

            string leftValue = ArgumentManager.ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ArgumentManager.ReplacePlaceholders(parts[1].Trim().Trim('"', '\''), eventArgs);

            bool areEqual = string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
            return isNotEquals ? !areEqual : areEqual;
        }

        public static bool EvaluateContainsCondition(string condition, EventArgs eventArgs)
        {
            string[] parts = condition.Split([" contains "], (int)StringSplitOptions.None, (StringSplitOptions)StringComparison.OrdinalIgnoreCase);
            if (parts.Length != 2)
                return false;

            string leftValue = ArgumentManager.ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ArgumentManager.ReplacePlaceholders(parts[1].Trim().Trim('"', '\''), eventArgs);

            return leftValue.Contains(rightValue);
        }

        public static bool EvaluateIsCondition(string condition, EventArgs eventArgs)
        {
            string[] parts = condition.Split([" is "], (int)StringSplitOptions.None, (StringSplitOptions)StringComparison.OrdinalIgnoreCase);
            if (parts.Length != 2)
                return false;

            string leftValue = ArgumentManager.ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = parts[1].Trim().Trim('"', '\'');

            return string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EvaluateEqualsCondition(string condition, EventArgs eventArgs)
        {
            string[] parts = condition.Split([" equals "], (int)StringSplitOptions.None, (StringSplitOptions)StringComparison.OrdinalIgnoreCase);
            if (parts.Length != 2)
                return false;

            string leftValue = ArgumentManager.ReplacePlaceholders(parts[0].Trim(), eventArgs);
            string rightValue = ArgumentManager.ReplacePlaceholders(parts[1].Trim().Trim('"', '\''), eventArgs);

            return string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}