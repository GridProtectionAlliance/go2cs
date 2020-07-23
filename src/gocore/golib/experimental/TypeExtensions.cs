//******************************************************************************************************
//  TypeHelpers.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/05/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA1031

namespace go.experimental
{
    /// <summary>
    /// Defines type related helper functions.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Attempts to find the best precedence-wise matching extension method called <paramref name="methodName"/> for the <paramref name="targetType"/> or
        /// for any of its promotions, see <see cref="PromotedStructAttribute"/>.
        /// </summary>
        /// <param name="targetType">Target <see cref="Type"/> to search.</param>
        /// <param name="methodName">Name of extension method to find.</param>
        /// <returns>Method metadata of extension method, <paramref name="methodName"/>, for <paramref name="targetType"/> if found; otherwise, <c>null</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo? GetExtensionMethodSearchingPromotions(this Type targetType, string methodName)
        {
            MethodInfo? method = targetType.GetExtensionMethod(methodName);

            if (method is null)
            {
                object[] customAttributes = targetType.GetCustomAttributes(typeof(PromotedStructAttribute), true);

                if (customAttributes.Length > 0)
                {
                    foreach (PromotedStructAttribute attribute in customAttributes.Cast<PromotedStructAttribute>())
                    {
                        method = attribute.PromotedType.GetExtensionMethodSearchingPromotions(methodName);

                        if (!(method is null))
                            break;
                    }
                }
            }

            return method;
        }

        // FYI:
        //       IsUnmanaged<bool>() is true, but
        //       IsBlittable<bool>() is false

        // ReSharper disable once UnusedTypeParameter
        private class UnmanagedTypeTester<T> where T : unmanaged { }

        /// <summary>
        /// Determines if type is unmanaged.
        /// </summary>
        /// <typeparam name="T">Type to check.</typeparam>
        /// <returns><c>true</c> if type is unmanaged; otherwise, <c>false</c>.</returns>
        public static bool IsUnmanaged<T>() => IsUnmanaged(typeof(T));

        /// <summary>
        /// Determines if type is unmanaged.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns><c>true</c> if type is unmanaged; otherwise, <c>false</c>.</returns>
        public static bool IsUnmanaged(this Type type)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                typeof(UnmanagedTypeTester<>).MakeGenericType(type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if type is blittable, i.e., can be instantiated via <see cref="GCHandle.Alloc(object, GCHandleType)"/>
        /// when <see cref="GCHandleType"/> parameter is <see cref="GCHandleType.Pinned"/>.
        /// </summary>
        /// <typeparam name="T">Type to check.</typeparam>
        /// <returns><c>true</c> if type is blittable; otherwise, <c>false</c>.</returns>
        public static bool IsBlittable<T>() where T : new()
        {
            try
            {
                GCHandle.Alloc(new T(), GCHandleType.Pinned).Free();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
