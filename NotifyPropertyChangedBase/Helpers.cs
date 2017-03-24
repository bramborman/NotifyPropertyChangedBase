using System;
using System.Linq;
#if WINDOWS_UWP
using System.Reflection;
#endif

namespace NotifyPropertyChangedBase
{
    internal static class Helpers
    {
        internal static void ValidateNotNull(object obj, string parameterName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidateNotNullOrWhiteSpace(string str, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Value cannot be white space or null.", parameterName);
            }
        }

        internal static bool GetIsValueType(this Type type)
        {
#if WINDOWS_UWP
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        internal static bool GetIsSubclassOf(this Type type, Type baseClass)
        {
#if WINDOWS_UWP
            return type.GetTypeInfo().IsSubclassOf(baseClass);
#else
            return type.IsSubclassOf(baseClass);
#endif
        }

        internal static bool ContainsGenericParameter(this Type type, Type constraint)
        {
#if WINDOWS_UWP
            return type.GetTypeInfo().GenericTypeArguments.Contains(constraint);
#else
            return type.GetGenericArguments().Contains(constraint);
#endif
        }
    }
}
