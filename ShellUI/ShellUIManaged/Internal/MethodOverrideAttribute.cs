using System;

namespace Fusion.Internal
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodOverrideAttribute : Attribute
    {
        public Type TargetType { get; }
        public string TargetMethodName { get; }
        public string DelegateFieldName { get; set; }

        public MethodOverrideAttribute(Type targetType, string targetMethodName = null, string delegateFieldName = null)
        {
            TargetType = targetType;
            TargetMethodName = targetMethodName;
            DelegateFieldName = delegateFieldName;
        }
    }
}