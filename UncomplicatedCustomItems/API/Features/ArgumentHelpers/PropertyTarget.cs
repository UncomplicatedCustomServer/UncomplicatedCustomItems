using System;
using System.Reflection;

namespace UncomplicatedCustomItems.API.Features.ArgumentHelpers
{
    internal class PropertyTarget
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
}