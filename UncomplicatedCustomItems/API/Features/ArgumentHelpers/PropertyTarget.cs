using System;
using System.Reflection;

namespace UncomplicatedCustomItems.API.Features.ArgumentHelpers
{
    internal class PropertyTarget
    {
        public object? Target { get; set; }
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
                PropertyInfo.SetValue(Target, SafeConvert(value, PropertyInfo.PropertyType));
                return;
            }

            FieldInfo?.SetValue(Target, SafeConvert(value, FieldInfo.FieldType));
        }

        private static object? SafeConvert(object? value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            if (value is string s && targetType.IsEnum)
                return Enum.Parse(targetType, s, ignoreCase: true);

            if (value is string str)
            {
                try
                {
                    return Convert.ChangeType(str, targetType);
                }
                catch
                {
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                }
            }

            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value;
            }
        }

        public Type? GetMemberType()
        {
            return PropertyInfo?.PropertyType ?? FieldInfo?.FieldType;
        }
    }
}