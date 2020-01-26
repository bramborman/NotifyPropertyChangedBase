// ---------------------------------------------------------------------------------------
// <copyright file="Helpers.cs" company="Marian Dolinský">
// Copyright (c) Marian Dolinský. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

using System;

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
    }
}
