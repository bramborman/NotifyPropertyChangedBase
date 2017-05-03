using System;
#if!NET_40
using System.Reflection;
#endif

namespace NotifyPropertyChangedBase
{
    internal static class Helpers
    {
        internal static void ValidateObjectNotNull(object obj, string parameterName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidateStringNotNullOrWhiteSpace(string str, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Value cannot be white space or null.", parameterName);
            }
        }

        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the 'type' parameter
        internal static bool GetIsValueType(this Type type)
        {
#if !NET_40
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the 'secondType' parameter
        internal static bool GetIsAssignableFrom(this Type type, Type secondType)
        {
#if !NET_40
            return type.GetTypeInfo().IsAssignableFrom(secondType.GetTypeInfo());
#else
            return type.IsAssignableFrom(secondType);
#endif
        }
    }
}
