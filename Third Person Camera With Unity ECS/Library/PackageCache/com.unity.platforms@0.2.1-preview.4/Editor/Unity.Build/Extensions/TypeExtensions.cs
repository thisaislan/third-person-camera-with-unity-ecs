using System;

namespace Unity.Build
{
    internal static class TypeExtensions
    {
        public static string GetFullyQualifedAssemblyTypeName(this Type type)
        {
            return $"{type}, {type.Assembly.GetName().Name}";
        }
    }
}
