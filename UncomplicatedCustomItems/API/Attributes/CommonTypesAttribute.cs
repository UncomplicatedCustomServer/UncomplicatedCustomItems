using System;

namespace UncomplicatedCustomItems.API.Attributes
{

    [AttributeUsage(AttributeTargets.Field)]
    public class CommonTypesAttribute : Attribute
    {
        public Type CommonType { get; }
        
        public CommonTypesAttribute(Type type)
        {
            CommonType = type;
        }
    }
}